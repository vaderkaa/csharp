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

# Yield

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

# Nullowatość

Jeśli ktoś przekaże do metody tablice, która jest nullem:
```csharp
int?[] tablica = new int?[5]; // Tworzymy tablicę na 5 elementów (kopert)

// ŹLE: if (tablica.HasValue) -> Błąd kompilacji, tablice nie mają tego przełącznika!

// DOBRZE: 
// Najpierw upewniamy się, czy tablica (klasa) w ogóle istnieje
if (tablica is not null) {
    
    // Teraz sprawdzamy, czy w kopercie nr 0 są dane, używając jej przełącznika
    if (tablica[0].HasValue) {
        Console.WriteLine(tablica[0].Value);
    }
}
```

**Typy wartościowe, klasy i sztuczne nulle w C#**

* **Klasy (typy referencyjne np. tablice, `string`):** Zwykły `null` to fizyczny brak przypisanego miejsca w pamięci.
* **Struktury (typy wartościowe np. `int`, `DateTime`, `Reading`):** Nigdy nie są nullem, zawsze trzymają wartość.
* **Sztuczny null (`int?`, `Reading?`):** Znak zapytania owija strukturę w specjalną "kopertę" (typ `Nullable<T>`), pozwalając udawać, że zmienna jest pusta.

**Jak działa "koperta" (typy z `?`):**
Każda koperta ma wbudowane dwie właściwości:

* `.HasValue` (typ `bool`): `true` jeśli w środku faktycznie są dane, `false` jeśli to sztuczny null.
* `.Value`: fizyczne wyciągnięcie danych z koperty. (Wyciąganie, gdy `HasValue` to `false`, wywala krytyczny błąd programu).

**Kiedy używać `.Value`, a kiedy samej zmiennej?**

* **Sama zmienna (np. `Console.WriteLine(x)`):** Funkcje takie jak konsola są sprytne – same rozpakują kopertę i wypiszą wartość, a jeśli jest pusta, nic nie zrobią (nie wywalą błędu).
* **Konieczność `.Value` (np. `int normalnaLiczba = x.Value;`):** Musisz użyć `.Value`, gdy chcesz przypisać zawartość koperty do zwykłej, "twardej" zmiennej bez znaku zapytania (np. `int` zamiast `int?`) albo użyć jej do ścisłych obliczeń matematycznych. Kompilator nie pozwoli włożyć "całej koperty" w miejsce, które przyjmuje tylko gołe liczby.

**Sprawdzanie, czy nie ma nulla:**

* **Klasyczne (dla kopert):** `if (liczba.HasValue)`
* **Nowoczesne (uniwersalne):** `if (liczba is not null)` – działa wszędzie. Dla struktur kompilator i tak sam zamieni to pod spodem na sprawdzenie `.HasValue`.

**Podwójna pułapka z tablicami (`int?[] tablica`)**
Tu spotykają się dwa światy – klasa (tablica) i sztuczny null (szufladka z `?`).

1. Sama zmienna `tablica` nie ma właściwości `.HasValue`, bo tablica to klasa. Sprawdzasz ją przez `if (tablica is not null)`.
2. Wywołanie `int?[] tablica = new int?[5]` tworzy istniejący w pamięci obiekt (tablica już NIE JEST nullem), ale jej szufladki są z urzędu wypełnione pustymi kopertami (ich `HasValue` ustawione jest na `false`).

**Krótki zapis – operator Elvisa (`?.` i `?[]`)**
Pozwala przejść przez całą ścieżkę w locie, nie pisząc długich drabinek z `if`:

```csharp
int? bezpiecznyElement = tablica?[0];
```

# List

Najważniejsze właściwości (Stan listy)
 * .Count – Zwraca fizyczną liczbę elementów, które aktualnie są w liście.
 * .Capacity – Zwraca wielkość "wewnętrznego pudełka" w pamięci RAM. Lista w C# pod spodem używa zwykłej tablicy. Gdy wbijasz do niej elementy i brakuje miejsca, lista po cichu tworzy dwukrotnie większą tablicę, przepina dane i wyrzuca starą (np. pojemność rośnie z 4 na 8, potem 16, 32).
 * [indeks] – Tradycyjny dostęp do szufladki, np. lista[0].
