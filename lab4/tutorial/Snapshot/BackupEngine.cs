namespace Snapshot;

public class BackupEngine(string sourceDirectory, string repositoryDirectory, IReadOnlyList<string> ignorePatterns)
{
    public string SourceDirectory { get; } = sourceDirectory;
    public string RepositoryDirectory { get; } = repositoryDirectory;
    // nie tworze publicznej wlasciwosci dla ignorePatterns, ale zostanie ono zapamietane w klasie i bede mogla go uzywac

    // 4 zdarzenia
    // wywoływane dla każdego znalezionego pliku, przed jego przetworzeniem, subskrybent może wykluczyć plik z kopii zapasowej
    public event EventHandler<FileFoundEventArgs>? FileFound;
    // wywoływane po obliczeniu skrótu pliku i jego sklasyfikowaniu
    public event EventHandler<FileProcessedEventArgs>? FileProcessed;
    // wywoływane po każdym przetworzonym pliku
    public event EventHandler<BackupProgressEventArgs>? ProgressChanged;
    // wywoływane na końcu kopii zapasowej
    public event EventHandler<BackupCompletedEventArgs>? Completed;

    protected virtual void OnFileFound(FileFoundEventArgs e) => FileFound?.Invoke(this, e);
    protected virtual void OnFileProcessed(FileProcessedEventArgs e) => FileProcessed?.Invoke(this, e);
    protected virtual void OnProgressChanged(BackupProgressEventArgs e) => ProgressChanged?.Invoke(this, e);
    protected virtual void OnCompleted(BackupCompletedEventArgs e) => Completed?.Invoke(this, e);

    public BackupSummary Run()
    {
        // SKANOWANIE PLIKOW W MOIM FOLDERZE
        var scanner = new DirectoryScanner(SourceDirectory, ignorePatterns);
        var scannedFiles = scanner.Scan()
                                  .OrderBy(f => f.RelativePath, StringComparer.Ordinal); // sortuje po sciezce wzglednej

        var files = new List<FileEntry>();
        foreach (FileEntry file in scannedFiles)
        {
            var foundArgs = new FileFoundEventArgs(file);
            OnFileFound(foundArgs); // wywoluje lambdy z Program.cs
            // czyli subskrybentow, ktorzy zostali podpieci do zdarzenia FileFound w klasie BackupEngine
            // zapisuje sie w Program.cs engine.FileFound += lambda
            // jesli ktorys subskrybent ma w swojej logice Console.WriteLine to sie wypisze

            if (!foundArgs.Cancel) // jesli ktorys subskrybent zwrocil cancel, to nie zapisujemy tego pliku
                files.Add(file);
        }

        int totalFiles = files.Count;
        long totalBytes = files.Sum(f => f.Size);

        string latestManifestPath = Path.Combine(RepositoryDirectory, "latest.manifest");
        Manifest? previousManifest = File.Exists(latestManifestPath)
                                     ? Manifest.Load(latestManifestPath) // wywoluje na KLASIE, bo jest STATYCZNA i dostaje obiekt Manifest
                                     : null; // nie bylo wczesniej manifestu, CZYLI LISTA PLIKOW Z NAZWAMI, ZHASHOWANA TRESCIA I ROZMIAREM

        var currentManifest = new Manifest(); // tworze nowy manifest
        var changes = new List<FileChange>(); // lista zmian

        int processedFiles = 0;
        long processedBytes = 0;

        foreach (FileEntry file in files)
        {
            string hash = FileHasher.ComputeSha256(file.FullPath);
            var entry = new ManifestEntry(file.RelativePath, file.Size, hash);
            currentManifest.Add(entry); // dodaje do Manifest ManifestEntry czyli dodaje informacje o tym pliku do listy jakby

            // jesli nie bylo poprzedniego manifestu, to plik jest nowy
            ChangeKind kind = previousManifest is null
                              ? ChangeKind.New
                              : previousManifest.Classify(entry); // porownujemy dzisiejszy plik z poprzednim i jak sie zmienil status

            changes.Add(new FileChange(file.RelativePath, kind));
            OnFileProcessed(new FileProcessedEventArgs(file, kind)); // wywolanie subskybentow zdarzenia po zupdateowaniu plikow

            processedFiles++;
            processedBytes += file.Size;
            OnProgressChanged(new BackupProgressEventArgs(processedFiles, totalFiles, processedBytes, totalBytes));
            // zaktualizowanie paska postepu
        }

        // WYKRYCIE USUNIETYCH PLIKOW
        if (previousManifest is not null)
        {
            foreach (ManifestEntry prevEntry in previousManifest.Entries)
            {
                if (!currentManifest.Contains(prevEntry.Path)) // w poprzedniej liscie plikow nie ma tego pliku
                {
                    changes.Add(new FileChange(prevEntry.Path, ChangeKind.Deleted)); // dodaje zmiany, ze usuniety
                    // jaki plik i jaki nowy status do listy
                }
            }
        }

        // bez tego usuniete pliki bylyby na samym koncu
        changes.Sort((a, b) => StringComparer.Ordinal.Compare(a.Path, b.Path));
        // posortuj porownujac sciezki plikow

        // utworzenie folderu repozytorium i zapisanie nowego manifestu jako latest.manifest, a stary zostaje nadpisany
        Directory.CreateDirectory(RepositoryDirectory);
        currentManifest.Save(latestManifestPath);

        var summary = new BackupSummary(changes);
        OnCompleted(new BackupCompletedEventArgs(summary)); // wrzucane dane i wywoluje subskrybentow dla tych danych

        return summary; // mozna potem wyjac zmiany, pojedyncze liczby albo zrobic ToString
    }
}