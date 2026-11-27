# System plików i strumienie w C# (`System.IO`)

## 1. Ścieżki (`Path`)

* **`Path.Combine` vs `Path.Join`** – `Combine` ignoruje wcześniejsze segmenty, jeśli kolejny jest ścieżką bezwzględną (ryzyko luki bezpieczeństwa); `Join` zawsze skleja je separatorem.
  ```csharp
  string bad = Path.Combine("data", "/etc/passwd"); // "/etc/passwd"
  string safe = Path.Join("data", "/etc/passwd");   // "data/etc/passwd"
  ```

* **`Path.GetRelativePath` / `Path.GetFullPath`** – zamienia ścieżkę pełną na względną względem katalogu bazowego (i odwrotnie).
  ```csharp
  string rel = Path.GetRelativePath("../sample", "../sample/docs/guide.md");
  string full = Path.GetFullPath("../sample");
  ```

* **`Path.GetFileName` / `GetExtension` / `ChangeExtension`** – wyciąga lub podmienia elementy nazwy pliku.
  ```csharp
  string name = Path.GetFileNameWithoutExtension("docs/guide.md"); // "guide"
  string bak = Path.ChangeExtension("docs/guide.md", ".bak");      // "docs/guide.bak"
  ```

* **Pliki tymczasowe i znaki systemowe** – pobieranie katalogu tymczasowego, losowych nazw i separatora systemu (`\` lub `/`).
  ```csharp
  string tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
  char sep = Path.DirectorySeparatorChar;
  ```

---

## 2. Pliki i metadane (`File` vs `FileInfo`)

* **`File.Exists` / `Copy` / `Move` / `Delete`** – podstawowe, jednorazowe operacje na plikach.
  ```csharp
  File.Copy("a.txt", "b.txt", overwrite: true);
  File.Move("b.txt", "c.txt", overwrite: true);
  ```

* **`File.ReadAllLines` / `WriteAllLines` vs `File.ReadLines`** – `ReadAll*` ładuje cały plik do RAM-u; `ReadLines` czyta leniwie linia po linii (lepsze dla dużych plików).
  ```csharp
  int errors = File.ReadLines("log.txt").Count(l => l.Contains("Error"));
  File.WriteAllLines("copy.manifest", File.ReadAllLines("latest.manifest"));
  ```

* **Atrybuty, czas i uprawnienia Unix (`File`)** – odczyt/zapis znaczników czasu oraz trybu dostępu (`chmod` w .NET 7+).
  ```csharp
  DateTime modified = File.GetLastWriteTimeUtc("a.txt");
  File.SetUnixFileMode("script.sh", UnixFileMode.UserRead | UnixFileMode.UserExecute);
  ```

* **`FileInfo`** – klasa instancyjna reprezentująca konkretny plik; wygodna do pobierania rozmiaru (`Length`) i metadanych.
  ```csharp
  var info = new FileInfo("../sample/data/measurements.csv");
  Console.WriteLine($"{info.Length} B, read-only: {info.IsReadOnly}");
  ```

---

## 3. Katalogi (`Directory` vs `DirectoryInfo`)

* **`Directory.CreateDirectory` / `Delete`** – `CreateDirectory` tworzy też brakujące foldery nadrzędne; `Delete` bez `recursive: true` usuwa tylko pusty katalog.
  ```csharp
  Directory.CreateDirectory("../backups/2026-11-27");
  Directory.Delete("../backups", recursive: true);
  ```

* **`EnumerateFiles` vs `GetFiles`** – `Enumerate*` działa leniwie (zwraca `IEnumerable<string>` po jednym elemencie, oszczędza RAM), `Get*` ładuje całą tablicę naraz.
  ```csharp
  foreach (string f in Directory.EnumerateFiles(".", "*.pdf", SearchOption.AllDirectories))
      Console.WriteLine(f);
  ```

* **Katalog bieżący i tymczasowy (`Directory`)** – sprawdzenie/zmiana katalogu roboczego oraz tworzenie unikalnego folderu w `temp`.
  ```csharp
  string cwd = Directory.GetCurrentDirectory();
  DirectoryInfo tempDir = Directory.CreateTempSubdirectory();
  ```

* **`DirectoryInfo`** – klasa instancyjna katalogu; pozwala łatwo przejść do rodzica (`Parent`) lub utworzyć podkatalog (`CreateSubdirectory`).
  ```csharp
  var dir = new DirectoryInfo(".");
  DirectoryInfo sub = dir.CreateSubdirectory("Subdir");
  ```

---

## 4. Katalogi specjalne i dyski (`Environment`, `DriveInfo`)

* **`Environment.GetFolderPath`** – pobiera ścieżkę do folderów systemowych (np. katalog domowy, Dokumenty) niezależnie od OS.
  ```csharp
  string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
  string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
  ```

* **`DriveInfo`** – sprawdza zamontowane partycje oraz wolne miejsce na dysku.
  ```csharp
  var root = new DriveInfo("/"); // na Windows: new DriveInfo("C")
  Console.WriteLine($"Wolne: {root.AvailableFreeSpace / (1024 * 1024 * 1024)} GB");
  ```

---

## 5. Strumienie (`Stream`, `FileStream`, `RandomAccess`)

* **Możliwości (`CanRead`, `CanWrite`, `CanSeek`) i błędy** – w odróżnieniu od POSIX (`errno`), błędy zgłaszane są wyjątkami (`IOException`, `FileNotFoundException`).
  ```csharp
  using Stream s = new FileStream("lorem.txt", FileMode.Open);
  Console.WriteLine($"Read: {s.CanRead}, Seek: {s.CanSeek}");
  ```

* **Odczyt i zapis bajtów (`Read`, `ReadExactly`, `Write`)** – `Read` zwraca `0` na końcu strumienia; `ReadExactly` (.NET 7) gwarantuje wczytanie pełnego bufora.
  ```csharp
  byte[] header = new byte[16];
  s.ReadExactly(header); // rzuca EndOfStreamException, jeśli plik jest krótszy
  ```

* **Pozycjonowanie (`Seek`, `Position`) i `RandomAccess` (.NET 6)** – `Seek` przesuwa kursor; `RandomAccess` czyta/pisze pod offsetem bez zmiany pozycji (bezpieczne wielowątkowo).
  ```csharp
  using var handle = File.OpenHandle("lorem.txt", FileMode.Open);
  int read = RandomAccess.Read(handle, header, fileOffset: 100);
  ```

* **Buforowanie, `Flush` i `using`** – `Flush(flushToDisk: true)` wymusza zapis na dysk (jak `fsync`); `using` automatycznie wywołuje `Dispose()` / `Close()`.
  ```csharp
  using FileStream fs = File.OpenWrite("out.bin"); // OpenOrCreate, NIE ucina pliku!
  fs.Flush(flushToDisk: true);
  ```

---

## 6. Dekoratory i adaptery strumieni

* **Dekoratory (`BufferedStream`, `GZipStream`, `BrotliStream`)** – opakowują inny strumień, dodając buforowanie lub kompresję; `leaveOpen: true` nie zamyka strumienia pod spodem.
  ```csharp
  using var ms = new MemoryStream();
  using (var gz = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
      gz.Write(new byte[1000]);
  ```

* **Adaptery tekstu (`StreamReader`, `StreamWriter`)** – czytają i piszą znaki/linie (domyślnie UTF-8 bez BOM; `StreamReader` rozpoznaje kodowanie po BOM).
  ```csharp
  using var sw = new StreamWriter(File.Create("out.txt"), Encoding.UTF8);
  sw.WriteLine("zażółć gęślą jaźń");
  ```

* **Adaptery binarne (`BinaryReader`, `BinaryWriter`, `BinaryPrimitives`)** – serializują typy proste w formacie *little-endian*; do *big-endian* służy `BinaryPrimitives`.
  ```csharp
  using var bw = new BinaryWriter(File.Create("player.bin"));
  bw.Write("Bob"); bw.Write(100); // zapisuje string (z długością) oraz Int32
  ```

---

## 7. Nasłuchiwanie zmian na żywo (`FileSystemWatcher`)

* **Konfiguracja i start** – monitoruje folder w tle na **osobnym wątku** (`inotify` na Linuxie); implementuje `IDisposable` (wymaga `using`).
  ```csharp
  using var watcher = new FileSystemWatcher("../sample", "*") { IncludeSubdirectories = true };
  watcher.EnableRaisingEvents = true;
  ```

* **Obsługa zmian (`Created`, `Changed`, `Deleted`, `Renamed`)** – `Renamed` używa `RenamedEventArgs` (`OldFullPath` i `FullPath`).
  ```csharp
  watcher.Created += (_, e) => Console.WriteLine($"+ Dodano: {e.FullPath}");
  watcher.Renamed += (_, e) => Console.WriteLine($"R {e.OldFullPath} -> {e.FullPath}");
  ```

* **Zdarzenie `Error`** – informuje o wewnętrznym błędzie watchera (np. przepełnieniu bufora przy zbyt wielu zmianach naraz i zgubieniu zdarzeń).
  ```csharp
  watcher.Error += (_, e) => Console.WriteLine(e.GetException().Message);
  ```

---

## 8. Archiwa ZIP (`System.IO.Compression`)

* **`ZipFile.CreateFromDirectory` / `ExtractToDirectory`** – szybkie pakowanie i rozpakowywanie całych folderów (rzuca błąd, jeśli cel już istnieje).
  ```csharp
  ZipFile.CreateFromDirectory("my_files", "archive.zip");
  ZipFile.ExtractToDirectory("archive.zip", "extracted_files");
  ```

* **`ZipFile.Open` i `ZipArchive`** – otwiera archiwum w trybie `Read`, `Create` lub `Update` do pracy wpis po wpisie (`Entries`, `GetEntry`, `CreateEntryFromFile`).
  ```csharp
  using ZipArchive zip = ZipFile.Open("archive.zip", ZipArchiveMode.Update);
  zip.CreateEntryFromFile("nowy.txt", "folder/nowy.txt");
  ```

* **`ZipArchiveEntry` (`Open`, `ExtractToFile`, `Delete`)** – operacje na pojedynczym pliku wewnątrz archiwum.
  ```csharp
  ZipArchiveEntry? entry = zip.GetEntry("folder/nowy.txt");
  entry?.ExtractToFile("wypakowany.txt", overwrite: true);
  ```

* **Asynchroniczne API ZIP (.NET 10)** – nieblokujące odpowiedniki (`CreateFromDirectoryAsync`, `OpenReadAsync`, `entry.OpenAsync`) z obsługą `await using`.
  ```csharp
  await using ZipArchive zip = await ZipFile.OpenReadAsync("archive.zip");
  await zip.ExtractToDirectoryAsync("extracted_async");
  ```
