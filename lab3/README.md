# Kowariancja i kontrawariancja

### Kowariancja (out — tylko WYJŚCIE)

Zasada (Jabłka ➡️ Owoce): Pojemnik z Jabłkami możesz bezpiecznie traktować jako ogólny pojemnik z Owocami.

Mechanika: Przypisujesz typ szczegółowy do zmiennej wymagającej typu ogólnego.

Warunek: Dane mogą z obiektu tylko wychodzić. Nie da się niczego dodać do środka, więc nie ma ryzyka, że ktoś przez pomyłkę wrzuci Gruszkę do listy Jabłek.

```csharp
// Metoda oczekuje zbioru ogólnych Rzeczy (object), by je tylko odczytać (IEnumerable ma 'out')
public void WypiszWszystko(IEnumerable<object> elementy) { /* ... */ }

// MAIN:
List<string> teksty = new List<string> { "Ala", "Kot" };

// ZADZIAŁA: Skoro metoda tylko czyta, może bezpiecznie odczytać teksty jako "ogólne obiekty"
WypiszWszystko(teksty);
```

### Kontrawariancja (in — tylko WEJŚCIE)

Zasada (Umyj Owoce ➡️ Umyj Jabłka): Narzędzie, które potrafi Umyć Dowolne Owoce, możesz bezpiecznie zatrudnić na stanowisku maszyny do Mycia Jabłek.

Mechanika: Przypisujesz narzędzie ogólne do zmiennej wymagającej narzędzia szczegółowego.

Warunek: Obiekt to "czarna dziura" – tylko przyjmuje dane do obróbki (jako parametry) i niczego nie zwraca.

```csharp
class Owoc { }
class Jablko : Owoc { }

// Interfejs z 'in' - gwarantuje, że obiekty do myjni tylko WCHODZĄ
interface IMyjnia<in T> 
{ 
    void Umyj(T obiekt); 
}

// Kompletny kod uniwersalnej myjni (potrafi umyć absolutnie KAŻDY Owoc)
class MyjniaOwocow : IMyjnia<Owoc> 
{ 
    public void Umyj(Owoc o) { Console.WriteLine("Myję owoc!"); } 
}

// Metoda twardo żąda narzędzia, które umie myć Jabłka
void WypucujMojeJablko(IMyjnia<Jablko> myjniaTylkoDlaJablek) 
{ 
    myjniaTylkoDlaJablek.Umyj(new Jablko());
}

// MAIN: Tworzysz uniwersalną myjnię do wszystkich owoców
IMyjnia<Owoc> uniwersalnaMyjnia = new MyjniaOwocow();

// ZADZIAŁA: Skoro uniwersalna myjnia umyje dowolny owoc, poradzi sobie z Jabłkiem
WypucujMojeJablko(uniwersalnaMyjnia);
```

# Rekordy

**1. Co to jest `record`?**

To skrót składniowy do tworzenia paczek na dane. Piszesz jedną linijkę, a kompilator odwala czarną robotę. Sam w tle pisze dla Ciebie:

* Konstruktor.
* Mądre sprawdzanie równości (`==` i `Equals` porównują *zawartość*, a nie fizyczne adresy w pamięci).
* Ładne formatowanie tekstu w `ToString()`.

**2. Trzy główne warianty (i różnice):**

**`record`** (domyślnie `record class`)
* Gdzie żyje: Na stercie (obciąża Garbage Collectora).
* Mutowalność: **Niemutowalny** (dane tylko do odczytu, modyfikator `init`).
* Zastosowanie: Standardowe przesyłanie paczek danych (np. profil użytkownika).

`init` (od initialize, czyli inicjuj) to rygorystyczna alternatywa dla set. Działa jak szybkoschnący beton:   
Pozwala ustawić wartość tylko w momencie powoływania obiektu do życia (gdy używasz słówka `new`).   
Ułamek sekundy później, gdy obiekt już powstanie, init zatrzaskuje kłódkę na zawsze. Wartość staje się zablokowana i można ją tylko czytać.

