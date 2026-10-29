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

// 1. Interfejs z 'in' - gwarantuje, że obiekty do myjni tylko WCHODZĄ
interface IMyjnia<in T> 
{ 
    void Umyj(T obiekt); 
}

// 2. Kompletny kod uniwersalnej myjni (potrafi umyć absolutnie KAŻDY Owoc)
class MyjniaOwocow : IMyjnia<Owoc> 
{ 
    public void Umyj(Owoc o) { Console.WriteLine("Myję owoc!"); } 
}

// Metoda twardo żąda narzędzia, które umie myć Jabłka
void WypucujMojeJablko(IMyjnia<Jablko> myjniaTylkoDlaJablek) 
{ 
    myjniaTylkoDlaJablek.Umyj(new Jablko());
}

// MAIN:
// Tworzysz uniwersalną myjnię do wszystkich owoców
IMyjnia<Owoc> uniwersalnaMyjnia = new MyjniaOwocow();

// ZADZIAŁA: Skoro uniwersalna myjnia umyje dowolny owoc, poradzi sobie z Jabłkiem
WypucujMojeJablko(uniwersalnaMyjnia);
```
