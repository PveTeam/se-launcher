using System.Collections.Concurrent;
using System.Reflection;
using HarmonyLib;
using NLog;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.FileSystem;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ResourceFreePatch
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private static readonly ConcurrentDictionary<ulong, WeakReference<Stream>> Streams = []; 
    private static ulong _idCounter;
    
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        const string prefix = "VRage.Game.ModAPI.IMyUtilities.";
        return AccessTools.GetDeclaredMethods(typeof(MyAPIUtilities))
            .Where(b => b.Name.StartsWith($"{prefix}WriteFile") || b.Name.StartsWith($"{prefix}ReadFile") ||
                        b.Name.StartsWith($"{prefix}WriteBinaryFile") || b.Name.StartsWith($"{prefix}ReadBinaryFile"));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var readMethod =
            AccessTools.DeclaredMethod(typeof(MyFileSystem), nameof(MyFileSystem.OpenRead), [typeof(string)]);
        var writeMethod = AccessTools.DeclaredMethod(typeof(MyFileSystem), nameof(MyFileSystem.OpenWrite),
            [typeof(string), typeof(FileMode)]);
        
        return new CodeMatcher(instructions)
            .SearchForward(b => b.Calls(readMethod) || b.Calls(writeMethod))
            .Advance(1)
            .Insert(CodeInstruction.Call(typeof(ResourceFreePatch), nameof(Wrap)))
            .InstructionEnumeration();
    }

    // todo ideally track calling mod for debug purposes, not super important anyways
    private static Stream? Wrap(Stream? stream)
    {
        if (stream == null)
            return null;
        
        var id = Interlocked.Increment(ref _idCounter);
        var trackingStream = new TrackingStream(id, stream);

        Streams.TryAdd(id, new WeakReference<Stream>(trackingStream));
        return trackingStream;
    }

    public static void OnUnloaded()
    {
        if (Streams.IsEmpty) return;
        
        Log.Info("Closing {Count} leftover files", Streams.Count);
        foreach (var r in Streams.Values)
        {
            if (!r.TryGetTarget(out var s))
                continue;
            
            try
            {
                s.Dispose();
            }
            catch (Exception e)
            {
                Log.Warn(e, "Failed to close leftover file {File}", s);
            }
        }
        Streams.Clear();
    }

    private class TrackingStream(ulong id, Stream stream) : Stream
    {
        private bool _disposed;
        
        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            
            ObjectDisposedException.ThrowIf(_disposed, this);
            _disposed = true;
            stream.Dispose();
            Streams.TryRemove(id, out _);
        }

        public override ValueTask DisposeAsync()
        {
            base.Dispose();
            return stream.DisposeAsync();
        }

        public override void Close()
        {
            base.Close();
            stream.Close();
        }

        #region Implementation delegation

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => stream.BeginRead(buffer, offset, count, callback, state);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => stream.BeginWrite(buffer, offset, count, callback, state);

        public override void CopyTo(Stream destination, int bufferSize) => stream.CopyTo(destination, bufferSize);

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult) => stream.EndRead(asyncResult);

        public override void EndWrite(IAsyncResult asyncResult) => stream.EndWrite(asyncResult);

        public override Task FlushAsync(CancellationToken cancellationToken) => stream.FlushAsync(cancellationToken);

        public override int Read(Span<byte> buffer) => stream.Read(buffer);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => stream.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => stream.ReadAsync(buffer, cancellationToken);

        public override int ReadByte() => stream.ReadByte();

        public override void Write(ReadOnlySpan<byte> buffer) => stream.Write(buffer);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => stream.WriteAsync(buffer, offset, count, cancellationToken);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => stream.WriteAsync(buffer, cancellationToken);

        public override void WriteByte(byte value) => stream.WriteByte(value);

        public override bool CanTimeout => stream.CanTimeout;

        public override int ReadTimeout
        {
            get => stream.ReadTimeout;
            set => stream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => stream.WriteTimeout;
            set => stream.WriteTimeout = value;
        }

        public override bool Equals(object? obj) => stream.Equals(obj);

        public override int GetHashCode() => stream.GetHashCode();

        public override string? ToString() => stream.ToString();

        public override void Flush() => stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

        public override void SetLength(long value) => stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        #endregion
    }
}

[HarmonyPatch(typeof(MySessionLoader), nameof(MySessionLoader.Unload))]
internal static class SessionUnloadPatch
{
    private static void Postfix()
    {
        // in case session didnt unload cleanly and has skipped regular unload event 
        ResourceFreePatch.OnUnloaded();
    }
}