using System.Runtime.InteropServices;
using Havok;
using VRage;
using VRage.Game.Components;
using VRage.Game.Models;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace CringeLauncher.Patches.Dedicated;

internal sealed class HullPruningStructure : IMyTriangePruningStructure
{
    private const float Epsilon = 1e-7f;
    private const float AxisEpsilon = 1e-4f;
    private const float PlaneEpsilon = 1e-5f;
    private const int MaxContainerDepth = 8;

    private readonly List<Vector3> _vertices = [];
    private readonly List<Plane> _planes = [];
    private readonly List<int> _faceIndices = [];
    private readonly List<Volume> _volumes = [];
    private readonly List<Plane> _planeScratch = [];

    private BoundingBox _bounds = BoundingBox.CreateInvalid();

    public int Size => _vertices.Count;

    private ReadOnlySpan<Vector3> Vertices => CollectionsMarshal.AsSpan(_vertices);
    private ReadOnlySpan<Plane> Planes => CollectionsMarshal.AsSpan(_planes);
    private ReadOnlySpan<int> FaceIndices => CollectionsMarshal.AsSpan(_faceIndices);
    private ReadOnlySpan<Volume> Volumes => CollectionsMarshal.AsSpan(_volumes);

    #region Building

    private enum VolumeKind : byte
    {
        /// <summary>A convex hull given by a vertex span, queried by support function (fallback).</summary>
        Hull,

        /// <summary>A convex hull with face planes and triangles, queried analytically.</summary>
        Polytope,

        /// <summary>An axis-aligned box: a centre vertex plus half extents.</summary>
        Box,

        /// <summary>A ball (a single vertex and a radius).</summary>
        Ball,

        /// <summary>A capsule: a segment and a radius.</summary>
        Capsule
    }

    private readonly struct Volume(
        VolumeKind kind,
        int vertexStart,
        int vertexCount,
        float radius,
        BoundingBox bounds,
        Vector3 extents = default,
        int faceStart = 0,
        int faceCount = 0,
        int planeStart = 0,
        int planeCount = 0)
    {
        public readonly VolumeKind Kind = kind;
        public readonly int VertexStart = vertexStart;
        public readonly int VertexCount = vertexCount;
        public readonly float Radius = radius;
        public readonly BoundingBox Bounds = bounds;
        public readonly Vector3 Extents = extents;
        public readonly int FaceStart = faceStart;
        public readonly int FaceCount = faceCount;
        public readonly int PlaneStart = planeStart;
        public readonly int PlaneCount = planeCount;
    }

    public void Collect(HkShape[] collisionShapes)
    {
        var identity = Matrix.Identity;
        foreach (var shape in collisionShapes)
            Collect(shape, ref identity, 0);
    }

    private void Collect(HkShape shape, ref Matrix transform, int depth)
    {
        if (depth > MaxContainerDepth || !shape.IsValid)
            return;

        switch (shape.ShapeType)
        {
            case HkShapeType.ConvexVertices:
            {
                AddHull((HkConvexVerticesShape)shape, ref transform);
                break;
            }
            case HkShapeType.Box:
            {
                AddBox((HkBoxShape)shape, ref transform);
                break;
            }
            case HkShapeType.Sphere:
            {
                var sphere = (HkSphereShape)shape;
                AddBall(Vector3.Zero, Vector3.Zero, sphere.Radius, ref transform);
                break;
            }
            case HkShapeType.Capsule:
            {
                var capsule = (HkCapsuleShape)shape;
                AddBall(capsule.VertexA, capsule.VertexB, capsule.Radius, ref transform);
                break;
            }
            case HkShapeType.Cylinder:
            {
                var cylinder = (HkCylinderShape)shape;
                AddBall(cylinder.VertexA, cylinder.VertexB, cylinder.Radius, ref transform);
                break;
            }
            case HkShapeType.ConvexTransform:
            {
                var wrapper = (HkConvexTransformShape)shape;
                var local = wrapper.Transform;
                Matrix.Multiply(ref local, ref transform, out var combined);
                Collect(wrapper.ChildShape, ref combined, depth + 1);
                break;
            }
            case HkShapeType.ConvexTranslate:
            {
                var wrapper = (HkConvexTranslateShape)shape;
                var local = Matrix.CreateTranslation(wrapper.Translation);
                Matrix.Multiply(ref local, ref transform, out var combined);
                Collect(wrapper.ChildShape, ref combined, depth + 1);
                break;
            }
            case HkShapeType.Transform:
            {
                var wrapper = (HkTransformShape)shape;
                var local = wrapper.Transform;
                Matrix.Multiply(ref local, ref transform, out var combined);
                Collect(wrapper.ChildShape, ref combined, depth + 1);
                break;
            }
            case HkShapeType.List:
            {
                var list = (HkListShape)shape;
                var children = list.TotalChildrenCount;
                for (var i = 0; i < children; i++)
                    Collect(list.GetChildByIndex(i), ref transform, depth + 1);
                break;
            }
            case HkShapeType.StaticCompound:
            {
                var compound = (HkStaticCompoundShape)shape;
                var instances = compound.InstanceCount;
                for (var i = 0; i < instances; i++)
                {
                    var local = compound.GetInstanceTransform(i);
                    Matrix.Multiply(ref local, ref transform, out var combined);
                    Collect(compound.GetInstance(i), ref combined, depth + 1);
                }

                break;
            }
            case HkShapeType.Collection:
            {
                CollectCollection((HkShapeCollection)shape, ref transform, depth);
                break;
            }
            case HkShapeType.Mopp:
            {
                CollectCollection(((HkMoppBvTreeShape)shape).ShapeCollection, ref transform, depth);
                break;
            }
        }
    }

