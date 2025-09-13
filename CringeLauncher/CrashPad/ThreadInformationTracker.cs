namespace CringeLauncher.CrashPad;

internal static class ThreadInformationTracker
{
    private static readonly ThreadLocal<StorageBucket> PerThreadStorage = new(ValueFactory, true);

    private static StorageBucket ValueFactory()
    {
        return new(Environment.CurrentManagedThreadId, GuessSimpleThreadType(Thread.CurrentThread));
    }

    private static ExceptionInformation.ThreadType GuessSimpleThreadType(Thread thread)
    {
        // this thread type implementation is limited to bcl-exposed information
        // so additional thread types must be set explicitly by caller
        return thread switch
        {
            { IsThreadPoolThread: true } => ExceptionInformation.ThreadType.ThreadPool,
            { IsBackground: true } => ExceptionInformation.ThreadType.Background,
            _ => ExceptionInformation.ThreadType.Normal
        };
    }

    public static ExceptionInformation.ThreadType GetThreadType(Thread thread)
    {
        if (thread == Thread.CurrentThread && PerThreadStorage.Value is { } fastPathValue) return fastPathValue.Type;

        return PerThreadStorage.Values.FirstOrDefault(b => b.ThreadId == Environment.CurrentManagedThreadId)?.Type ??
               GuessSimpleThreadType(thread);
    }

    public static void MarkCurrentThreadType(ExceptionInformation.ThreadType type)
    {
        PerThreadStorage.Value = new(Environment.CurrentManagedThreadId, type);
    }

    private record StorageBucket(int ThreadId, ExceptionInformation.ThreadType Type)
    {
        public ExceptionInformation.ThreadType Type { get; set; } = Type;
    }
}