**`record struct`**
* Gdzie żyje: Na stosie (superszybki, znika od razu, nie męczy GC).
* Mutowalność: **Mutowalny!** (można mu podmieniać dane w locie przez `set`).
* Zastosowanie: Matematyka, gry, generowanie milionów punktów na sekundę, gdy chcesz je w locie modyfikować.

**`readonly record struct`**
* Gdzie żyje: Na stosie (superszybki, zero GC).
* Mutowalność: **Niemutowalny** (całkowicie zablokowany, modyfikator `init`).
* Zastosowanie: Złoty środek dla silników gier i matematyki (np. wektory w Raytracerze). Masz maksymalną wydajność z pamięci podręcznej i 100% pewności, że żadna inna funkcja nie zmieni Ci przypadkiem koordynatów wektora w trakcie obliczeń.

# Wzorce list i wycinków

Najważniejsza zasada: Wyrażenie `switch` działa jak lejek (czyta z góry na dół).   
Wygrywa pierwszy warunek, który pasuje, a reszta jest ignorowana. Dlatego układamy je od najbardziej szczegółowych (ścisła długość listy), do najbardziej ogólnych (jakiś element na początku/końcu).

```csharp
public string AnalizujOdczyty(List<double> odczyty)
{
    return odczyty switch
    {
        [] => "empty",                                // Dokładnie 0 elementów
        [_] => "single",                              // Dokładnie 1 dowolny element
        [var v1, var v2] when v1 < v2 => "pair-asc",  // Dokładnie 2 elementy (rosnąco)
        [var v1, var v2] when v1 > v2 => "pair-desc", // Dokładnie 2 elementy (malejąco)
        [var v1, var v2] when v1 == v2 => "pair-eq",  // Dokładnie 2 elementy (takie same)
        [> 100, ..] => "starts-high",                 // Pierwszy to >100, a potem cokolwiek
        [.., < 0] => "ends-low",                      // Cokolwiek na początku, ale ostatni to <0
        _ => "other"                                  // Wszystko, co nie złapało się wyżej
    };
}
```

**Słownik pojęć:**
* `[ ]` (klamry) – wyznaczają ścisłą długość kolekcji (jeśli nie ma w nich `..`).
* `_` (wewnątrz klamer) – jeden dowolny element ("joker").
* `..` (spread/wycinek) – połyka 0 lub więcej elementów. Mówi: "tutaj może być cokolwiek aż do końca/początku".
* `var x` – wyciąga wartość z tablicy w locie do zmiennej x.
* `when` – dodaje klasyczny warunek logiczny po udanym dopasowaniu długości.
* `_ (przed => "other")` – domyślny default, wyłapuje wszystko, co zostało.

# Porównywanie elementów kolekcji

Polecenie: "bigger packages should come before smaller - if packages have the same size order by Name".

Tam będziesz musiała zaimplementować tzw. logikę zapasową (fallback), która ratuje sytuację podczas remisu:

```csharp
public int CompareTo(Package? other) {
    if (other is null) return 1;

    // KROK 1: Sortujemy po rozmiarze (MALEJĄCO, bo większe mają być pierwsze - dlatego other jest z przodu!)
    int wynik = other.DownloadKb.CompareTo(this.DownloadKb);
    
    // Jeśli nie ma remisu (wynik jest różny od zera), od razu wiemy kto wygrał. Koniec roboty.
    if (wynik != 0) {
        return wynik;
    }

    // KROK 2: Skoro procesor doszedł aż tutaj, znaczy że był remis w rozmiarze (wynik = 0).
    // Wkracza zasada zapasowa - decyduje nazwa.
    return string.Compare(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
}
```

**1. Kierunek sortowania (Złota zasada)**

Algorytm domyślnie sortuje rosnąco, więc sterujesz nim, po prostu odwracając kolejność zmiennych w kodzie:

Rosnąco (A-Z, 1-10): Pytasz "JA do NIEGO"   
`return this.Wiek.CompareTo(other.Wiek);`