    private void CollectCollection(HkShapeCollection collection, ref Matrix transform, int depth)
    {
        var count = collection.ShapeCount;
        for (var i = 0; i < count; i++)
            Collect(collection.GetShape((uint)i, null), ref transform, depth + 1);
    }

    private void AddHull(HkConvexVerticesShape shape, ref Matrix transform)
    {
        using HkGeometry geometry = new();
        shape.GetGeometry(geometry, out _);

        var triangleCount = geometry.TriangleCount;
        var vertexCount = geometry.VertexCount;
        if (triangleCount <= 0 || vertexCount < 4)
        {
            shape.GetVertices(out var vertices);
            AddCloud(vertices, ref transform);
            return;
        }

        // Havok rounds a convex shape by its convex radius.
        var margin = shape.Base.ConvexRadius;
        var vertexStart = _vertices.Count;
        var centroid = Vector3.Zero;
        var bounds = BoundingBox.CreateInvalid();
        for (var i = 0; i < vertexCount; i++)
        {
            var vertex = Vector3.Transform(geometry.GetVertex(i), ref transform);
            _vertices.Add(vertex);
            centroid += vertex;
            bounds = bounds.Include(vertex);
        }

        centroid /= vertexCount;

        var faceStart = _faceIndices.Count / 3;
        var planeStart = _planes.Count;
        var scratch = _planeScratch;
        scratch.Clear();

        for (var triangle = 0; triangle < triangleCount; triangle++)
        {
            geometry.GetTriangle(triangle, out var i0, out var i1, out var i2, out _);
            var a = _vertices[vertexStart + i0];
            var b = _vertices[vertexStart + i1];
            var c = _vertices[vertexStart + i2];

            var normal = Vector3.Cross(b - a, c - a);
            var lengthSquared = normal.LengthSquared();
            if (lengthSquared <= Epsilon)
                continue;

            normal /= MathF.Sqrt(lengthSquared);
            if (Vector3.Dot(normal, a - centroid) < 0f)
                normal = -normal;

            _faceIndices.Add(vertexStart + i0);
            _faceIndices.Add(vertexStart + i1);
            _faceIndices.Add(vertexStart + i2);
            scratch.Add(new(normal, -Vector3.Dot(normal, a) - margin));
        }

        scratch.Sort(PlaneComparer.Instance);
        for (var i = 0; i < scratch.Count; i++)
        {
            if (i > 0 && SamePlane(scratch[i - 1], scratch[i]))
                continue;

            _planes.Add(scratch[i]);
        }

        var planeCount = _planes.Count - planeStart;
        var faceCount = _faceIndices.Count / 3 - faceStart;
        if (planeCount < 4 || faceCount == 0 || !IsSolid(bounds, margin))
        {
            _planes.RemoveRange(planeStart, planeCount);
            _faceIndices.RemoveRange(faceStart * 3, faceCount * 3);
            _vertices.RemoveRange(vertexStart, vertexCount);
            shape.GetVertices(out var vertices);
            AddCloud(vertices, ref transform);
            return;
        }

        _bounds = _bounds.Include(bounds);
        _volumes.Add(new(VolumeKind.Polytope, vertexStart, vertexCount, margin, bounds, default,
            faceStart, faceCount, planeStart, planeCount));
    }

    private void AddCloud(ReadOnlySpan<Vector3> vertices, ref Matrix transform, float margin = 0f)
    {
        if (vertices.Length < 4)
            return;

        var start = _vertices.Count;
        var bounds = BoundingBox.CreateInvalid();
        _vertices.EnsureCapacity(_vertices.Count + vertices.Length);
        foreach (var v in vertices)
        {
            var vertex = Vector3.Transform(v, ref transform);
            _vertices.Add(vertex);
            bounds = bounds.Include(vertex);
        }

        _bounds = _bounds.Include(bounds);
        _volumes.Add(new(VolumeKind.Hull, start, vertices.Length, margin, bounds));
    }

