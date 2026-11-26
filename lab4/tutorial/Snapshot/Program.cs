namespace Snapshot;

internal class Program // klasa widoczna wszedzie wewnatrz obecnego projektu
{
    private static readonly string[] IgnorePatterns = ["*.tmp", "*.bak", "bin", "obj", "cache"];

    private static int Main(string[] args)
    {
        try
        {
            switch (args)
            {
                case ["scan", var source]: // czy ma dokladnie 2 argumenty
                    return Scan(source); // wykonuje funkcje Scan (z dolu) i konczy wywolywanie calej metody, calego maina
                default:
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) // when pozwala zrobic is i or
        {
            Console.Error.WriteLine($"Error: {e.Message}");
            return 1;
        }
    }

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
            Console.WriteLine($"{file.Size.ToHumanSize(), 10}  {file.RelativePath}"); // WYROWNANIE DO 10 ZNAKOW DO PRAWEJ STRONY

        // liczba plikow i laczny rozmiar
        long totalSize = files.Sum(f => f.Size);
        Console.WriteLine($"{files.Count} files, {totalSize.ToHumanSize()} in total");

        // trzy najwieksze pliki
        Console.WriteLine("Largest files:");
        foreach (FileEntry file in files.OrderByDescending(f => f.Size).Take(3))
            Console.WriteLine($"{file.Size.ToHumanSize(),10}  {file.RelativePath}");

        return 0; // skanowanie zakonczylo sie sukcesem
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet run -- scan <source-directory>");
    }
}