Malejąco (Z-A, 10-1): Pytasz "ON do MNIE" (zamieniasz miejscami)   
`return other.Rozmiar.CompareTo(this.Rozmiar);`

**2. Odporność na Nulle (Kto przeżyje, a kto wybuchnie)**

Zasada nadrzędna: null jest "najmniejszy" (przegrywa ze wszystkim).

Wybucha (Brak odporności): `obiekt.CompareTo(inny)`   
Uderzenie w kropkę po pustej zmiennej to pewny `NullReferenceException`. Zawsze musisz najpierw sprawdzić: `if (obiekt is null)`.

Kuloodporne (Pełna odporność): `string.Compare(a, b)`   
Metoda statyczna bezpiecznie przyjmuje nulle i nic nie wybucha. Dodatkowo pozwala wstawić flagę np. `StringComparison.InvariantCultureIgnoreCase`, by ignorować wielkość liter.

**3. Dwa interfejsy (Kto ocenia)**

`IComparable<T>` (Zdolność wrodzona / Czarna skrzynka):   
Klasa potrafi posortować się sama. Skoro kod wykonuje się wewnątrz niej (`this`), to Ty na 100% istniejesz. Sprawdzasz tylko przeciwnika:   
`if (other is null) return 1;` (Ja > Pustka).

`IComparer<T>` (Zewnętrzny sędzia):   
Zupełnie osobna klasa-sędzia. Sędzia dostaje `x` i `y` z zewnątrz. Musi zabezpieczyć obie strony:   
`if (ReferenceEquals(x, y)) return 0;` (remis / oba to null)
`if (x is null) return -1;` (lewy przegrywa)
`if (y is null) return 1;` (prawy przegrywa)

**4. Łączenie reguł (Kaskada przy remisie)**

Jeśli sortujesz po kilku cechach (np. najpierw rozmiar, potem nazwa), drugą cechę sprawdzasz tylko wtedy, gdy pierwsza zwróciła 0:

```csharp
int wynik = other.Rozmiar.CompareTo(this.Rozmiar); // Malejąco
if (wynik != 0) return wynik;                      // Mamy zwycięzcę
return string.Compare(this.Name, other.Name);      // Remis? Decyduje nazwa
```

```csharp
// if I don't own Person, but still need to order them by age in some collection
public class PersonByAgeAscending : IComparer<Person> {
    public int Compare(Person? a, Person? b) {
        if (a is null) return -1;
        if (b is null) return 1;
        return a.Age.CompareTo(b.Age);
}

List<Person> people = [ new("A", 30), new("B", 20) ];
people.Sort(new PersonByAgeAscending());
```

Kiedy tego używasz?

* Gdy klasa jest cudza i nie możesz jej edytować.
* Gdy dla jednej klasy potrzebujesz wielu różnych sortowań. Raz chcesz posortować ludzi po wieku (wtedy dajesz sędziego PersonByAge), a w innej części programu po nazwisku (wtedy tworzysz drugiego sędziego PersonByName i wrzucasz go do Sort()). Zdolność "wrodzona" (IComparable) pozwala zdefiniować tylko jeden, domyślny sposób dla całego programu.

**Klasa Comparer<T> zamiast interfejsu IComparer<T>**

Dlaczego to oficjalnie zalecane?

Interfejs (`IComparer<T>`) to tylko goły kontrakt. Jeśli go użyjesz, Twój sędzia dogada się tylko z nowoczesnymi, generycznymi listami.

Klasa bazowa (`Comparer<T>`) to gotowy szablon od Microsoftu. W tle, całkowicie za darmo, dopisuje kod zapewniający kompatybilność wsteczną. Dzięki temu Twój sędzia obsłuży również przestarzałe kolekcje i kody napisane we wczesnych wersjach C#.

Co się zmienia w kodzie?
Cała matematyka i obsługa nulli w środku zostaje w 100% taka sama. Zmieniają się tylko dwa słowa w nagłówkach (usuwasz literkę "I" z nazwy oraz dodajesz `override`):