Dodawanie elementów
 * .Add(element) – Dorzuca jedną rzecz na sam koniec listy.
 * .AddRange(kolekcja) – Dokleja na koniec całą inną listę lub tablicę naraz. Dużo szybsze niż robienie pętli z pojedynczym Add().
 * .Insert(indeks, element) – Wpycha element w konkretne miejsce (np. lista.Insert(0, x) wstawia na sam początek). To kosztowna operacja, bo komputer musi przesunąć całą resztę elementów w prawo.
Usuwanie elementów
 * .Remove(element) – Szuka pierwszej sztuki podanego elementu i wyrzuca go z listy. Zwraca true, jeśli się udało, i false, jeśli takiego elementu nie było.
 * .RemoveAt(indeks) – Usuwa to, co leży w konkretnej szufladce (np. lista.RemoveAt(0)). Elementy po prawej stronie automatycznie przesuwają się w lewo, żeby załatać dziurę.
 * .RemoveAll(warunek) – Usuwa masowo wszystko, co spełnia Twój przepis, np. lista.RemoveAll(x => x < 0) wyrzuci wszystkie liczby ujemne.
 * .Clear() – Wyzerowuje listę. .Count spada do 0, ale .Capacity (pudełko w pamięci) zostaje na przyszłość, żeby nie marnować czasu na ponowne budowanie mebla.
Wyszukiwanie i sprawdzanie
 * .Contains(element) – Zwraca true albo false. Odpowiada na pytanie: "Czy ten konkretny obiekt leży w środku?".
 * .IndexOf(element) – Przeszukuje listę od lewej i oddaje numer szufladki, w której leży element. Jeśli go nie znajdzie, zwraca -1.
 * .Find(warunek) – Oddaje pierwszy element, który pasuje do przepisu (np. lista.Find(x => x.Value > 100)).
 * .FindAll(warunek) – Oddaje nową, mniejszą listę, w której są tylko elementy spełniające warunek.
 * .Exists(warunek) – Zwraca true, jeśli jakikolwiek element w liście pasuje do warunku.
Sortowanie i transformacje
 * .Sort() – Układa elementy rosnąco. Jeśli obiekty są złożone (np. Twoje czujniki), musisz mu podać przepis, jak ma je porównywać: lista.Sort((a, b) => a.At.CompareTo(b.At)).
 * .Reverse() – Odwraca kolejność wszystkich karteczek o 180 stopni (pierwsza staje się ostatnią).
 * .ToArray() – Zamraża listę. Kopiuje jej obecną zawartość i wypluwa jako twardą, niemodyfikowalną w rozmiarze tablicę (np. int[]). Przydatne, gdy musisz przekazać dane do funkcji, która nie przyjmuje list.

Te znaczki to tzw. indeksy od końca (^) oraz wycinki (..). Zostały dodane w nowszym C#, żeby ułatwić wybieranie elementów z list i tablic bez pisania długich równań matematycznych.
Daszek (^) – Odliczanie od końca
Zamiast pisać długie matematyczne obliczenia, żeby dostać się do ostatnich elementów na liście, używasz daszka. Zastępuje on klasyczne lista.Count - x.
 * lista[^1] – oznacza "pierwszy element od końca" (czyli dosłownie ostatni). To jest w 100% to samo co stare lista[lista.Count - 1].
 * lista[^2] – oznacza "drugi od końca" (przedostatni).
