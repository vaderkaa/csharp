using System.ComponentModel; // dla CancelEventArgs

namespace Snapshot;

public class FileFoundEventArgs(FileEntry file) : CancelEventArgs // dla wlasciwosci public bool Cancel { get; set; }
{
    public FileEntry File { get; } = file;
}

public class FileProcessedEventArgs(FileEntry file, ChangeKind kind) : EventArgs
{
    public FileEntry File { get; } = file;
    public ChangeKind Kind { get; } = kind;
}

// pasek postepu
public class BackupProgressEventArgs(int processedFiles, int totalFiles, long processedBytes, long totalBytes) : EventArgs
{
    public int ProcessedFiles { get; } = processedFiles;
    public int TotalFiles { get; } = totalFiles;
    public long ProcessedBytes { get; } = processedBytes;
    public long TotalBytes { get; } = totalBytes;

    public double Fraction => TotalBytes == 0 ? 1.0 : (double)ProcessedBytes / TotalBytes;
}

public class BackupCompletedEventArgs(BackupSummary summary) : EventArgs
{
    public BackupSummary Summary { get; } = summary;
}