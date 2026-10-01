using VRageMath;

namespace CringeLauncher.Patches.Dedicated;

internal static class SupportMapping
{
    private const float Epsilon = 1e-7f;
    private const float ConvergenceEpsilon = 1e-5f;
    private const float TouchEpsilon = 1e-9f;
    private const float GrazingEpsilon = 1e-6f;
    private const float DuplicateEpsilon = 1e-10f;
    
    private const int MaxIterations = 64;

    internal readonly ref struct Body
    {
        public readonly ReadOnlySpan<Vector3> Vertices;
        public readonly Vector3 Start;
        public readonly Vector3 End;
        public readonly float Radius;
        public readonly bool IsSegment;

        private Body(ReadOnlySpan<Vector3> vertices, Vector3 start, Vector3 end, float radius, bool isSegment)
        {
            Vertices = vertices;
            Start = start;
            End = end;
            Radius = radius;
            IsSegment = isSegment;
        }

        public static Body Cloud(ReadOnlySpan<Vector3> vertices, float radius = 0f) =>
            new(vertices, default, default, radius, false);

        public static Body Ball(Vector3 center, float radius) => new(default, center, center, radius, false);

        public static Body Segment(Vector3 start, Vector3 end, float radius) =>
            new(default, start, end, radius, true);
    }

    internal static Vector3 Support(in Body body, Vector3 direction)
    {
        var vertices = body.Vertices;
        if (!vertices.IsEmpty)
        {
            var best = vertices[0];
            var bestDot = Vector3.Dot(best, direction);
            for (var i = 1; i < vertices.Length; i++)
            {
                var candidate = vertices[i];
                var dot = Vector3.Dot(candidate, direction);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = candidate;
                }
            }

            return body.Radius > 0f ? best + Normalized(direction) * body.Radius : best;
        }