    private void AddBox(HkBoxShape shape, ref Matrix transform)
    {
        var halfExtents = shape.HalfExtents;
        var margin = shape.Base.ConvexRadius;
        if (IsAxisAligned(ref transform))
        {
            Vector3 extents = new(halfExtents.X * transform.Right.Length() + margin,
                halfExtents.Y * transform.Up.Length() + margin,
                halfExtents.Z * transform.Forward.Length() + margin);
            var center = transform.Translation;

            _vertices.Add(center);
            BoundingBox bounds = new(center - extents, center + extents);
            _bounds = _bounds.Include(bounds);
            _volumes.Add(new(VolumeKind.Box, _vertices.Count - 1, 1, 0f, bounds, extents));
            return;
        }

        Vector3 x = new(halfExtents.X, 0f, 0f);
        Vector3 y = new(0f, halfExtents.Y, 0f);
        Vector3 z = new(0f, 0f, halfExtents.Z);

        ReadOnlySpan<Vector3> corners =
        [
            -x - y - z, -x - y + z, -x + y + z, -x + y - z,
            x - y - z, x - y + z, x + y + z, x + y - z
        ];

        AddCloud(corners, ref transform, margin);
    }

    private static bool SamePlane(Plane a, Plane b) =>
        Vector3.Dot(a.Normal, b.Normal) > 1f - PlaneEpsilon && MathF.Abs(a.D - b.D) <= PlaneEpsilon;
    
    private sealed class PlaneComparer : IComparer<Plane>
    {
        public static readonly PlaneComparer Instance = new();

        public int Compare(Plane x, Plane y)
        {
            var order = x.Normal.X.CompareTo(y.Normal.X);
            if (order != 0)
                return order;

            order = x.Normal.Y.CompareTo(y.Normal.Y);
            if (order != 0)
                return order;

            order = x.Normal.Z.CompareTo(y.Normal.Z);
            return order != 0 ? order : x.D.CompareTo(y.D);
        }
    }

    private static bool IsSolid(BoundingBox bounds, float margin)
    {
        var size = bounds.Max - bounds.Min;
        var tolerance = MathF.Max(1e-4f, margin * 0.5f);
        return size.X > tolerance && size.Y > tolerance && size.Z > tolerance;
    }

    private static bool IsAxisAligned(ref Matrix transform) =>
        MathF.Abs(transform.Right.Y) <= AxisEpsilon && MathF.Abs(transform.Right.Z) <= AxisEpsilon &&
        MathF.Abs(transform.Up.X) <= AxisEpsilon && MathF.Abs(transform.Up.Z) <= AxisEpsilon &&
        MathF.Abs(transform.Forward.X) <= AxisEpsilon && MathF.Abs(transform.Forward.Y) <= AxisEpsilon;

    private void AddBall(Vector3 from, Vector3 to, float radius, ref Matrix transform)
    {
        var scaledRadius = radius * ScaleOf(ref transform);
        var start = _vertices.Count;
        var first = Vector3.Transform(from, ref transform);
        _vertices.Add(first);

        var bounds = BoundingBox.CreateInvalid().Include(first - new Vector3(scaledRadius))
            .Include(first + new Vector3(scaledRadius));

        if (from == to)
        {
            _bounds = _bounds.Include(bounds);
            _volumes.Add(new(VolumeKind.Ball, start, 1, scaledRadius, bounds));
            return;
        }

        var second = Vector3.Transform(to, ref transform);
        _vertices.Add(second);
        bounds = bounds.Include(second - new Vector3(scaledRadius)).Include(second + new Vector3(scaledRadius));

        _bounds = _bounds.Include(bounds);
        _volumes.Add(new(VolumeKind.Capsule, start, 2, scaledRadius, bounds));
    }

    private static float ScaleOf(ref Matrix transform) =>
        MathF.Max(transform.Right.Length(), MathF.Max(transform.Up.Length(), transform.Forward.Length()));

    #endregion

    #region Queries

    public MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(IMyEntity entity, ref LineD line,
        IntersectionFlags flags = IntersectionFlags.DIRECT_TRIANGLES)
    {
        var worldVolume = entity.PositionComp.WorldVolume;
        if (!MyUtils.IsLineIntersectingBoundingSphere(ref line, ref worldVolume))
            return null;

        var customInvMatrix = entity.PositionComp.WorldMatrixInvScaled;
        return GetIntersectionWithLine(entity, ref line, ref customInvMatrix, flags);
    }

    public MyIntersectionResultLineTriangleEx? GetIntersectionWithLine(IMyEntity entity, ref LineD line,
        ref MatrixD customInvMatrix, IntersectionFlags flags = IntersectionFlags.DIRECT_TRIANGLES)
    {
        Line local = new(Vector3D.Transform(line.From, ref customInvMatrix), Vector3D.Transform(line.To, ref customInvMatrix));
        if (!TryFindClosestHit(ref local, (flags & IntersectionFlags.FLIPPED_TRIANGLES) == 0, out var distance,
                out var normal, out var volumeIndex))
            return null;

        var point = local.From + local.Direction * distance;
        var triangle = ResultTriangle(volumeIndex, point, normal);
        MyIntersectionResultLineTriangle result = new(0, ref triangle, ref normal, distance);
        return new MyIntersectionResultLineTriangleEx(result, entity, ref local);
    }

