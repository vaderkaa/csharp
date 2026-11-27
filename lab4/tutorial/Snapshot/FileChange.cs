namespace Snapshot;

public enum ChangeKind
{
    Unchanged,
    New,
    Modified,
    Deleted
}

public record FileChange(string Path, ChangeKind Kind);

public static class ChangeKindExtensions
{
    public static char ToSymbol(this ChangeKind kind) // => switch kind
    {
        switch (kind)
        {
            case ChangeKind.New:
                return '+';
            case ChangeKind.Modified:
                return '~';
            case ChangeKind.Deleted:
                return '-';
            case ChangeKind.Unchanged:
                return ' ';
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }
}