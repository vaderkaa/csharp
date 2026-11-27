# System plików w C# (`System.IO`)

## 1. Ścieżki (`Path`)

* **`Path.Combine`** – bezpiecznie łączy fragmenty ścieżki (dobiera `\` na Windowsie lub `/` na Linuxie).
  ```csharp
  string path = Path.Combine("C:\\sample", "docs", "guide.md");
  // Wynik: C:\sample\docs\guide.md
  ```

* **`Path.GetRelativePath` / `Path.GetFullPath`** – zamienia ścieżkę pełną na względną względem katalogu bazowego (i odwrotnie).
  ```csharp
  string rel = Path.GetRelativePath("../sample", "../sample/docs/guide.md");
  string full = Path.GetFullPath("../sample");
  ```

* **`Path.GetFileName` / `GetExtension` / `GetDirectoryName`** – wyciąga nazwę pliku, rozszerzenie lub folder nadrzędny ze stringa.
  ```csharp
  string name = Path.GetFileName("docs/guide.md"); // "guide.md"
  string ext = Path.GetExtension("docs/guide.md"); // ".md"
  ```

---

## 2. Katalogi (`Directory`)

* **`Directory.CreateDirectory`** – tworzy katalog wraz ze wszystkimi brakującymi folderami pośrednimi (nie rzuca błędu, jeśli już istnieje).
  ```csharp
  Directory.CreateDirectory("../backups/2026-11-27");
  bool exists = Directory.Exists("../backups/2026-11-27");
  ```

* **`Directory.EnumerateFiles`** – leniwie zwraca ścieżki plików (nie ładuje wszystkiego naraz do RAM-u jak `GetFiles`).
  ```csharp
  foreach (string f in Directory.EnumerateFiles("../sample", "*", SearchOption.AllDirectories))
      Console.WriteLine(f);
  ```

* **`Directory.Delete`** – domyślnie usuwa tylko pusty katalog (rzuca `IOException`, jeśli nie jest pusty); z `recursive: true` usuwa też zawartość.
  ```csharp
  if (Directory.Exists("../backups"))
      Directory.Delete("../backups", recursive: true);
  ```

---

## 3. Pliki i metadane (`File` vs `FileInfo`)

* **`File.Exists` / `Copy` / `Delete`** – jednorazowe operacje na plikach.
  ```csharp
  if (File.Exists("a.txt"))
      File.Copy("a.txt", "b.txt", overwrite: true);
  ```

* **`File.ReadAllLines` / `WriteAllLines` (oraz `ReadAllText`)** – szybki odczyt i zapis małych plików tekstowych w całości.
  ```csharp
  string[] lines = File.ReadAllLines("latest.manifest");
  File.WriteAllLines("copy.manifest", lines);
  ```

* **`FileInfo`** – klasa instancyjna do odczytu metadanych pliku (rozmiar w bajtach, data modyfikacji).
  ```csharp
  var info = new FileInfo("../sample/data/measurements.csv");
  Console.WriteLine($"{info.Length} B, zmodyfikowano: {info.LastWriteTimeUtc}");
  ```

---

## 4. Strumienie (`Stream`) – duże pliki i `using`

* **`FileStream`** – niskopoziomowy dostęp do bajtów; zawsze w `using`, żeby zwolnić uchwyt pliku w systemie.
  ```csharp
  using FileStream fs = File.OpenRead("duzy_plik.bin");
  byte[] hash = SHA256.HashData(fs);
  ```

* **`StreamReader` / `StreamWriter`** – czytanie i dopisywanie tekstu kawałek po kawałku (domyślnie UTF-8).
  ```csharp
  using var writer = new StreamWriter("log.txt", append: true);
  writer.WriteLine("Nowy wpis w logu");
  ```

---

## 5. Nasłuchiwanie zmian na żywo (`FileSystemWatcher`)

* **Konfiguracja i start** – monitoruje folder w tle i zgłasza zdarzenia (`IDisposable` – wymaga `using`).
  ```csharp
  using var watcher = new FileSystemWatcher("../sample") { IncludeSubdirectories = true };
  watcher.EnableRaisingEvents = true;
  ```

* **Obsługa zdarzeń (`Created`, `Changed`, `Deleted`, `Renamed`)** – podpina reakcje jak w `BackupEngine`.
  ```csharp
  watcher.Created += (_, e) => Console.WriteLine($"+ Dodano: {e.Name}");
  watcher.Renamed += (_, e) => Console.WriteLine($"R {e.OldName} -> {e.Name}");
  ```
