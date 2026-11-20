namespace CommitGraph;

public sealed class Repository
{
    public Dictionary<string, string> Branches { get; set; } = [];

    public Dictionary<string, RepositoryObject> Objects { get; set; } = [];

    public Dictionary<string, Author> Authors { get; } = [];

    public IEnumerable<Commit> Commits => Objects.Values.OfType<Commit>();

    public void AddAuthor(Author a) => Authors[a.Id] = a;

    public void AddObject(RepositoryObject o) => Objects[o.Hash] = o;

    public void CreateBranch(string name, string hash) => Branches[name] = hash;

    public string? Head { get; set; }
}

public static class RepositoryExtensions
{
    public static bool TryGetCommit(this Repository repo,
    string hash, out Commit? commit)
    {
        commit = null;

        if (hash is not null && 
        repo.Objects.ContainsKey(hash) &&
        repo.Objects[hash] is Commit c)
        {
            commit = c;
            return true;
        }
        
        return false;
    }

    public static Commit GetCommitOrThrow
    (this Repository repo, string hash)
    {
        if (!repo.TryGetCommit(hash, out var commit))
            throw new KeyNotFoundException($"Commit {hash} not found");

        return commit!;
        // wiem, ze zmienna commit ma typ Commit?
        // ale wczesniej sprawdzilismy czy nie jest nullem
        // wiec zignoruj ostrzezenia
    }

    public static IEnumerable<Commit> TraverseBranchByFirstParent
    (this Repository repo, string? startHash = null)
    // domyslnie null, a nie HEAD, bo musi to byc stala
    {
        var currentHash = startHash ?? repo.Head;

        if(currentHash is null)
            yield break;

        while(currentHash is not null)
        {
            if (!repo.TryGetCommit(currentHash, out var commit))
                yield break;

            yield return commit!;

            currentHash = commit!.ParentHashes.Count == 0
            ? null
            : commit.ParentHashes[0];
        }
    }

    public static IEnumerable<Commit> TraverseByRevision
    (this Repository repo, string pattern)
    {
        var rev = Revision.Parse(pattern);

        string currentHash;

        // Branches to slownik: klucz - nazwa galezi, wartosc - hash commita,
        // na ktory galaz wskazuje

        // 1. Czy BaseRef to nazwa gałęzi w słowniku Branches?
        if (repo.Branches.TryGetValue(rev.BaseRef, out var branchHash))
            currentHash = branchHash;
        // 2. Czy BaseRef to dosłownie napis "HEAD" (i repo.HEAD nie jest nullem)?
        else if (rev.BaseRef == "HEAD" && repo.Head is not null)
            currentHash = repo.Head;
        // 3. Czy BaseRef występuje bezpośrednio jako klucz w słowniku Objects
        // (czyli podano surowy hash)?
        else if (repo.Objects.ContainsKey(rev.BaseRef))
            currentHash = rev.BaseRef;
        // 4. Żadne z powyższych
        else
            throw new KeyNotFoundException("Base reference not found");

        var baseCommit = repo.GetCommitOrThrow(currentHash);
        yield return baseCommit;

        // przechodze przez kolejne modyfikatory (~N, ^N)
        foreach (var modifier in rev.Modifiers)
        {
            var innerHash = currentHash;
            foreach (var hash in modifier.Apply(innerHash, repo))
            {
                currentHash = hash;
                var commit = repo.GetCommitOrThrow(currentHash);
                yield return commit;
            }
        }
    }
}