    public void GetTrianglesIntersectingLine(IMyEntity entity, ref LineD line, IntersectionFlags flags,
        List<MyIntersectionResultLineTriangleEx> result)
    {
        var customInvMatrix = entity.PositionComp.WorldMatrixNormalizedInv;
        GetTrianglesIntersectingLine(entity, ref line, ref customInvMatrix, flags, result);
    }

    public void GetTrianglesIntersectingLine(IMyEntity entity, ref LineD line, ref MatrixD customInvMatrix,
        IntersectionFlags flags, List<MyIntersectionResultLineTriangleEx> result)
    {
        Line local = new(Vector3D.Transform(line.From, ref customInvMatrix), Vector3D.Transform(line.To, ref customInvMatrix));
        var frontFacing = (flags & IntersectionFlags.FLIPPED_TRIANGLES) == 0;
        var start = result.Count;

        var volumes = Volumes;
        for (var v = 0; v < volumes.Length; v++)
        {
            var volume = volumes[v];
            if (!local.BoundingBox.Intersects(volume.Bounds))
                continue;

            if (!TryRay(volume, ref local, frontFacing, out var distance, out var normal))
                continue;

            var point = local.From + local.Direction * distance;
            var triangle = SyntheticTriangle(point, normal);
            MyIntersectionResultLineTriangle hit = new(v, ref triangle, ref normal, distance);
            result.Add(new(hit, entity, ref local));
        }

        var added = result.Count - start;
        if (added > 1)
            result.Sort(start, added, DistanceComparer.Instance);
    }

    public void GetTrianglesIntersectingSphere(ref BoundingSphere sphere, Vector3? referenceNormalVector, float? maxAngle,
        List<MyTriangle_Vertex_Normals> retTriangles, int maxNeighbourTriangles)
    {
        if (retTriangles.Count == maxNeighbourTriangles)
            return;

        var indices = FaceIndices;
        var volumes = Volumes;
        var sphereBounds = BoundingBox.CreateInvalid().Include(ref sphere);

        foreach (var volume in volumes)
        {
            if (volume.Kind != VolumeKind.Polytope || !volume.Bounds.Intersects(sphereBounds))
                continue;

            for (var i = 0; i < volume.FaceCount; i++)
            {
                var face = volume.FaceStart + i;
                var a = _vertices[indices[face * 3]];
                var b = _vertices[indices[face * 3 + 1]];
                var c = _vertices[indices[face * 3 + 2]];
                var triangle = BoundingBox.CreateInvalid().Include(a, b, c);
                if (!triangle.Intersects(sphere))
                    continue;

                var normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
                if (referenceNormalVector.HasValue && maxAngle.HasValue &&
                    MyUtils.GetAngleBetweenVectors(referenceNormalVector.Value, normal) > maxAngle.Value)
                    continue;

                retTriangles.Add(new()
                {
                    Vertices = new() { Vertex0 = a, Vertex1 = b, Vertex2 = c },
                    Normals = new() { Normal0 = normal, Normal1 = normal, Normal2 = normal }
                });

                if (retTriangles.Count == maxNeighbourTriangles)
                    return;
            }
        }
    }

    public bool GetIntersectionWithSphere(IMyEntity physObject, ref BoundingSphereD sphere)
    {
        var matrix = physObject.PositionComp.WorldMatrixNormalizedInv;
        Vector3 center = Vector3D.Transform(sphere.Center, ref matrix);
        BoundingSphere localSphere = new(center, (float)sphere.Radius);
        return GetIntersectionWithSphere(ref localSphere);
    }

    public bool GetIntersectionWithSphere(ref BoundingSphere localSphere)
    {
        if (!_bounds.Intersects(localSphere))
            return false;

        var volumes = Volumes;
        foreach (var volume in volumes)
        {
            if (!volume.Bounds.Intersects(localSphere))
                continue;

            switch (volume.Kind)
            {
                case VolumeKind.Hull:
                case VolumeKind.Polytope:
                    if (SupportMapping.Distance(BodyOf(in volume), SupportMapping.Body.Ball(localSphere.Center, localSphere.Radius),
                            out _, out _) <= 0f)
                        return true;
                    break;

                case VolumeKind.Box:
                    if (SquaredPointBox(volume.Bounds, localSphere.Center) <= Squared(localSphere.Radius))
                        return true;
                    break;

                case VolumeKind.Ball:
                    if (Vector3.DistanceSquared(localSphere.Center, _vertices[volume.VertexStart]) <=
                        Squared(localSphere.Radius + volume.Radius))
                        return true;
                    break;

                case VolumeKind.Capsule:
                    if (Vector3.DistanceSquared(localSphere.Center,
                            ClosestPointOnSegment(_vertices[volume.VertexStart], _vertices[volume.VertexStart + 1],
                                localSphere.Center)) <= Squared(localSphere.Radius + volume.Radius))
                        return true;
                    break;
            }
        }

        return false;
    }

