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
