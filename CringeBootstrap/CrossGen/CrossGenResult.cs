namespace CringeBootstrap.CrossGen;

internal record CrossGenResult(string CacheDirectory, bool CacheHit = false, bool Failed = false);