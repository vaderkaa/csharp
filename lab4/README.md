### Skąd rozwiązywane są ścieżki względne?

Katalog pliku wykonywalnego (`AppContext.BaseDirectory`) = miejsce na dysku, gdzie fizycznie siedzi plik `.exe`.

Bieżący katalog roboczy (`Directory.GetCurrentDirectory()`) = folder, w którym otworzyłeś konsolę/terminal w momencie kliknięcia `Enter`. To do niego doklejane są ścieżki względne.

### Delegaty i zdarzenia

```csharp
// =========================================================================
// ETAP 1: PRZYGOTOWANIE KOPERTY (Paczka z danymi, którą będziemy wysyłać)
// =========================================================================
// Tworzymy pudełko na informację "ile sekund zostało". 
// Musi dziedziczyć po klasie EventArgs, bo tego wymaga konwencja C#.
public class TickEventArgs(int remaining) : EventArgs
{
    public int Remaining { get; } = remaining; // Dane w środku koperty
}

// =========================================================================
// ETAP 2: BUDOWA NADAWCY (Klasa zegara, która wysyła sygnały)
// =========================================================================
public class Countdown(int start)
{
    // 2A. Tworzymy dwa "gniazdka" (zdarzenia), do których można się podpiąć:
    // Gniazdko Tick -> wymaga funkcji przyjmującej kopertę TickEventArgs (z liczbą)
    public event EventHandler<TickEventArgs>? Tick;
    
    // Gniazdko Finished -> wymaga funkcji przyjmującej pustą kopertę EventArgs (bez danych)
    public event EventHandler? Finished;

    // 2B. Logika zegara: kiedy i jak zegar wysyła sygnały
    public void Run()
    {
        for (int i = start; i > 0; i--)
        {
            // Pakujemy liczbę 'i' w kopertę TickEventArgs
            var koperta = new TickEventArgs(i);
            
            // Naciskamy spust (? oznacza: wyślij, tylko jeśli ktoś się podpiął)
            // 'this' to sam zegar (sender), 'koperta' to dane (e)
            Tick?.Invoke(this, koperta);
        }
        
        // Gdy pętla się skończy, wysyłamy sygnał końca z pustą kopertą
        Finished?.Invoke(this, EventArgs.Empty);
    }
}

// =========================================================================
// ETAP 3: PRZYGOTOWANIE ODBIORCY (Co ma się stać, gdy przyjdzie sygnał?)
// =========================================================================
// Piszesz zwykłą funkcję, która zareaguje na koniec odliczania.
// Jej parametry (object? sender, EventArgs e) muszą pasować do gniazdka Finished.
static void OnFinished(object? sender, EventArgs e) 
{
    Console.WriteLine("Liftoff!");
}

// =========================================================================
// ETAP 4: URUCHOMIENIE PROGRAMU (Łączymy klocki i odpalamy)
// =========================================================================

// 4A. Tworzymy obiekt zegara (na razie żadne gniazdka nie są podpięte)
var countdown = new Countdown(3);

// 4B. Podpinamy reakcje do gniazdek za pomocą operatora += (nic się jeszcze nie wykonuje!)
// Pod gniazdko Tick podpinamy lambdę (otwiera kopertę 'e' i wypisuje liczbę)
countdown.Tick += (sender, e) => Console.WriteLine($"{e.Remaining}...");

// Pod gniazdko Finished podpinamy naszą gotową funkcję z Etapu 3
countdown.Finished += OnFinished;

// 4C. Odpalamy zegar! Dopiero teraz leci pętla i wywołują się podpięte wyżej funkcje.
// Na ekranie pojawi się: 3... 2... 1... Liftoff!
countdown.Run();

// 4D. Sprzątanie / Test blokady:
// Wypinamy funkcję z gniazdka Finished (teraz gniazdko znowu jest puste)
countdown.Finished -= OnFinished; 

// Poniższa linijka da błąd kompilacji, bo jesteśmy na zewnątrz klasy Countdown
// i nie wolno nam ręcznie naciskać spustu zdarzenia:
// countdown.Finished(countdown, EventArgs.Empty);
```
