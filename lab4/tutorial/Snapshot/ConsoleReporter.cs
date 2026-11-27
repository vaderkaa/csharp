namespace Snapshot;

public class ConsoleReporter : IDisposable
{
    private const int BarWidth = 30;
    private readonly BackupEngine _engine;
    private int _lastProgressLength;

    public ConsoleReporter(BackupEngine engine)
    {
        _engine = engine;
        _engine.FileProcessed += OnFileProcessed;
        _engine.ProgressChanged += OnProgressChanged;
        _engine.Completed += OnCompleted;
    }

    private void OnFileProcessed(object? sender, FileProcessedEventArgs e)
    {
        if (e.Kind == ChangeKind.Unchanged)
            return;

        string line = $"{e.Kind.ToSymbol()} {e.File.RelativePath}";
        Console.WriteLine($"\r{line.PadRight(_lastProgressLength)}");
        _lastProgressLength = 0;
    }

    private void OnProgressChanged(object? sender, BackupProgressEventArgs e)
    {
        int filled = (int)Math.Round(e.Fraction * BarWidth);
        string bar = new string('#', filled) + new string('.', BarWidth - filled);
        int percent = (int)Math.Round(e.Fraction * 100);

        string line = $"[{bar}] {percent}% ({e.ProcessedFiles}/{e.TotalFiles} files, {e.ProcessedBytes.ToHumanSize()})";
        int width = Math.Max(_lastProgressLength, line.Length);

        Console.Write($"\r{line.PadRight(width)}");
        _lastProgressLength = line.Length;
    }

    private void OnCompleted(object? sender, BackupCompletedEventArgs e)
    {
        if (_lastProgressLength > 0)
        {
            Console.WriteLine();
            _lastProgressLength = 0;
        }

        foreach (FileChange change in e.Summary.Changes)
        {
            if (change.Kind == ChangeKind.Deleted)
            {
                Console.WriteLine($"{change.Kind.ToSymbol()} {change.Path}");
            }
        }

        Console.WriteLine(e.Summary);
    }

    public void Dispose()
    {
        _engine.FileProcessed -= OnFileProcessed;
        _engine.ProgressChanged -= OnProgressChanged;
        _engine.Completed -= OnCompleted;
    }
}