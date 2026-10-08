### Polecenia gita po kolei:
`dotnet new console` – tworzy szkielet aplikacji.
`dotnet new gitignore` – natychmiast generuje plik blokujący wysyłanie "śmieci" (skompilowanych plików tymczasowych).
`git init` – tworzy lokalne repozytorium w tym folderze.
`git add .` – dodaje pliki do poczekalni (dzięki .gitignore wejdzie tylko czysty kod).
`git commit -m "Inicjalizacja"` – zatwierdza pliki w lokalnej historii.
`git remote add origin <link>` – łączy lokalny projekt z pustym miejscem na GitHubie.
`git branch -M main` – ustala domyślną nazwę głównej gałęzi.
`git push -u origin main` – wysyła wszystko na serwer.

### Tworzenie aplikacji .NET:
1. `dotnet new sln -n PrimeChecker` - Tworzy pusty plik nowej solucji.
2. `dotnet new classlib -n PrimeChecker.Lib` - Tworzy projekt nowej biblioteki klas.
3. `dotnet new console -n PrimeChecker.App` - Tworzy projekt głównej aplikacji konsolowej.
4. `dotnet new mstest -n PrimeChecker.Tests` - Tworzy nowy projekt testów jednostkowych.
5. `dotnet sln add PrimeChecker.Lib` - Dodaje plik biblioteki do solucji.
6. `dotnet sln add PrimeChecker.App` - Dodaje aplikację konsolową do solucji.
7. `dotnet sln add PrimeChecker.Tests` - Dodaje projekt testowy do solucji.
8. `dotnet add PrimeChecker.App reference PrimeChecker.Lib` - Łączy aplikację konsolową z biblioteką.
9. `dotnet add PrimeChecker.Tests reference PrimeChecker.Lib` - Łączy projekt testowy z biblioteką.
10. `dotnet build` - Kompiluje wszystkie projekty w solucji.
11. `dotnet run --project PrimeChecker.App`
12. `dotnet test`

### Asercje
`Assert.AreEqual(expected, actual);`
`Assert.AreEqual(expected, actual, delta); // for float comparisons`
`Assert.IsTrue(condition);`
`Assert.IsFalse(condition);`
`Assert.IsNull(value);`
`Assert.IsNotNull(value);`

### Nazwa testu
Nazwa testu powinna możliwie dokładnie opisywać sprawdzany przypadek. Jedną z popularnych konwencji jest:
`NazwaMetody_Scenariusz_OczekiwanyWynik`
Na przykład:
`CalculateArea_ValidDimensions_ReturnsCorrectArea`
`CalculateArea_OneSideIsZero_ReturnsZero`
