using paczkomatoza;

class Program
{
    static void Main(string[] args)
    {
        DistributionCenter dc = DistributionCenter.GetDefaultDistributionCenter();

        // 1. Dla każdego nadawcy wypisz liczbę nadanych paczek oraz średnią wagę
        var result1 = from sender in dc.Senders.Values
                      join shipment in dc.Shipments.Values
                      on sender.Id equals shipment.SenderId into powiazanePaczki
                      select new
                      {
                          Nadawca = sender,
                          Ilosc = powiazanePaczki.Count(),
                          Srednia = powiazanePaczki.Any() ?
                          powiazanePaczki.Average(x => x.Parcel.WeightKg) : 0m
                          // m, bo Average zwraca decimal
                      };

        Console.WriteLine("=== ZADANIE 1: Nadawcy ===");
        foreach (var r in result1)
        {
            Console.WriteLine($"{r.Nadawca.Name} | paczek: {r.Ilosc} | średnia waga: {r.Srednia:F2} kg");
        }

        // 2. Dla każdego statusu paczki znajduje kuriera,
        // który ma najwięcej paczek z takim statusem
        var result2 = from shipment in dc.Shipments.Values
                      where shipment.DeliveryCourierId != null
                      join courier in dc.Couriers.Values
                      on shipment.DeliveryCourierId equals courier.Id
                      group courier by shipment.Status into statusGroup
                      let bestCourier = (from c in statusGroup
                                         group c by c into courierGroup
                                         let howMany = courierGroup.Count()
                                         orderby howMany descending
                                         select new
                                         {
                                             HowMany = howMany,
                                             Courier = courierGroup.Key
                                         }).First()
                      select new
                      {
                          Status = statusGroup.Key,
                          Kurier = bestCourier.Courier,
                          LiczbaPaczek = bestCourier.HowMany
                      };

        Console.WriteLine("\n=== ZADANIE 2: Najlepszy kurier dla każdego statusu ===");
        foreach (var r in result2)
        {
            Console.WriteLine($"Status: {r.Status} -> {r.Kurier.Name} ({r.LiczbaPaczek} szt.)");
        }

        // 3. Zwraca paczkomat, w którym obecnie czeka najwięcej paczek gotowych do odbioru
        var result3 = (from shipment in dc.Shipments.Values
                       where shipment.Status == ShipmentStatus.WaitingInLockerToBeReceived
                       && shipment.DestinationLockerId != null
                       join locker in dc.Lockers.Values
                       on shipment.DestinationLockerId equals locker.Id
                       group shipment by locker into lockerGroup
                       let lockerCount = lockerGroup.Count()
                       orderby lockerCount descending
                       select new
                       {
                           Paczkomat = lockerGroup.Key,
                           LiczbaOczekujacych = lockerCount
                       }).FirstOrDefault();
        // program sprawdza czy result3 != null
        // gdyby nie bylo zadnej paczki do odebrania, to First() rzuciloby wyjatek

        Console.WriteLine("\n=== ZADANIE 3: Paczkomat z największą liczbą paczek do odbioru ===");
        if (result3 != null)
        {
            Console.WriteLine($"{result3.Paczkomat.Name} ({result3.Paczkomat.City}) -> czeka paczek: {result3.LiczbaOczekujacych}");
        }
        else
        {
            Console.WriteLine("Brak paczek oczekujących w paczkomatach.");
        }

        // A. Pojedyncza przesyłka, której paczka ma największa objętość
        // W przypadku remisu wybierz przesyłkę utworzoną najwcześniej
        var resultA = (from shipment in dc.Shipments.Values
                      let v = shipment.Parcel.LengthCm * shipment.Parcel.WidthCm 
                      * shipment.Parcel.HeightCm
                      join sender in dc.Senders.Values
                      on shipment.SenderId equals sender.Id
                      join recipient in dc.Recipients.Values
                      on shipment.RecipientId equals recipient.Id
                      orderby v descending, shipment.CreatedAt ascending
                      select new
                      {
                          TrackingCode = shipment.TrackingCode,
                          SenderName = sender.Name,
                          RecipientName = recipient.Name,
                          VolumeCm3 = v ?? 0m
                      }).FirstOrDefault();

        Console.WriteLine("=== ZADANIE A ===");
        if (resultA != null)
        {
            Console.WriteLine($"Kod: {resultA.TrackingCode} | Nadawca: {resultA.SenderName} | Odbiorca: {resultA.RecipientName} | Objętość: {resultA.VolumeCm3} cm3");
        }

        // B. Dla każdego odbiorcy z bazy oblicz, ile średnio zdarzeń
        // w historii śledzenia przypada na jedną jego przesyłkę
        var resultB = from recipient in dc.Recipients.Values
                      join shipment in dc.Shipments.Values
                      on recipient.Id equals shipment.RecipientId
                      into recipientShipments
                      let srednia = recipientShipments.Any()
                      ? recipientShipments.Average
                      (x => dc.TrackingHistory[x.TrackingCode].Count)
                      : 0.0
                      select new
                      {
                          RecipientName = recipient.Name,
                          ShipmentsCount = recipientShipments.Count(),
                          AvgTrackingEvents = srednia
                      };
        // bierze shipment, jego tracking code, zlicza ile zdarzen odnotowano
        // sumuje i dzieli przez liczbe paczek

        Console.WriteLine("\n=== ZADANIE B ===");
        foreach (var r in resultB)
        {
            Console.WriteLine($"{r.RecipientName} | przesyłek: {r.ShipmentsCount} | śr. zdarzeń: {r.AvgTrackingEvents:F2}");
        }

        // C. Znajdź miasto docelowe, do którego wysłano najwięcej przesyłek
        // W przypadku remisu rozstrzygnij po większej łącznej wadze
        // przesyłek wysłanych do tego miasta

        var resultC = (from shipment in dc.Shipments.Values
                       group shipment by shipment.DestinationCity into cityGroup
                       let count = cityGroup.Count()
                       let weight = cityGroup.Sum(x => x.Parcel.WeightKg)
                       orderby count descending, weight descending
                       select new
                       {
                           City = cityGroup.Key,
                           ShipmentsCount = count,
                           TotalWeightKg = weight
                       }).FirstOrDefault();

        Console.WriteLine("\n=== ZADANIE C ===");
        if (resultC != null)
        {
            Console.WriteLine($"Miasto: {resultC.City} | przesyłek: {resultC.ShipmentsCount} | łączna waga: {resultC.TotalWeightKg:F2} kg");
        }

        // D. Dla każdego paczkomatu, który kiedykolwiek brał udział
        // w wysyłce lub odbiorze paczki, znajdź najwcześniej utworzoną przesyłkę,
        // która z niego korzystała
        // Posortuj wyniki po nazwie paczkomatu
        var resultD = from locker in dc.Lockers.Values
                      let matchingShipments = (from s in dc.Shipments.Values
                                               where locker.Id == s.DestinationLockerId
                                               || locker.Id == s.SourceLockerId
                                               orderby s.CreatedAt
                                               select s)
                      where matchingShipments.Any()
                      let firstShipment = matchingShipments.First()
                      orderby locker.Name
                      select new
                      {
                          LockerName = locker.Name,
                          LockerCity = locker.City,
                          FirstShipmentTrackingCode = firstShipment.TrackingCode,
                          CreatedAt = firstShipment.CreatedAt
                      };

        Console.WriteLine("\n=== ZADANIE D ===");
        foreach (var r in resultD)
        {
            Console.WriteLine($"{r.LockerName} ({r.LockerCity}) -> pierwsza: {r.FirstShipmentTrackingCode} ({r.CreatedAt:yyyy-MM-dd HH:mm})");
        }

        // E. Znajdź wszystkie paczkomaty (Locker), z których korzystało najwięcej unikalnych nadawców (SenderId)
        // (licząc zarówno nadania z paczkomatu SourceLockerId, jak i dostawy do paczkomatu DestinationLockerId)
        // Wypisz wszystkie paczkomaty osiągające ten maksymalny wynik (obsługa remisu na 1. miejscu)
        var lockerSendersStats = (from locker in dc.Lockers.Values
                                  let uniqueSenderNames = (from s in dc.Shipments.Values
                                                           where s.SourceLockerId == locker.Id || s.DestinationLockerId == locker.Id
                                                           join sender in dc.Senders.Values on s.SenderId equals sender.Id
                                                           orderby sender.Name ascending
                                                           select sender.Name).Distinct()
                                  select new
                                  {
                                      LockerName = locker.Name,
                                      UniqueSendersCount = uniqueSenderNames.Count(),
                                      SenderNames = uniqueSenderNames
                                  }).ToList();

        var maxSendersCount = lockerSendersStats.Any() ? lockerSendersStats.Max(x => x.UniqueSendersCount) : 0;

        var resultE = from stat in lockerSendersStats
                      where stat.UniqueSendersCount == maxSendersCount
                      orderby stat.LockerName ascending
                      select stat;

        Console.WriteLine("\n=== ZADANIE E ===");
        foreach (var r in resultE)
        {
            Console.WriteLine($"{r.LockerName} -> unikalnych nadawców: {r.UniqueSendersCount} ({string.Join(", ", r.SenderNames)})");
        }

        // F. Przeszukaj całą historię zdarzeń (dc.TrackingHistory) i znajdź lokalizację (TrackingEvent.Location),
        // w której odnotowano najwięcej zdarzeń o statusie innym niż ShipmentStatus.Prepared
        var resultF = (from eventList in dc.TrackingHistory.Values
                       from ev in eventList
                       where ev.Status != ShipmentStatus.Prepared
                       group ev by ev.Location into locationGroup
                       let count = locationGroup.Count()
                       orderby count descending
                       select new
                       {
                           Location = locationGroup.Key,
                           EventsCount = count
                       }).FirstOrDefault();

        Console.WriteLine("\n=== ZADANIE F ===");
        if (resultF != null)
        {
            Console.WriteLine($"Lokalizacja: {resultF.Location} | zdarzeń: {resultF.EventsCount}");
        }
    }

}