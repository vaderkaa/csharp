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
    }
}