    public bool GetIntersectionWithAABB(IMyEntity physObject, ref BoundingBoxD aabb)
    {
        var matrix = physObject.PositionComp.WorldMatrixNormalizedInv;
        var center = Vector3D.Transform(aabb.Center, ref matrix);
        var local = aabb;
        local.Translate(center - aabb.Center);

        BoundingBox box = new(local.Min, local.Max);
        if (!_bounds.Intersects(box))
            return false;

        Span<Vector3> corners = stackalloc Vector3[8];
        var volumes = Volumes;
        foreach (var volume in volumes)
        {
            if (!volume.Bounds.Intersects(box))
                continue;

            switch (volume.Kind)
            {
                case VolumeKind.Polytope:
                {
                    var separated = false;
                    for (var plane = volume.PlaneStart; plane < volume.PlaneStart + volume.PlaneCount; plane++)
                    {
                        if (PlaneSeparatesBox(plane, ref box))
                        {
                            separated = true;
                            break;
                        }
                    }

                    if (separated)
                        break;

                    FillCorners(in box, corners);
                    if (SupportMapping.Distance(BodyOf(in volume), SupportMapping.Body.Cloud(corners), out _, out _) <= 0f)
                        return true;
                    break;
                }
                case VolumeKind.Hull:
                {
                    FillCorners(in box, corners);
                    if (SupportMapping.Distance(BodyOf(in volume), SupportMapping.Body.Cloud(corners), out _, out _) <= 0f)
                        return true;
                    break;
                }
                case VolumeKind.Box:
                    if (volume.Bounds.Intersects(box))
                        return true;
                    break;
                case VolumeKind.Ball:
                {
                    var point = _vertices[volume.VertexStart];
                    if (Vector3.DistanceSquared(point, Vector3.Clamp(point, box.Min, box.Max)) <= Squared(volume.Radius))
                        return true;
                    break;
                }
                case VolumeKind.Capsule:
                    if (SquaredDistanceToBox(ref box, _vertices[volume.VertexStart], _vertices[volume.VertexStart + 1]) <=
                        Squared(volume.Radius))
                        return true;
                    break;
            }
        }

        return false;
    }

    public void GetTrianglesIntersectingAABB(ref BoundingBox aabb, List<MyTriangle_Vertex_Normal> retTriangles,
        int maxNeighbourTriangles)
    {
        if (retTriangles.Count == maxNeighbourTriangles || !_bounds.Intersects(aabb))
            return;

        var indices = FaceIndices;
        var volumes = Volumes;
        foreach (var volume in volumes)
        {
            if (volume.Kind != VolumeKind.Polytope || !volume.Bounds.Intersects(aabb))
                continue;

            for (var i = 0; i < volume.FaceCount; i++)
            {
                var face = volume.FaceStart + i;
                var a = _vertices[indices[face * 3]];
                var b = _vertices[indices[face * 3 + 1]];
                var c = _vertices[indices[face * 3 + 2]];
                if (!BoundingBox.CreateInvalid().Include(a, b, c).Intersects(aabb))
                    continue;

                retTriangles.Add(new()
                {
                    Vertexes = new() { Vertex0 = a, Vertex1 = b, Vertex2 = c },
                    Normal = Vector3.Forward
                });

                if (retTriangles.Count == maxNeighbourTriangles)
                    return;
            }
        }
    }

    public void Close()
    {
    }

    private SupportMapping.Body BodyOf(in Volume volume) =>
        volume.Kind switch
        {
            VolumeKind.Ball => SupportMapping.Body.Ball(_vertices[volume.VertexStart], volume.Radius),
            VolumeKind.Capsule => SupportMapping.Body.Segment(_vertices[volume.VertexStart],
                _vertices[volume.VertexStart + 1], volume.Radius),
            _ => SupportMapping.Body.Cloud(Vertices.Slice(volume.VertexStart, volume.VertexCount), volume.Radius)
        };

    private bool TryFindClosestHit(ref Line line, bool frontFacing, out float distance, out Vector3 normal,
        out int volumeIndex)
    {
        distance = 0f;
        normal = default;
        volumeIndex = -1;

        var found = false;
        var closest = float.MaxValue;
        Vector3 closestNormal = default;
        var closestVolume = -1;

        var volumes = Volumes;
        for (var v = 0; v < volumes.Length; v++)
        {
            var volume = volumes[v];
            if (!line.BoundingBox.Intersects(volume.Bounds))
                continue;

            if (!TryRay(volume, ref line, frontFacing, out var candidate, out var candidateNormal) ||
                candidate >= closest)
                continue;

            closest = candidate;
            closestNormal = candidateNormal;
            closestVolume = v;
            found = true;
        }

        distance = closest;
        normal = closestNormal;
        volumeIndex = closestVolume;
        return found;
    }