Ważna pułapka: Daszek nie działa jak zwykłe indeksy liczone od zera. lista[^0] wywali błąd programu! Dlaczego? Bo ^0 oznacza dosłownie "długość całej tablicy", a element o takim indeksie fizycznie nie istnieje (spadłabyś z krawędzi tablicy). Liczenie od końca zawsze zaczynamy od ^1.
Dwie kropki (..) – Wycinki (Ranges)
Służą do wycinania "plastra" z większej listy lub tablicy. Zapisujesz to jako [poczatek .. koniec].
Złota zasada: Element początkowy jest brany do wycinka, ale element końcowy już NIE (jest to tzw. granica wykluczająca).
 * lista[1..4] – wyciąga elementy z szufladek nr 1, 2 i 3. Zatrzymuje się przed 4.
 * lista[2..] – jeśli nie podasz prawej strony, komputer weźmie wszystko od indeksu nr 2 aż do samego końca.
 * lista[..3] – jeśli nie podasz lewej strony, komputer zacznie od samego początku i weźmie elementy 0, 1 i 2.
 * lista[^3..^1] – możesz to łączyć z daszkami! Weźmie "trzeci od końca" i "drugi od końca" (zatrzyma się przed "pierwszym od końca").
Jak to się ma do [.., { Value: < 0 }] z poprzedniego zadania?
W tamtym zadaniu użyliśmy dwóch kropek we wzorcach list wewnątrz instrukcji switch. Tam kropki pełnią funkcję "worka na resztę".
Kiedy program widzi filtr napisany jako [.., { Value: < 0 }], kropki na początku oznaczają: "Zignoruj dowolną liczbę elementów na starcie (może być ich 0, a może być 1000), po prostu przewiń to do przodu i skup się tylko na ostatnim elemencie w nawiasach".

# Dictionary

Słownik (Dictionary<TKey, TValue>) to odpowiednik biurowej szafki na dokumenty. Każda szufladka w tej szafce musi mieć przyklejoną absolutnie unikalną naklejkę (Klucz / Key), pod którą kryje się konkretna zawartość (Wartość / Value).
Najważniejsze właściwości (Stan słownika)
 * .Count – Zwraca liczbę zajętych szufladek w szafce.
 * .Keys – Oddaje worek, w którym są wyłącznie same naklejki ze wszystkich szufladek (np. lista wszystkich nazw czujników).
 * .Values – Oddaje worek z samą zawartością, całkowicie ignorując naklejki (np. same liczby odczytów).
Dodawanie i modyfikacja
 * slownik.Add(klucz, wartosc) – Wstawia nową szufladkę z naklejką i zawartością. Uwaga: Jeśli spróbujesz dodać naklejkę, która już fizycznie wisi na innej szufladce, program natychmiast wywali krytyczny błąd ( wyjątek ArgumentException).
 * slownik[klucz] = wartosc – Metoda inteligentna i najczęściej używana (indexer). Działa podwójnie: jeśli naklejki nie ma w szafce, to ją bezpiecznie zakłada. Jeśli naklejka już jest, to po prostu otwiera tę szufladkę i bezlitośnie nadpisuje (podmienia) jej starą zawartość na nową.
 * .TryAdd(klucz, wartosc) – Metoda ostrożna. Próbuje wstawić nową szufladkę. Jeśli naklejki nie było, wstawia ją i zwraca true. Jeśli naklejka już była, nic nie robi (nie psuje starych danych) i po prostu cicho zwraca false.
Wyciąganie danych
 * slownik[klucz] – Podajesz nazwę naklejki w nawiasach kwadratowych i dostajesz zawartość szufladki. Uwaga: Jeśli takiej naklejki nie ma w szafce, program wywali błąd (KeyNotFoundException).
 * .TryGetValue(klucz, out wartosc) – Metoda ratunkowa. Komputer mówi: "Sprawdzę, czy jest taka naklejka. Jeśli tak, to włożę wynik do Twojej zmiennej i zwrócę true. Jeśli nie, to zwrócę false i dam Ci spokój, bez wywalania programu".
   (Przykład: if (slownik.TryGetValue("Balkon", out double wynik)) { ... })
