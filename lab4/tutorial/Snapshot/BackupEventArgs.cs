using System.ComponentModel; // dla CancelEventArgs

namespace Snapshot;

// dostarczenie informacji o znalezionym pliku i odebranie decyzji o anulowaniu backupu
public class FileFoundEventArgs(FileEntry file) : CancelEventArgs // dla wlasciwosci public bool Cancel { get; set; }
{
    public FileEntry File { get; } = file;
}

// BackupEngine oblicza hash, porownuje go z poprzednim manifestem i pakuje wynik w ta paczke danych dla eventu
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

    // ulamek ukonczenia od 0.0 do 1.0
    // trzeba rzutowac na double!!!
    // Backup Engine po przetworzeniu pliku zwieksza processedFiles i processedBytes
    // i pakuje wyniki w ta paczke danych dla ConsoleReporter
    public double Fraction => TotalBytes == 0 ? 1.0 : (double)ProcessedBytes / TotalBytes;
}

// gdy wszystkie pliki zostaly zhashowane, usuniete pliki wykryte i zapisany nowy manifest
// paczka danych dla powiadomienia po zakonczeniu calego procesu
public class BackupCompletedEventArgs(BackupSummary summary) : EventArgs
{
    public BackupSummary Summary { get; } = summary;
}
// BackupEngine na koncu Run() tworzy var summary = new BackupSummary(changes), pakuje go w BackupCompletedEventArgs
// i wywoluje OnCompleted na tej paczce
// ConsoleReporter odbiera to w OnCompleted i analizuje

// Completed(w BackupEngine) = lista numerów telefonów (subskrybentów).
// OnCompleted(w BackupEngine) = naciśnięcie przycisku „zadzwoń do wszystkich z listy Completed”.
// OnCompleted(w ConsoleReporter) = odebranie telefonu i wypisanie podsumowania w konsoli.