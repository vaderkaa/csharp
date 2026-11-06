namespace Task3;

public class Program {
    public static void Main(string[] args) {
        Database database = Database.GetInstance();

        // 1. List people (in alphabetical order by surname, then first name) who do not have a loyalty card.
        // Hints: join...into clause, Count() method
        Console.WriteLine("****** Question 1 ******");
        Console.WriteLine("Provided:");

        var zadanie1 = from klient in database.Customers
                        join karta in database.LoyaltyCards
                        on klient.CustomerId equals karta.CustomerId into powiazane_karty
                        where powiazane_karty.Count() == 0
                        orderby klient.LastName, klient.FirstName
                        select new { Nazwisko = klient.LastName, Imie = klient.FirstName };

        foreach(var item in zadanie1)
        {
            Console.WriteLine($"{item.Nazwisko} {item.Imie}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Bąk Magdalena");
        Console.WriteLine("Bąk Marek");
        Console.WriteLine("Wiśniewski Artur");

        // 2. Based on the PESEL number, select people who are more than 29 years old, by birth year. Display these people from oldest to youngest.
        // Requirement: do not count the same person multiple times (you may use the let clause)
        // Hint: A PESEL number from 2000 onwards differs in that 20 is added to the month value
        Console.WriteLine();
        Console.WriteLine("****** Question 2 ******");
        Console.WriteLine("Provided:");

        var zadanie2 = from klient in database.Customers
                       let rok = int.Parse(klient.PESEL.Substring(0, 2))
                       let miesiac = int.Parse(klient.PESEL.Substring(2, 2))
                       let pelny_rok = (miesiac >= 21) ? 2000 + rok : 1900 + rok
                       where (2020 - pelny_rok) >= 29
                       orderby pelny_rok
                       select new
                       {
                           Imie = klient.FirstName,
                           Nazwisko = klient.LastName,
                           RokUrodzenia = pelny_rok,
                           Wiek = 2020 - pelny_rok,
                           Pesel = klient.PESEL
                       };

        foreach(var item in zadanie2)
        {
            Console.WriteLine($"{item.Nazwisko} {item.Imie} {item.Wiek}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Kowalski Jan 117");
        Console.WriteLine("Marciniak Jerzy 35");
        Console.WriteLine("Antoniewski Michał 32");

        // 3. Determine the 3 people who have been using the store's services the longest.
        Console.WriteLine();
        Console.WriteLine("****** Question 3 ******");
        Console.WriteLine("Provided:");

        var zadanie3 = (from klient in database.Customers
                        join zamowienie in database.Orders on klient.CustomerId equals zamowienie.CustomerId into zamowieniaKlienta
                        where zamowieniaKlienta.Any()
                        let pierwszaData = zamowieniaKlienta.Min(z => z.Date)
                        orderby pierwszaData ascending
                        select new
                        {
                            Imie = klient.FirstName,
                            Nazwisko = klient.LastName,
                            PierwszeZamowienie = pierwszaData
                        }).Take(3);

        foreach (var item in zadanie3)
        {
            // Używamy formatowania :yyyy-MM-dd, żeby odciąć godziny i minuty (które w bazie są domyślnie zerami)
            Console.WriteLine($"{item.Nazwisko} {item.Imie} {item.PierwszeZamowienie:dd.MM.yyyy}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Nowak Andrzej 02.01.2017");
        Console.WriteLine("Antoniewski Michał 22.03.2017");
        Console.WriteLine("Kowalska Aneta 12.03.2018");

        // 4. Determine the 5 items with the highest total number of ordered units.
        Console.WriteLine();
        Console.WriteLine("****** Question 4 ******");
        Console.WriteLine("Provided:");

        var zadanie4 = (from produkt in database.Products
                        join op in database.OrderProducts on produkt.ProductId equals op.ProductId into zamowieniaProduktu
                        let sumaSztuk = zamowieniaProduktu.Sum(z => z.Quantity)
                        orderby sumaSztuk descending
                        select new
                        {
                            Nazwa = produkt.Name,
                            SumaSztuk = sumaSztuk
                        }).Take(5);

        foreach (var item in zadanie4)
        {
            Console.WriteLine($"{item.Nazwa} {item.SumaSztuk}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Gaming chair 21");
        Console.WriteLine("Marker 11");
        Console.WriteLine("CD 10");
        Console.WriteLine("Pen 10");
        Console.WriteLine("Monitor 5");

        // 5. For each person, determine the total amount they spent in the store (taking into account the discount available to people with a loyalty card).
        // List the customers in alphabetical order by surname, then first name.
        Console.WriteLine();
        Console.WriteLine("****** Question 5 ******");
        Console.WriteLine("Provided:");

        // ZAPYTANIE 1: Obliczamy czystą sumę wydatków dla każdego klienta (bez zniżek)
        // Złączamy produkty z zamówieniami i od razu grupujemy po CustomerId
        var wydatkiBezZnizki = from op in database.OrderProducts
                               join p in database.Products on op.ProductId equals p.ProductId
                               join o in database.Orders on op.OrderId equals o.OrderId
                               group op.Quantity * p.Price by o.CustomerId into g
                               select new
                               {
                                   CustomerId = g.Key,
                                   Kwota = g.Sum()
                               };

        // ZAPYTANIE 2: Łączymy bazę klientów z przygotowanymi wydatkami i kartami lojalnościowymi
        var zadanie5 = from klient in database.Customers

                       // 1. Podpinamy wydatki z pierwszego zapytania używając 'into' (Left Join)
                       // żeby nie odrzucić klientów, którzy nigdy nic nie kupili.
                       join wydatek in wydatkiBezZnizki on klient.CustomerId equals wydatek.CustomerId into powiazaneWydatki
                       // .Sum() na pustej liście zwróci bezpieczne 0.0
                       let sumaWydatkow = powiazaneWydatki.Sum(w => w.Kwota)

                       // 2. Podpinamy tabelę zniżek
                       join karta in database.LoyaltyCards on klient.CustomerId equals karta.CustomerId into powiazaneKarty
                       // Jeśli klient nie ma karty, lista jest pusta, a .Sum() zwróci 0.0
                       let znizka = powiazaneKarty.Sum(k => k.Discount)

                       // 3. Aplikujemy zniżkę do sumy
                       let kwotaKoncowa = sumaWydatkow * (1.0 - znizka)

                       // 4. Sortujemy według wymagań: nazwisko, a następnie imię
                       orderby klient.LastName, klient.FirstName

                       select new
                       {
                           Imie = klient.FirstName,
                           Nazwisko = klient.LastName,
                           WydanaKwota = kwotaKoncowa
                       };

        foreach (var item in zadanie5)
        {
            // Używamy formatowania :F2, żeby wyświetlić równo dwa miejsca po przecinku (np. 150.00 zamiast 150)
            Console.WriteLine($"{item.Nazwisko} {item.Imie}: {item.WydanaKwota:F2}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Antoniewski Michał 1053");
        Console.WriteLine("Bąk Magdalena 2912");
        Console.WriteLine("Bąk Marek 3000");
        Console.WriteLine("Czerwińska Justyna 2593,5");
        Console.WriteLine("Kowalska Aneta 212,5");
        Console.WriteLine("Kowalski Jan 3654");
        Console.WriteLine("Marciniak Jerzy 13600");
        Console.WriteLine("Nowak Alina 1400");
        Console.WriteLine("Nowak Andrzej 2550");
        Console.WriteLine("Wiśniewski Artur 2025");

        Console.WriteLine();
    }
}
