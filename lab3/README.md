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

* **`record`** (domyślnie `record class`)
* Gdzie żyje: Na stercie (obciąża Garbage Collectora).
* Mutowalność: **Niemutowalny** (dane tylko do odczytu, modyfikator `init`).
* Zastosowanie: Standardowe przesyłanie paczek danych (np. profil użytkownika).

`init` (od initialize, czyli inicjuj) to rygorystyczna alternatywa dla set. Działa jak szybkoschnący beton:   
Pozwala ustawić wartość tylko w momencie powoływania obiektu do życia (gdy używasz słówka `new`).   
Ułamek sekundy później, gdy obiekt już powstanie, init zatrzaskuje kłódkę na zawsze. Wartość staje się zablokowana i można ją tylko czytać.


* **`record struct`**
* Gdzie żyje: Na stosie (superszybki, znika od razu, nie męczy GC).
* Mutowalność: **Mutowalny!** (można mu podmieniać dane w locie przez `set`).
* Zastosowanie: Matematyka, gry, generowanie milionów punktów na sekundę, gdy chcesz je w locie modyfikować.


* **`readonly record struct`**
* Gdzie żyje: Na stosie (superszybki, zero GC).
* Mutowalność: **Niemutowalny** (całkowicie zablokowany, modyfikator `init`).
* Zastosowanie: Złoty środek dla silników gier i matematyki (np. wektory w Raytracerze). Masz maksymalną wydajność z pamięci podręcznej i 100% pewności, że żadna inna funkcja nie zmieni Ci przypadkiem koordynatów wektora w trakcie obliczeń.
