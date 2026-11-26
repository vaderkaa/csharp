namespace Snapshot;

// sealed - blokuje mozliwosc dziedziczenia
public sealed record FileEntry(string RelativePath, string FullPath, long Size, DateTime LastWriteTime);