Wyszukiwanie i sprawdzanie
 * .ContainsKey(klucz) – Najważniejsza metoda przy słownikach. Odpowiada true / false na pytanie: "Czy mamy już taką naklejkę?". Słowniki są specjalnie zoptymalizowane tak, żeby ta metoda działała dosłownie w ułamek milisekundy, nawet jeśli szufladek jest milion (tzw. tablica z haszowaniem).
 * .ContainsValue(wartosc) – Szuka po zawartości (np. "Czy w jakiejkolwiek szufladce leży liczba 100?"). To działa wolno, bo komputer musi otworzyć każdą szufladkę po kolei i zajrzeć do środka.
Usuwanie elementów
 * .Remove(klucz) – Szuka szufladki po naklejce i wyrywa ją całą z szafki. Zwraca true (udało się wyrzucić) lub false (w ogóle nie było takiej naklejki, więc nic nie wyrzucono).
 * .Clear() – Wywala całą zawartość wszystkich szufladek naraz. Szafka zostaje pusta (.Count spada do 0).
Przeglądanie (Pętle)
 * foreach (var kvp in slownik) – Klasyczne wyciąganie po jednej szufladce naraz. Zmienna kvp (czyli Key-Value Pair) to ta wyciągnięta szufladka. Rozpakowujesz ją używając:
   * kvp.Key (czytasz naklejkę)
   * kvp.Value (czytasz zawartość)

Kiedy robisz foreach (var kvp in slownik), napotykasz dwie blokady:
 * Zmienna kvp jest tylko do odczytu: Nie możesz napisać kvp.Value = 10;. Dlaczego? Bo kvp to tylko kartka, na której komputer przepisał Ci to, co widział w szafce. Zmiana liczby na tej kartce nie zmieniłaby magicznie zawartości prawdziwej szuflady.
 * Zakaz przebudowy mebla: Jeśli w trakcie trwania pętli foreach spróbujesz dołożyć do szafki nową szufladkę (Add) albo jakąś wyrwać (Remove), C# natychmiast wywali krytyczny błąd (InvalidOperationException: Collection was modified). Zasada brzmi: Nie możesz przebudowywać szafki, podczas gdy pętla jest w trakcie jej inwentaryzacji.
Oto jak się to rozwiązuje w praktyce w zależności od tego, co dokładnie chcesz zrobić:
Sposób 1: Kiedy chcesz tylko podmienić wartość w ISTNIEJĄCEJ szufladce
Mimo że pętla trwa, C# pozwala Ci włożyć rękę do szafki i podmienić zawartość, o ile nie zmieniasz liczby szufladek. Robisz to po prostu odwołując się do słownika przez klucz:
foreach (var kvp in slownik) {
    // kvp.Value = 100; --> BŁĄD, bo to tylko odczyt z kartki
    
    // Ale to zadziała idealnie:
    slownik[kvp.Key] = 100; // Mówisz: "Szafko, do tej konkretnej szuflady włóż 100"
}

Sposób 2: Pancerna metoda na wszystko (Usuwanie, Dodawanie, Zmienianie)
Jeśli chcesz w trakcie pętli swobodnie usuwać stare czujniki, dodawać nowe albo robić totalną demolkę w słowniku, musisz użyć triku z klonowaniem listy naklejek. Służy do tego dopisek .ToList().

```csharp
// Pobieramy same naklejki i robimy ich ksero do osobnej listy (.ToList)
foreach (var klucz in slownik.Keys.ToList()) {
    
    // Teraz pętla chodzi po KSEROKOPII naklejek, a nie po samej szafce!
    // Możesz więc bezkarnie demolować prawdziwą szafkę:
    
    slownik[klucz] = 999; // Zmiana wartości
    slownik.Remove(klucz); // Usunięcie szufladki
    slownik.Add("Nowy Czujnik", 0); // Dodanie nowej szufladki
}
```

Dzięki .ToList() robisz sobie "zdjęcie" wszystkich naklejek przed rozpoczęciem pracy. Pętla czyta z tego ksera, więc główna szafka jest całkowicie odblokowana i możesz na niej wykonywać dowolne operacje.
