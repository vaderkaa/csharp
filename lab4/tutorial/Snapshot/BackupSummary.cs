namespace Snapshot;

public class BackupSummary(IReadOnlyList<FileChange> changes)
{
    public IReadOnlyList<FileChange> Changes { get; } = changes;

    public int New => Changes.Count(c => c.Kind == ChangeKind.New);
    public int Modified => Changes.Count(c => c.Kind == ChangeKind.Modified);
    public int Deleted => Changes.Count(c => c.Kind == ChangeKind.Deleted);
    public int Unchanged => Changes.Count(c => c.Kind == ChangeKind.Unchanged);

    public override string ToString() =>
        $"New: {New}, modified: {Modified}, deleted: {Deleted}, unchanged: {Unchanged}";
}