    private MyTriangle_Vertices ResultTriangle(int volumeIndex, Vector3 point, Vector3 normal)
    {
        if (volumeIndex >= 0)
        {
            var volume = Volumes[volumeIndex];
            if (volume.Kind == VolumeKind.Polytope && TryFaceTriangle(in volume, point, out var face, out _))
                return face;
        }

        return SyntheticTriangle(point, normal);
    }

    private bool TryFaceTriangle(in Volume volume, Vector3 point, out MyTriangle_Vertices triangle, out int faceIndex)
    {
        var indices = FaceIndices;
        var interior = (volume.Bounds.Min + volume.Bounds.Max) * 0.5f;
        triangle = default;
        faceIndex = -1;

        for (var i = 0; i < volume.FaceCount; i++)
        {
            var face = volume.FaceStart + i;
            var a = _vertices[indices[face * 3]];
            var b = _vertices[indices[face * 3 + 1]];
            var c = _vertices[indices[face * 3 + 2]];
            var normal = Vector3.Cross(b - a, c - a);
            if (normal.LengthSquared() <= Epsilon)
                continue;

            if (Vector3.Dot(Vector3.Cross(b - a, point - a), normal) < 0f ||
                Vector3.Dot(Vector3.Cross(c - b, point - b), normal) < 0f ||
                Vector3.Dot(Vector3.Cross(a - c, point - c), normal) < 0f)
                continue;

            var outward = Vector3.Dot(normal, a - interior) >= 0f;
            triangle = outward
                ? new() { Vertex0 = a, Vertex1 = c, Vertex2 = b }
                : new MyTriangle_Vertices { Vertex0 = a, Vertex1 = b, Vertex2 = c };
            faceIndex = face;
            return true;
        }

        return false;
    }

    private bool ClipPolytope(ref Line line, in Volume volume, bool frontFacing, out float distance, out Vector3 normal) =>
        SupportMapping.ClipConvex(Planes.Slice(volume.PlaneStart, volume.PlaneCount), line.From, line.Direction,
            line.Length, frontFacing, out distance, out normal);

    private bool PlaneSeparatesBox(int planeIndex, ref BoundingBox box) =>
        SupportMapping.PlaneSeparatesBox(_planes[planeIndex], box.Min, box.Max);

    private bool TryRay(in Volume volume, ref Line line, bool frontFacing, out float distance, out Vector3 normal) =>
        volume.Kind switch
        {
            VolumeKind.Hull => SupportMapping.Raycast(BodyOf(in volume), line.From, line.Direction, line.Length,
                frontFacing, out distance, out normal),
            VolumeKind.Polytope => ClipPolytope(ref line, in volume, frontFacing, out distance, out normal),
            VolumeKind.Box => RayBox(ref line, _vertices[volume.VertexStart], volume.Extents, frontFacing, out distance,
                out normal),
            VolumeKind.Ball => RayBall(ref line, _vertices[volume.VertexStart], volume.Radius, frontFacing,
                out distance, out normal),
            _ => RayCapsule(ref line, _vertices[volume.VertexStart], _vertices[volume.VertexStart + 1], volume.Radius,
                frontFacing, out distance, out normal)
        };

    private static bool RayBall(ref Line line, Vector3 center, float radius, bool frontFacing, out float distance,
        out Vector3 normal)
    {
        distance = 0f;
        normal = default;

        var offset = line.From - center;
        var b = Vector3.Dot(offset, line.Direction);
        var c = Vector3.Dot(offset, offset) - radius * radius;
        var discriminant = b * b - c;
        if (discriminant < 0f)
            return false;

        var root = MathF.Sqrt(discriminant);
        var t = frontFacing ? -b - root : -b + root;
        if (t < 0f || t > line.Length)
            return false;

        distance = t;
        normal = (line.From + line.Direction * t - center) / radius;
        return true;
    }

