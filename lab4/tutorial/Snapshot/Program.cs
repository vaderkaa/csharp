namespace Snapshot;

internal class Program // klasa widoczna wszedzie wewnatrz obecnego projektu
{
    private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];

    private static int Scan(string source)
    {
        // sprawdzenie czy katalog w ogole istnieje
        if (!Directory.Exists(source))
        {
            Console.Error.WriteLine($"Error: Directory '{source}' does not exist.");
            return 1;
        }

        Console.WriteLine($"Scanning {source}");

        var scanner = new DirectoryScanner(source, IgnorePatterns);
        List<FileEntry> files = scanner.Scan()
                                .OrderBy(f => f.RelativePath, StringComparer.Ordinal) // sortuje po ASCII, dzieki czemu wszedzie wypluje to samo
                                .ToList();

        // wypisanie kazdego pliku i jego rozmiaru
        foreach (FileEntry file in files)
            Console.WriteLine($"{file.Size.ToHumanSize(),10}  {file.RelativePath}"); // WYROWNANIE DO 10 ZNAKOW DO PRAWEJ STRONY

        // liczba plikow i laczny rozmiar
        long totalSize = files.Sum(f => f.Size);
        Console.WriteLine($"{files.Count} files, {totalSize.ToHumanSize()} in total");

        // trzy najwieksze pliki
        Console.WriteLine("Largest files:");
        foreach (FileEntry file in files.OrderByDescending(f => f.Size).Take(3))
            Console.WriteLine($"{file.Size.ToHumanSize(),10}  {file.RelativePath}");

        return 0; // skanowanie zakonczylo sie sukcesem
    }

    private static int Backup(string source, string repository, long? maxSizeKb = null)
    {
        if (!Directory.Exists(source)) // czy katalog zrodlowy istnieje
        {
            Console.Error.WriteLine($"Error: Source directory '{source}' does not exist.");
            return 1;
        }

        if (PathUtils.IsInside(repository, source)) // czy repozytorium jest w srodku pliku zrodlowego
        {
            Console.Error.WriteLine("Error: Repository directory cannot be inside the source directory.");
            return 1;
        }

        var engine = new BackupEngine(source, repository, IgnorePatterns);
        var reporter = new ConsoleReporter(engine);

        if (maxSizeKb.HasValue)
        {
            long maxBytes = maxSizeKb.Value * 1024;
            engine.FileFound += (_, e) =>
            {
                if (e.File.Size > maxBytes)
                {
                    e.Cancel = true;
                    Console.WriteLine($"Skipping {e.File.RelativePath} ({e.File.Size.ToHumanSize()} > {maxBytes.ToHumanSize()})");
                }
            };
        }

        engine.Run();

        string latestManifestPath = Path.Combine(repository, "latest.manifest");
        Console.WriteLine($"Manifest saved to {latestManifestPath}");

        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- scan <source-directory>");
        Console.WriteLine("  dotnet run -- backup <source-directory> <repository-directory> [max-size-kb]");
    }

    private static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["scan", var source]: // czy ma dokladnie 2 argumenty
                    return Scan(source); // wykonuje funkcje Scan i konczy wywolywanie calej metody, calego maina
                case ["backup", var source, var repository]:
                    return Backup(source, repository);
                case ["backup", var source, var repository, var maxSize] when long.TryParse(maxSize, out long maxSizeKb):
                    return Backup(source, repository, maxSizeKb);
                default:
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException) // when pozwala zrobic is i or
        {
            Console.Error.WriteLine($"Error: {e.Message}");
            return 1;
        }
    }
}