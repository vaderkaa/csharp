# Skąd rozwiązywane są ścieżki względne?

Katalog pliku wykonywalnego (`AppContext.BaseDirectory`) = miejsce na dysku, gdzie fizycznie siedzi plik `.exe`.

Bieżący katalog roboczy (`Directory.GetCurrentDirectory()`) = folder, w którym otworzyłeś konsolę/terminal w momencie kliknięcia `Enter`. To do niego doklejane są ścieżki względne.

# Delegaty i zdarzenia

### Wytłumaczenie przykładu z tutoriala

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

// Poniższa linijka da błąd kompilacji, bo jesteśmy na zewnątrz klasy Countdown i nie wolno nam ręcznie naciskać spustu zdarzenia:
// countdown.Finished(countdown, EventArgs.Empty);
```

`required` – przy tworzeniu obiektu (`new Product { ... }`) kompilator zmusi Cię do podania nazwy (inaczej będzie błąd kompilacji).

`init` – nazwę można ustawić tylko w momencie tworzenia obiektu. Później nie da się jej już zmienić (jest tylko do odczytu).

### Schemat

```csharp
// 0. DANE ZDARZENIA (tworzysz tylko dla zdarzenia, które przesyła dane)
public class StatusChangedEventArgs : EventArgs
{
    public string NewStatus { get; }
    public StatusChangedEventArgs(string newStatus) => NewStatus = newStatus;
}

// 1. WYDAWCA (schemat łączący zdarzenie z danymi i bez danych + własne add/remove)
public class Worker
{
    // === WARIANT A: ZDARZENIE Z DANYMI (EventHandler<T>) ===
    private EventHandler<StatusChangedEventArgs>? _statusChanged; // Prywatne pole delegatu

    public event EventHandler<StatusChangedEventArgs>? StatusChanged
    {
        add
        {
            Console.WriteLine("Ktoś zasubskrybował StatusChanged");
            _statusChanged += value;
        }
        remove
        {
            Console.WriteLine("Ktoś odsubskrybował StatusChanged");
            _statusChanged -= value;
        }
    }

    // === WARIANT B: ZDARZENIE BEZ DANYCH (zwykły EventHandler) ===
    private EventHandler? _workFinished; // Prywatne pole delegatu

    public event EventHandler? WorkFinished
    {
        add
        {
            Console.WriteLine("Ktoś zasubskrybował WorkFinished");
            _workFinished += value;
        }
        remove
        {
            Console.WriteLine("Ktoś odsubskrybował WorkFinished");
            _workFinished -= value;
        }
    }

    // === LOGIKA KLASY ===
    public void DoWork()
    {
        OnStatusChanged(new StatusChangedEventArgs("W trakcie"));
        // ... właściwa praca ...
        OnWorkFinished(EventArgs.Empty); // Brak danych -> zawsze EventArgs.Empty
    }

    // === METODY WYWOŁUJĄCE (Invoke wołamy na PRYWATNYCH POLACH, nie na nazwach eventów!) ===
    protected virtual void OnStatusChanged(StatusChangedEventArgs e)
    {
        _statusChanged?.Invoke(this, e);
    }

    protected virtual void OnWorkFinished(EventArgs e)
    {
        _workFinished?.Invoke(this, e);
    }
}

// 2. SPIĘCIE W PROGRAM.CS
var worker = new Worker();

void OnStatus(object? sender, StatusChangedEventArgs e) => Console.WriteLine($"Status: {e.NewStatus}");
void OnFinished(object? sender, EventArgs e) => Console.WriteLine("Koniec pracy!");

worker.StatusChanged += OnStatus;   // Wykona blok 'add'
worker.WorkFinished += OnFinished;  // Wykona blok 'add'

worker.DoWork();                    // Wyzwoli oba zdarzenia po kolei

worker.StatusChanged -= OnStatus;   // Wykona blok 'remove'
worker.WorkFinished -= OnFinished;  // Wykona blok 'remove'
```
