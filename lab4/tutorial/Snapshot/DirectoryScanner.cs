using System.IO.Enumeration; // do FileSystemName.MatchesSimpleExpression(pattern, name)

namespace Snapshot;

public class DirectoryScanner(string rootDirectory, IReadOnlyList<string> ignorePatterns)
{
    public string RootDirectory { get; } = Path.GetFullPath(rootDirectory);

    public string ToRelativePath(string fullPath)
    {
        // otrzymuje sciezke wzgledna od root do pliku
        return Path.GetRelativePath(RootDirectory, fullPath).Replace(Path.DirectorySeparatorChar, '/');
    }

    public bool IsIgnored(string relativePath)
    {
        string[] segments = relativePath.Split('/'); // jesli ktos wywola to z zewnatrz to w sumie przypal
        // string[] segments = relativePath.Split('/', '\\');

        foreach (var segment in segments)
        {
            foreach(var pattern in ignorePatterns) // ignorePatterns jest z kontruktora glownego (naglowek klasy)
            {
                // pattern, nazwa pliku, ignoreCase = true (ignoruje wielkosc liter)
                if (FileSystemName.MatchesSimpleExpression(pattern, segment))
                    return true;
            }
        }

        return false;
    }

    public IEnumerable<FileEntry> Scan()
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true, // search the whole tree, not just the top directory
            IgnoreInaccessible = true, // skip entries we are not allowed to read (the default)
        };

        foreach (FileInfo file in new DirectoryInfo(RootDirectory).EnumerateFiles("*", options)) // omija pliki systemowe i ukryte
        {
            string relativePath = ToRelativePath(file.FullName);

            if (IsIgnored(relativePath))
                continue;

            yield return new FileEntry(relativePath, file.FullName, file.Length, file.LastWriteTime);
        }
    }
}