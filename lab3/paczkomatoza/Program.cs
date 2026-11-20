using paczkomatoza;

class Program
{
    static void Main(string[] args)
    {
        DistributionCenter dc = DistributionCenter.GetDefaultDistributionCenter();

        // 1. Dla każdego nadawcy wypisz liczbę nadanych paczek oraz średnią wagę
        

        Console.WriteLine("=== ZADANIE 1: Nadawcy ===");
        //foreach (var r in result1)
        //{
        //    Console.WriteLine($"{r.Nadawca.Name} | paczek: {r.Ilosc} | średnia waga: {r.Srednia:F2} kg");
        //}

        // 2. Dla każdego statusu paczki znajduje kuriera, który ma najwięcej paczek z takim statusem
        // var result2 = 

        Console.WriteLine("\n=== ZADANIE 2: Najlepszy kurier dla każdego statusu ===");
        //foreach (var r in result2)
        //{
        //    Console.WriteLine($"Status: {r.Status} -> {r.Kurier.Name} ({r.LiczbaPaczek} szt.)");
        //}

        // 3. Zwraca paczkomat, w którym obecnie czeka najwięcej paczek gotowych do odbioru
        // var result3 = ...

        Console.WriteLine("\n=== ZADANIE 3: Paczkomat z największą liczbą paczek do odbioru ===");
        //if (result3 != null)
        //{
        //    Console.WriteLine($"{result3.Paczkomat.Name} ({result3.Paczkomat.City}) -> czeka paczek: {result3.LiczbaOczekujacych}");
        //}
        //else
        //{
        //    Console.WriteLine("Brak paczek oczekujących w paczkomatach.");
        //}
    }
}