    private static bool RayCapsule(ref Line line, Vector3 from, Vector3 to, float radius, bool frontFacing,
        out float distance, out Vector3 normal)
    {
        var axis = to - from;
        var axisLength = axis.Length();
        if (axisLength <= Epsilon)
            return RayBall(ref line, from, radius, frontFacing, out distance, out normal);

        axis /= axisLength;

        var found = false;
        var best = float.MaxValue;
        Vector3 bestNormal = default;

        var offset = line.From - from;
        var axisOrigin = Vector3.Dot(offset, axis);
        var axisDirection = Vector3.Dot(line.Direction, axis);
        var sideOrigin = offset - axis * axisOrigin;
        var sideDirection = line.Direction - axis * axisDirection;

        var a = Vector3.Dot(sideDirection, sideDirection);
        if (a > Epsilon)
        {
            var b = 2f * Vector3.Dot(sideOrigin, sideDirection);
            var c = Vector3.Dot(sideOrigin, sideOrigin) - radius * radius;
            var discriminant = b * b - 4f * a * c;
            if (discriminant >= 0f)
            {
                var root = MathF.Sqrt(discriminant);
                var near = (-b - root) / (2f * a);
                var far = (-b + root) / (2f * a);

                for (var i = 0; i < 2; i++)
                {
                    var t = i == 0 ? near : far;
                    if (t < 0f || t > line.Length || t >= best)
                        continue;

                    var height = axisOrigin + axisDirection * t;
                    if (height < 0f || height > axisLength)
                        continue;

                    var perpendicular = line.From + line.Direction * t - from - axis * height;
                    if (perpendicular.LengthSquared() <= Epsilon)
                        continue;

                    best = t;
                    bestNormal = Vector3.Normalize(perpendicular);
                    found = true;
                }
            }
        }

        for (var cap = 0; cap < 2; cap++)
        {
            var center = cap == 0 ? from : to;
            if (!RayBall(ref line, center, radius, frontFacing, out var t, out var capNormal) || t >= best)
                continue;

            var height = Vector3.Dot(line.From + line.Direction * t - from, axis);
            if (cap == 0 ? height > 0f : height < axisLength)
                continue; // that hit belongs to the other cap or to the side surface

            best = t;
            bestNormal = capNormal;
            found = true;
        }

        distance = found ? best : 0f;
        normal = bestNormal;
        return found;
    }

    private static void FillCorners(in BoundingBox box, Span<Vector3> corners)
    {
        corners[0] = new(box.Min.X, box.Min.Y, box.Min.Z);
        corners[1] = new(box.Min.X, box.Min.Y, box.Max.Z);
        corners[2] = new(box.Min.X, box.Max.Y, box.Min.Z);
        corners[3] = new(box.Min.X, box.Max.Y, box.Max.Z);
        corners[4] = new(box.Max.X, box.Min.Y, box.Min.Z);
        corners[5] = new(box.Max.X, box.Min.Y, box.Max.Z);
        corners[6] = new(box.Max.X, box.Max.Y, box.Min.Z);
        corners[7] = new(box.Max.X, box.Max.Y, box.Max.Z);
    }

    private static MyTriangle_Vertices SyntheticTriangle(Vector3 point, Vector3 normal)
    {
        var reference = MathF.Abs(normal.Y) < 0.9f ? Vector3.Up : Vector3.Right;
        var tangent = Vector3.Normalize(Vector3.Cross(normal, reference));
        var other = Vector3.Cross(normal, tangent);

        return new()
        {
            Vertex0 = point,
            Vertex1 = point + tangent,
            Vertex2 = point + other
        };
    }

    private static float Squared(float value) => value * value;

    private static Vector3 ClosestPointOnSegment(Vector3 from, Vector3 to, Vector3 point)
    {
        var direction = to - from;
        var lengthSquared = direction.LengthSquared();
        if (lengthSquared <= Epsilon)
            return from;

        var t = Math.Clamp(Vector3.Dot(point - from, direction) / lengthSquared, 0f, 1f);
        return from + direction * t;
    }

    private static float SquaredDistanceToBox(ref BoundingBox box, Vector3 from, Vector3 to)
    {
        if (SegmentIntersectsBox(ref box, from, to))
            return 0f;

        var best = MathF.Min(SquaredPointBox(ref box, from), SquaredPointBox(ref box, to));

        Span<Vector3> corners = stackalloc Vector3[8];
        FillCorners(in box, corners);

        ReadOnlySpan<(int A, int B)> edges =
        [
            (0, 1), (2, 3), (4, 5), (6, 7),
            (0, 2), (1, 3), (4, 6), (5, 7),
            (0, 4), (1, 5), (2, 6), (3, 7)
        ];

        foreach (var (a, b) in edges)
        {
            var squared = SquaredSegmentDistance(from, to, corners[a], corners[b]);
            if (squared < best)
                best = squared;
        }

        return best;
    }

    private static float SquaredPointBox(ref BoundingBox box, Vector3 point) =>
        Vector3.DistanceSquared(point, Vector3.Clamp(point, box.Min, box.Max));

    private static float SquaredPointBox(BoundingBox box, Vector3 point) =>
        SquaredPointBox(ref box, point);