        var center = body.IsSegment
            ? Vector3.Dot(body.Start, direction) >= Vector3.Dot(body.End, direction) ? body.Start : body.End
            : body.Start;
        return center + Normalized(direction) * body.Radius;
    }

    internal static float Distance(in Body a, in Body b, out Vector3 pointA, out Vector3 pointB)
    {
        Span<Vector3> difference = stackalloc Vector3[4];
        
        Span<Vector3> onA = stackalloc Vector3[4];
        Span<Vector3> onB = stackalloc Vector3[4];
        Span<Vector3> bestOnA = stackalloc Vector3[4];
        Span<Vector3> bestOnB = stackalloc Vector3[4];
        
        Span<float> weights = stackalloc float[4];
        Span<float> bestWeights = stackalloc float[4];
        
        var bestDistance = float.MaxValue;
        var bestCount = 0;

        var direction = a.Start - b.Start;
        if (direction.LengthSquared() <= Epsilon)
            direction = Vector3.UnitX;

        var count = 1;
        difference[0] = Support(a, direction) - Support(b, -direction);
        onA[0] = Support(a, direction);
        onB[0] = Support(b, -direction);
        weights[0] = 1f;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var closest = Combine(difference, weights, count);
            var squared = closest.LengthSquared();

            if (squared < bestDistance)
            {
                bestDistance = squared;
                bestCount = count;
                for (var i = 0; i < count; i++)
                {
                    bestOnA[i] = onA[i];
                    bestOnB[i] = onB[i];
                    bestWeights[i] = weights[i];
                }
            }

            if (squared <= TouchEpsilon)
            {
                pointA = Combine(onA, weights, count);
                pointB = Combine(onB, weights, count);
                return 0f;
            }

            var search = -closest;
            var supportA = Support(a, search);
            var supportB = Support(b, -search);
            var support = supportA - supportB;

            var duplicate = false;
            for (var i = 0; i < count; i++)
            {
                if (!((difference[i] - support).LengthSquared() <= DuplicateEpsilon)) continue;
                duplicate = true;
                break;
            }

            // The support point brings the simplex no closer to the origin, or is already part of it.
            if (duplicate || squared - Vector3.Dot(closest, support) <= ConvergenceEpsilon * squared)
                break;

            difference[count] = support;
            onA[count] = supportA;
            onB[count] = supportB;
            count++;

            if (!Reduce(difference, onA, onB, weights, ref count)) continue;
            
            pointA = Combine(onA, weights, count);
            pointB = Combine(onB, weights, count);
            return 0f;
        }

        pointA = Combine(bestOnA, bestWeights, bestCount);
        pointB = Combine(bestOnB, bestWeights, bestCount);
        return bestCount == 0 ? float.MaxValue : MathF.Sqrt(bestDistance);
    }

    internal static bool PlaneSeparatesBox(Plane plane, Vector3 boxMin, Vector3 boxMax)
    {
        var center = (boxMin + boxMax) * 0.5f;
        var halfExtents = (boxMax - boxMin) * 0.5f;
        var radius = MathF.Abs(plane.Normal.X) * halfExtents.X + MathF.Abs(plane.Normal.Y) * halfExtents.Y +
                     MathF.Abs(plane.Normal.Z) * halfExtents.Z;
        return Vector3.Dot(plane.Normal, center) + plane.D > radius;
    }

    internal static bool ClipConvex(ReadOnlySpan<Plane> planes, Vector3 from, Vector3 direction, float maxDistance,
        bool frontFacing, out float distance, out Vector3 normal)
    {
        distance = 0f;
        normal = default;

        var enter = 0f;
        var exit = maxDistance;
        var enterPlane = -1;
        var exitPlane = -1;

        for (var i = 0; i < planes.Length; i++)
        {
            var plane = planes[i];
            var denominator = Vector3.Dot(plane.Normal, direction);
            var numerator = Vector3.Dot(plane.Normal, from) + plane.D;

            if (MathF.Abs(denominator) < Epsilon)
            {
                if (numerator > 0f)
                    return false;

                continue;
            }

            var t = -numerator / denominator;
            if (denominator < 0f)
            {
                if (t > enter)
                {
                    enter = t;
                    enterPlane = i;
                }
            }
            else if (t < exit)
            {
                exit = t;
                exitPlane = i;
            }

            if (enter > exit)
                return false;
        }

        var selected = frontFacing ? enterPlane : exitPlane;
        var travelled = frontFacing ? enter : exit;
        if (selected < 0 || travelled < 0f || travelled > maxDistance)
            return false;

        distance = travelled;
        normal = planes[selected].Normal;
        return true;
    }

    private static float DistanceToPoint(in Body body, Vector3 point, Span<Vector3> carried,
        Span<float> weights, ref int carriedCount, out Vector3 onBody)
    {
        Span<Vector3> difference = stackalloc Vector3[4];
        Span<Vector3> onA = stackalloc Vector3[4];
        Span<Vector3> onB = stackalloc Vector3[4];
        Span<Vector3> bestOnA = stackalloc Vector3[4];
        Span<float> bestWeights = stackalloc float[4];

        int count;
        if (carriedCount > 0)
        {
            count = Math.Min(carriedCount, 4);
            for (var i = 0; i < count; i++)
            {
                onA[i] = carried[i];
                onB[i] = point;
                difference[i] = carried[i] - point;
                weights[i] = i == 0 ? 1f : 0f;
            }
        }
        else
        {
            count = 1;
            onA[0] = Support(body, Vector3.UnitX);
            onB[0] = point;
            difference[0] = onA[0] - point;
            weights[0] = 1f;
        }

        var bestDistance = float.MaxValue;
        var bestCount = 0;

        Reduce(difference, onA, onB, weights, ref count);

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var closest = Combine(difference, weights, count);
            var squared = closest.LengthSquared();

            if (squared < bestDistance)
            {
                bestDistance = squared;
                bestCount = count;
                for (var i = 0; i < count; i++)
                {
                    bestOnA[i] = onA[i];
                    bestWeights[i] = weights[i];
                }
            }

            if (squared <= TouchEpsilon)
                break;

            var search = -closest;
            var supportA = Support(body, search);
            var support = supportA - point;

            var duplicate = false;
            for (var i = 0; i < count; i++)
            {
                if ((difference[i] - support).LengthSquared() <= DuplicateEpsilon)
                {
                    duplicate = true;
                    break;
                }
            }

            if (duplicate || squared - Vector3.Dot(closest, support) <= ConvergenceEpsilon * squared)
                break;

            onA[count] = supportA;
            onB[count] = point;
            difference[count] = support;
            count++;
            
            if (!Reduce(difference, onA, onB, weights, ref count)) continue;
            
            bestDistance = 0f;
            bestCount = count;
            for (var i = 0; i < count; i++)
            {
                bestOnA[i] = onA[i];
                bestWeights[i] = weights[i];
            }

            break;
        }

        // hand the surviving body points back for the next advancement step
        carriedCount = Math.Max(bestCount, 1);
        for (var i = 0; i < carriedCount; i++)
            carried[i] = bestOnA[i];

        onBody = Combine(bestOnA, bestWeights, carriedCount);
        return bestCount == 0 ? 0f : MathF.Sqrt(bestDistance);
    }

    /// <summary>
    ///     Entry point of a ray against a convex body; <paramref name="frontFacing" /> asks for the entry
    ///     point (the shipped adapter's <c>DIRECT_TRIANGLES</c>) rather than the exit point.
    /// </summary>
    internal static bool Raycast(in Body body, Vector3 origin, Vector3 direction, float maxDistance,
        bool frontFacing, out float distance, out Vector3 normal)
    {
        distance = 0f;
        normal = default;

        var from = frontFacing ? origin : origin + direction * maxDistance;
        var step = frontFacing ? direction : -direction;
        var point = from;
        var travelled = 0f;
        var previousSeparation = float.MaxValue;
        var surfaceNormal = default(Vector3);

        // Declared once: the advanced point moves each step, so the simplex is carried and re-based by
        // DistanceToPoint instead of restarting a full GJK search per step.
        Span<Vector3> carried = stackalloc Vector3[4];
        Span<float> carriedWeights = stackalloc float[4];
        var carriedCount = 0;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var separation = DistanceToPoint(body, point, carried, carriedWeights, ref carriedCount, out var onBody);

            if (separation * separation <= TouchEpsilon)
            {
                // A ray that starts on the surface is not crossing a front face.
                if (travelled <= Epsilon)
                    return false;

                distance = frontFacing ? travelled : maxDistance - travelled;
                // The direction the body was last approached from is its outward normal at the hit.
                normal = surfaceNormal;
                return true;
            }

            // towardSurface points from the body out to the current point, so approaching means moving
            // against it; the outward normal at the hit is towardSurface itself.
            var towardSurface = (point - onBody) / separation;
            var closing = -Vector3.Dot(step, towardSurface);

            // Moving away, or grazing: conservative advancement can only approach the body from outside.
            if (closing <= GrazingEpsilon || separation >= previousSeparation)
                return false;

            surfaceNormal = towardSurface;
            previousSeparation = separation;
            travelled += separation / closing;
            if (travelled > maxDistance)
                return false;

            point = from + step * travelled;
        }

        return false;
    }

    private static bool Reduce(Span<Vector3> difference, Span<Vector3> onA, Span<Vector3> onB, Span<float> weights,
        ref int count)
    {
        switch (count)
        {
            case 1:
                weights[0] = 1f;
                break;
            case 2:
                ReduceSegment(difference, onA, onB, weights, out count);
                break;
            case 3:
                ReduceTriangle(difference, onA, onB, weights, out count);
                break;
            default:
                return ReduceTetrahedron(difference, onA, onB, weights, out count);
        }
        
        return false;
    }

    private static void ReduceSegment(Span<Vector3> difference, Span<Vector3> onA, Span<Vector3> onB,
        Span<float> weights, out int count)
    {
        var a = difference[0];
        var ab = difference[1] - a;
        var lengthSquared = ab.LengthSquared();
        var t = lengthSquared <= Epsilon ? 0f : Math.Clamp(-Vector3.Dot(a, ab) / lengthSquared, 0f, 1f);

        if (t <= 0f)
        {
            Keep(difference, onA, onB, weights, out count, 0);
            weights[0] = 1f;
            return;
        }

        if (t >= 1f)
        {
            Keep(difference, onA, onB, weights, out count, 1);
            weights[0] = 1f;
            return;
        }

        count = 2;
        weights[0] = 1f - t;
        weights[1] = t;
    }

    private static void ReduceTriangle(Span<Vector3> difference, Span<Vector3> onA, Span<Vector3> onB,
        Span<float> weights, out int count)
    {
        Span<float> barycentric = stackalloc float[3];
        ClosestPointTriangle(difference[0], difference[1], difference[2], barycentric);
        KeepWeights(barycentric, difference, onA, onB, weights, out count);
    }

    private static bool ReduceTetrahedron(Span<Vector3> difference, Span<Vector3> onA, Span<Vector3> onB,
        Span<float> weights, out int count)
    {
        Span<float> best = stackalloc float[4];

        if (ClosestPointTetrahedron(difference[0], difference[1], difference[2], difference[3], best))
        {
            // The origin is inside the tetrahedron, so the bodies overlap.
            weights[0] = 1f;
            count = 1;
            return true;
        }

        var kept = 0;
        for (var i = 0; i < 4; i++)
        {
            if (best[i] <= 0f)
                continue;

            difference[kept] = difference[i];
            onA[kept] = onA[i];
            onB[kept] = onB[i];
            weights[kept] = best[i];
            kept++;
        }

        count = Math.Max(kept, 1);
        
        return false;
    }

    private static void Keep(Span<Vector3> difference, Span<Vector3> onA, Span<Vector3> onB, Span<float> weights,
        out int count, int index)
    {
        (difference[0], difference[index]) = (difference[index], difference[0]);
        (onA[0], onA[index]) = (onA[index], onA[0]);
        (onB[0], onB[index]) = (onB[index], onB[0]);

        count = 1;
        weights[0] = 1f;
    }

    private static void KeepWeights(Span<float> barycentric, Span<Vector3> difference, Span<Vector3> onA,
        Span<Vector3> onB, Span<float> weights, out int count)
    {
        var kept = 0;
        for (var i = 0; i < 3; i++)
        {
            if (barycentric[i] <= 0f)
                continue;

            difference[kept] = difference[i];
            onA[kept] = onA[i];
            onB[kept] = onB[i];
            weights[kept] = barycentric[i];
            kept++;
        }

        count = Math.Max(kept, 1);
    }

    private static Vector3 Combine(ReadOnlySpan<Vector3> points, ReadOnlySpan<float> weights, int count)
    {
        Vector3 result = default;
        for (var i = 0; i < count; i++)
            result += points[i] * weights[i];
        return result;
    }

    private static Vector3 Normalized(Vector3 value)
    {
        var lengthSquared = value.LengthSquared();
        return lengthSquared <= Epsilon ? Vector3.Zero : value / MathF.Sqrt(lengthSquared);
    }

    private static void ClosestPointTriangle(Vector3 a, Vector3 b, Vector3 c, Span<float> weights)
    {
        weights[0] = 1f;
        weights[1] = 0f;
        weights[2] = 0f;

        var ab = b - a;
        var ac = c - a;
        var abAb = Vector3.Dot(ab, ab);
        var abAc = Vector3.Dot(ab, ac);
        var acAc = Vector3.Dot(ac, ac);
        var denominator = abAb * acAc - abAc * abAc;

        var v = 0f;
        var w = 0f;
        if (denominator > Epsilon)
        {
            var apAb = Vector3.Dot(-a, ab);
            var apAc = Vector3.Dot(-a, ac);
            v = (acAc * apAb - abAc * apAc) / denominator;
            w = (abAb * apAc - abAc * apAb) / denominator;
        }

        if (v >= 0f && w >= 0f && v + w <= 1f)
        {
            // Inside the triangle: the perpendicular projection is the closest point.
            weights[0] = 1f - v - w;
            weights[1] = v;
            weights[2] = w;
            return;
        }

        var edgeBc = EdgeParameter(b, c);
        var edgeAc = EdgeParameter(a, c);
        var edgeAb = EdgeParameter(a, b);

        var distanceBc = SquaredAtParameter(b, c, edgeBc);
        var distanceAc = SquaredAtParameter(a, c, edgeAc);
        var distanceAb = SquaredAtParameter(a, b, edgeAb);

        if (distanceBc <= distanceAc && distanceBc <= distanceAb)
        {
            weights[0] = 0f;
            weights[1] = 1f - edgeBc;
            weights[2] = edgeBc;
        }
        else if (distanceAc <= distanceAb)
        {
            weights[0] = 1f - edgeAc;
            weights[1] = 0f;
            weights[2] = edgeAc;
        }
        else
        {
            weights[0] = 1f - edgeAb;
            weights[1] = edgeAb;
            weights[2] = 0f;
        }
    }

    private static float EdgeParameter(Vector3 from, Vector3 to)
    {
        var direction = to - from;
        var lengthSquared = direction.LengthSquared();
        return lengthSquared <= Epsilon ? 0f : Math.Clamp(-Vector3.Dot(from, direction) / lengthSquared, 0f, 1f);
    }

    private static float SquaredAtParameter(Vector3 from, Vector3 to, float t) =>
        (from + (to - from) * t).LengthSquared();

    private static bool ClosestPointTetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Span<float> weights)
    {
        if (Enclosed(a, b, c, d))
            return true;

        var best = float.MaxValue;
        Span<float> face = stackalloc float[3];

        for (var index = 0; index < 4; index++)
        {
            Vector3 p0;
            Vector3 p1;
            Vector3 p2;
            switch (index)
            {
                case 0:
                    p0 = a;
                    p1 = b;
                    p2 = c;
                    break;
                case 1:
                    p0 = a;
                    p1 = c;
                    p2 = d;
                    break;
                case 2:
                    p0 = a;
                    p1 = d;
                    p2 = b;
                    break;
                default:
                    p0 = b;
                    p1 = c;
                    p2 = d;
                    break;
            }

            ClosestPointTriangle(p0, p1, p2, face);
            var candidate = p0 * face[0] + p1 * face[1] + p2 * face[2];
            var squared = candidate.LengthSquared();
            if (squared >= best)
                continue;

            best = squared;
            weights.Clear();
            switch (index)
            {
                case 0:
                    weights[0] = face[0];
                    weights[1] = face[1];
                    weights[2] = face[2];
                    break;
                case 1:
                    weights[0] = face[0];
                    weights[2] = face[1];
                    weights[3] = face[2];
                    break;
                case 2:
                    weights[0] = face[0];
                    weights[3] = face[1];
                    weights[1] = face[2];
                    break;
                default:
                    weights[1] = face[0];
                    weights[2] = face[1];
                    weights[3] = face[2];
                    break;
            }
        }

        return false;
    }

    private static bool Enclosed(Vector3 a, Vector3 b, Vector3 c, Vector3 d) =>
        Inside(a, b, c, d) && Inside(a, c, d, b) && Inside(a, d, b, c) && Inside(b, c, d, a);

    private static bool Inside(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 apex)
    {
        var normal = Vector3.Cross(p1 - p0, p2 - p0);
        var lengthSquared = normal.LengthSquared();
        if (lengthSquared <= Epsilon)
            return false;

        normal /= MathF.Sqrt(lengthSquared);
        var toApex = Vector3.Dot(normal, apex - p0);
        if (MathF.Abs(toApex) <= 1e-6f)
            return false;

        return toApex * Vector3.Dot(normal, -p0) >= 0f;
    }
}
