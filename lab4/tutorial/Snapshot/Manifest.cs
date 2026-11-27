namespace Snapshot;

public record ManifestEntry(string Path, long Size, string Hash);

public class Manifest
{
    // klucz - sciezka pliku, wartosc - ManifestEntry
    // utrzymuje klucze w kolejnosci alfabetycznej, zgodnie z StringComparer.Ordinal
    private readonly SortedDictionary<string, ManifestEntry> _entries = new(StringComparer.Ordinal);
    // readonly dla typow referencyjnych blokuje mozliwosc stworzenia w tym miejcu nowego obiektu lub danie nulla

    public IReadOnlyCollection<ManifestEntry> Entries => _entries.Values;
    public int Count => _entries.Count;
    public long TotalSize => _entries.Values.Sum(e => e.Size);

    public void Add(ManifestEntry entry) => _entries[entry.Path] = entry;

    public bool Contains(string path) => _entries.ContainsKey(path);

    public ChangeKind Classify(ManifestEntry current)
    {
        if (!_entries.TryGetValue(current.Path, out ManifestEntry? previous))
            return ChangeKind.New;

        return previous.Hash == current.Hash
            ? ChangeKind.Unchanged // nie zmienilo hasha, wiec nie zmienilo tresci
            : ChangeKind.Modified;
    }

    // bieze manifest i zapisuje do strumienia
    public void WriteTo(Stream stream)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true); // bufor zostanie oprozniony, ale stream nie zostanie zamkniety
        // jesli bez using, to trzeba writer.Flush(), bo bufor moze nie byc zapelniony i nic sie nie zapisze wtedy bez Flush
        writer.WriteLine("# Snapshot manifest v1");

        foreach (ManifestEntry entry in _entries.Values)
            writer.WriteLine($"{entry.Hash} {entry.Size} {entry.Path}");
    }

    // bieze strumien i zapisuje w obiekcie manifest
    public static Manifest ReadFrom(Stream stream)
    {
        var manifest = new Manifest();
        using var reader = new StreamReader(stream, leaveOpen: true);

        while (reader.ReadLine() is { } line) // czytaj linijke, jesli nie jest null, to zapisz ja do line
        // bezpieczniej: line = reader.ReadLine()) != null
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) // pomijamy
                continue;

            string[] parts = line.Split(' ', 3); // zapisujemy hash, rozmiar, a reszte dajemy do parts[2], bo moze zawierac spacje
            if (parts.Length != 3 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[2]) || !long.TryParse(parts[1], out long size))
            // Parse zamienia parts[1] na longa, jesli sie uda to zwraca true i zapisuje liczbe do size, jesli sie nie uda to zwraca false i 0
            {
                throw new InvalidDataException($"Invalid manifest line: '{line}'");
            }

            manifest.Add(new ManifestEntry(parts[2], size, parts[0]));
        }

        return manifest;
    }

    public void Save(string path)
    {
        using FileStream stream = File.Create(path);
        WriteTo(stream);
    }

    public static Manifest Load(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return ReadFrom(stream);
    }
}