```csharp
// BYŁO (Goły interfejs):
public class PersonByAgeAscending : IComparer<Person> {
    public int Compare(Person? a, Person? b) { ... }
}

// JEST (Zalecana klasa bazowa):
public class PersonByAgeAscending : Comparer<Person> {
    public override int Compare(Person? a, Person? b) { ... }
}
```

Oto jak wyglądałby kod w `Main` (głównym programie) i jak krok po kroku współpracuje on z Twoją nową maszyną z zadania.

Zapiszmy w `Main` takie wywołanie: chcemy **3 odczyty** z czujnika "Termometr", zaczynając od wartości **20.0** i godziny **12:00**.

```csharp
// 1. Ustawienie maszyny na hali (nic się jeszcze nie produkuje!)
var mojeOdczyty = T2.Repeat("Termometr", 20.0, 3, new DateTime(2026, 1, 1, 12, 0, 0));

// 2. Szef przychodzi i zaczyna ciągnąć za dźwignię
foreach (var odczyt in mojeOdczyty) {
    Console.WriteLine($"Czujnik: {odczyt.Sensor}, Wynik: {odczyt.Value}, Czas: {odczyt.At}");
}

```

A teraz co dokładnie robi procesor (nasz ping-pong między `Main` a `T2`):

**KROK 1: Tylko obietnica**
Kiedy procesor czyta linijkę `var mojeOdczyty = T2.Repeat(...)`, funkcja `T2` w ogóle nie startuje. Przekazałeś jej nazwę, wartość startową, limit (`count`) i czas startowy, ale na razie maszyna tylko czeka w gotowości. Pamięć RAM jest pusta.

**KROK 2: "Daj pierwszy!" (Obieg 1)**

* **[Main]** Program wchodzi w pętlę `foreach`. Pętla krzyczy do maszyny: *"Daj pierwszy odczyt!"*
* **[T2]** Maszyna się włącza. Ustawia sobie w środku zmienne robocze. Robi pierwszy obieg pętli `for` i skleja odczyt: `Termometr, 20.0, 12:00`. Robi `yield return`, wypluwa ten odczyt na taśmę i **zamraża się**.
* **[Main]** Program bierze z taśmy odczyt i wypisuje na ekran: `Czujnik: Termometr, Wynik: 20, Czas: 12:00`.

**KROK 3: "Daj drugi!" (Obieg 2)**

* **[Main]** Pętla kręci się dalej i krzyczy: *"Daj drugi!"*
* **[T2]** Maszyna się odwiesza. Dodaje do swojego wewnętrznego zegara 2 minuty, a do wartości dodaje 0,2. Robi drugi obieg pętli `for` i skleja: `Termometr, 20.2, 12:02`. Wypluwa przez `yield return` i znowu **zamarza**.


* **[Main]** Program bierze wynik i wypisuje: `Czujnik: Termometr, Wynik: 20.2, Czas: 12:02`.

**KROK 4: "Daj trzeci!" (Obieg 3)**

* **[Main]** Pętla krzyczy: *"Daj trzeci!"*
* **[T2]** Maszyna się odwiesza. Znowu robi matematykę. Skleja: `Termometr, 20.4, 12:04`. Wypluwa i **zamarza**.
* **[Main]** Program bierze wynik i wypisuje: `Czujnik: Termometr, Wynik: 20.4, Czas: 12:04`.

**KROK 5: Koniec pracy**

* **[Main]** Pętla krzyczy: *"Daj czwarty!"*
* **[T2]** Maszyna się odwiesza, ale widzi, że limit `count` wynosił 3. Pętla `for` w maszynie dobiegła końca. Maszyna wyłącza się całkowicie i mówi pętli w `Main`: *"Koniec, taśma pusta"*.


* **[Main]** Zewnętrzny `foreach` kończy działanie. Program idzie dalej.

Słówko `yield` sprawia po prostu, że podpinasz swoją zewnętrzną pętlę `foreach` (w `Main`) bezpośrednio pod wewnętrzną pętlę `for` (w `T2`) i kręcą się one na zmianę, sztuka po sztuce.