    private static bool RayBox(ref Line line, Vector3 center, Vector3 extents, bool frontFacing, out float distance,
        out Vector3 normal)
    {
        distance = 0f;
        normal = default;

        var min = center - extents;
        var max = center + extents;
        var enter = 0f;
        var exit = line.Length;
        var enterAxis = -1;
        var exitAxis = -1;

        for (var axis = 0; axis < 3; axis++)
        {
            var origin = axis == 0 ? line.From.X : axis == 1 ? line.From.Y : line.From.Z;
            var direction = axis == 0 ? line.Direction.X : axis == 1 ? line.Direction.Y : line.Direction.Z;
            var low = axis == 0 ? min.X : axis == 1 ? min.Y : min.Z;
            var high = axis == 0 ? max.X : axis == 1 ? max.Y : max.Z;

            if (MathF.Abs(direction) < Epsilon)
            {
                if (origin < low || origin > high)
                    return false;

                continue;
            }

            var near = (low - origin) / direction;
            var far = (high - origin) / direction;
            if (near > far)
                (near, far) = (far, near);

            if (near > enter)
            {
                enter = near;
                enterAxis = axis;
            }

            if (far < exit)
            {
                exit = far;
                exitAxis = axis;
            }

            if (enter > exit)
                return false;
        }

        var selected = frontFacing ? enterAxis : exitAxis;
        var travelled = frontFacing ? enter : exit;
        if (selected < 0 || travelled < 0f || travelled > line.Length)
            return false;

        var component = selected == 0 ? line.Direction.X : selected == 1 ? line.Direction.Y : line.Direction.Z;
        var sign = (frontFacing ? -1f : 1f) * MathF.Sign(component);
        distance = travelled;
        normal = selected == 0
            ? new(sign, 0f, 0f)
            : selected == 1
                ? new Vector3(0f, sign, 0f)
                : new Vector3(0f, 0f, sign);
        return true;
    }

    private static bool SegmentIntersectsBox(ref BoundingBox box, Vector3 from, Vector3 to)
    {
        var direction = to - from;
        var enter = 0f;
        var exit = 1f;

        for (var axis = 0; axis < 3; axis++)
        {
            var origin = axis == 0 ? from.X : axis == 1 ? from.Y : from.Z;
            var delta = axis == 0 ? direction.X : axis == 1 ? direction.Y : direction.Z;
            var min = axis == 0 ? box.Min.X : axis == 1 ? box.Min.Y : box.Min.Z;
            var max = axis == 0 ? box.Max.X : axis == 1 ? box.Max.Y : box.Max.Z;

            if (MathF.Abs(delta) < Epsilon)
            {
                if (origin < min || origin > max)
                    return false;

                continue;
            }

            var t0 = (min - origin) / delta;
            var t1 = (max - origin) / delta;
            if (t0 > t1)
                (t0, t1) = (t1, t0);

            enter = MathF.Max(enter, t0);
            exit = MathF.Min(exit, t1);
            if (enter > exit)
                return false;
        }

        return true;
    }

    private static float SquaredSegmentDistance(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        var u = a2 - a1;
        var v = b2 - b1;
        var w = a1 - b1;

        var a = Vector3.Dot(u, u);
        var b = Vector3.Dot(u, v);
        var c = Vector3.Dot(v, v);
        var d = Vector3.Dot(u, w);
        var e = Vector3.Dot(v, w);
        var denominator = a * c - b * b;

        var sNumerator = denominator;
        var tNumerator = denominator;
        if (denominator < Epsilon)
        {
            sNumerator = 0f;
            tNumerator = e;
            denominator = c;
        }
        else
        {
            sNumerator = b * e - c * d;
            tNumerator = a * e - b * d;
            if (sNumerator < 0f)
            {
                sNumerator = 0f;
                tNumerator = e;
                denominator = c;
            }
            else if (sNumerator > denominator)
            {
                sNumerator = denominator;
                tNumerator = e + b;
                denominator = c;
            }
        }

        if (tNumerator < 0f)
        {
            tNumerator = 0f;
            if (-d < 0f)
                sNumerator = 0f;
            else if (-d > a)
                sNumerator = denominator;
            else
            {
                sNumerator = -d;
                denominator = a;
            }
        }
        else if (tNumerator > denominator)
        {
            tNumerator = denominator;
            if (-d + b < 0f)
                sNumerator = 0f;
            else if (-d + b > a)
                sNumerator = denominator;
            else
            {
                sNumerator = -d + b;
                denominator = a;
            }
        }

        var s = MathF.Abs(sNumerator) <= Epsilon ? 0f : sNumerator / denominator;
        var t = MathF.Abs(tNumerator) <= Epsilon ? 0f : tNumerator / denominator;

        var difference = w + u * s - v * t;
        return Vector3.Dot(difference, difference);
    }

    private sealed class DistanceComparer : IComparer<MyIntersectionResultLineTriangleEx>
    {
        public static readonly DistanceComparer Instance = new();

        public int Compare(MyIntersectionResultLineTriangleEx x, MyIntersectionResultLineTriangleEx y) =>
            x.Triangle.Distance.CompareTo(y.Triangle.Distance);
    }

    #endregion
}
