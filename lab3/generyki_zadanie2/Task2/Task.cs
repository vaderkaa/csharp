using System.Globalization;

namespace Task2;
public readonly record struct Reading(string Sensor, double Value, DateTime At);

public record class Data(
    double HighestValue,                        // highest recorded value across all sensors
    double LowestTemp,                          // lowest recorded temperature
    string MostReadings,                        // name of the sensor with most readings
    TimeSpan TotalDuration,                     // difference between first and last recorded reading
    IList<string> UniqueSensors,                // list of all sensor names without duplicates
    IList<(string, double)> AverageReadings,    // average reading per sensor
    IList<(string, TimeSpan)> LargestGaps,      // a largest timespan between two consecutive readings (time order) per sensor
    IList<(Reading, Reading)> SignChanges       // two consecutive readings (time order) having two differnet signs (first < second) 
);

public class SensorLog {
    private readonly List<Reading> _readings = new();
    public void Add(Reading r) { _readings.Add(r); }
    public IReadOnlyList<Reading> All => _readings;

    public SensorLog LoadFromCsv(string path) {
        bool first = true;
        foreach (var line in File.ReadLines(path)) {
            if (first) { first = false; continue; } // assuming header            
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }
            var parts = line.Split(',');
            Add(new Reading(parts[0].Trim(), double.Parse(parts[1], CultureInfo.InvariantCulture), DateTime.Parse(parts[2], CultureInfo.InvariantCulture)));
        }
        return this;
    }
}

// =============================================================
// TODO 1
// =============================================================

public static class T1
{
    public static IEnumerable<Reading> InRange(IEnumerable<Reading> source, double min, double max)
    {
        foreach (var reading in source)
        {
            if (reading.Value >= min && reading.Value <= max)
            {
                yield return reading; // Leniwe zwracanie elementu
            }
        }
    }
}

// =============================================================
// TODO 2 
// =============================================================

public static class T2
{
    public static IEnumerable<Reading> Repeat(string sensor, double value, int count, DateTime start)
    {
        double currentValue = value;
        DateTime currentTime = start;

        for (int i = 0; i < count; i++)
        {
            yield return new Reading(sensor, currentValue, currentTime);

            currentTime = currentTime.AddMinutes(2);
            currentValue += 0.2;
        }
    }
}

// =============================================================
// TODO 3
// =============================================================

public class ReadingBySensorThenValue : Comparer<Reading>
{
    public override int Compare(Reading x, Reading y)
    {
        // 1. Nazwa czujnika (rosnąco)
        int sensorCmp = string.Compare(x.Sensor, y.Sensor, StringComparison.InvariantCultureIgnoreCase);
        if (sensorCmp != 0)
        {
            return sensorCmp;
        }

        // 2. Następnie według zarejestrowanej wartości (malejąco)
        int valueCmp = y.Value.CompareTo(x.Value);
        if (valueCmp != 0)
        {
            return valueCmp;
        }

        // 3. Następnie według czasu rejestracji (rosnąco)
        return x.At.CompareTo(y.At);
    }
}

// =============================================================
// TODO 4 
// =============================================================

public static class T4
{
    public static Data Summarize(IEnumerable<Reading> source)
    {
        // 1. Zgrywamy wszystko do fizycznej listy i sortujemy chronologicznie
        List<Reading> list = new List<Reading>();
        foreach (var r in source)
        {
            list.Add(r);
        }

        if (list.Count == 0)
        {
            throw new ArgumentException("Brak danych z czujników!");
        }

        // Sortujemy całą listę chronologicznie (od najstarszego do najnowszego)
        list.Sort((x, y) => x.At.CompareTo(y.At)); // Wyrazenie lambda (tego nie bylo)

        // 2. Przygotowanie zmiennych i słowników (segregatorów)
        double highestValue = double.MinValue;
        double lowestTemp = double.MaxValue;

        Dictionary<string, int> counts = new Dictionary<string, int>(); // Nazwa czujnika i ile razy wystapil do MostReadings
        Dictionary<string, double> sums = new Dictionary<string, double>(); // Do AverageReadings
        Dictionary<string, List<Reading>> sensorChronological = new Dictionary<string, List<Reading>>();
        // Nazwa czujnika i lista jego odczytow
        // LargestGaps - największy odstęp czasowy między dwoma kolejnymi odczytami (w porządku chronologicznym) dla każdego czujnika

        // 3. GŁÓWNA PĘTLA - jeden obieg do zebrania wszystkich statystyk
        foreach (var r in list)
        {
            // Szukanie maksimów i minimów globalnych
            if (r.Value > highestValue) highestValue = r.Value;
            if (r.Value < lowestTemp) lowestTemp = r.Value;

            // Jeśli widzimy ten czujnik pierwszy raz, zakładamy mu puste teczki
            if (!counts.ContainsKey(r.Sensor))
            {
                counts[r.Sensor] = 0;
                sums[r.Sensor] = 0;
                sensorChronological[r.Sensor] = new List<Reading>();
            }

            // Dorzucamy dane do odpowiednich teczek czujnika
            counts[r.Sensor]++; // Dodajemy odczyt
            sums[r.Sensor] += r.Value; // Dodajemy wartosc odczytu
            sensorChronological[r.Sensor].Add(r);
        }

        // 4. WYLICZANIE BRAKUJĄCYCH STATYSTYK Z SEGREGATORÓW

        // TotalDuration (ostatni czas minus pierwszy czas z posortowanej listy)
        TimeSpan totalDuration = list[list.Count - 1].At - list[0].At;

        // MostReadings (kto ma najwięcej wpisów w słowniku counts?)
        string mostReadings = "";
        int maxCount = -1;
        foreach (var kvp in counts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostReadings = kvp.Key;
            }
        }

        // UniqueSensors (wszystkie klucze z naszego słownika to gotowa lista nazw)
        List<string> uniqueSensors = new List<string>();
        foreach (var key in counts.Keys)
        {
            uniqueSensors.Add(key);
        }

        // AverageReadings (suma dzielona przez ilość dla każdego czujnika)
        List<(string, double)> averageReadings = new List<(string, double)>();
        foreach (var kvp in counts)
        {
            double avg = sums[kvp.Key] / kvp.Value;
            averageReadings.Add((kvp.Key, avg)); // Skleja string i srednia w calosc
        }

        // LargestGaps (największa luka w czasie dla poszczególnych czujników)
        List<(string, TimeSpan)> largestGaps = new List<(string, TimeSpan)>();
        foreach (var kvp in sensorChronological) // Kazdy czujnik ma ze soba liste chronoloicznie ulozonych odczytow
        {
            string sensorName = kvp.Key;
            List<Reading> readings = kvp.Value;

            TimeSpan maxGap = TimeSpan.Zero;
            for (int i = 0; i < readings.Count - 1; i++) // Bo patrzymy na element [i + 1]
            {
                TimeSpan gap = readings[i + 1].At - readings[i].At;
                if (gap > maxGap)
                {
                    maxGap = gap;
                }
            }
            largestGaps.Add((sensorName, maxGap));
        }

        // SignChanges (zmiany znaku w ułożeniu chronologicznym)
        List<(Reading, Reading)> signChanges = new List<(Reading, Reading)>();
        for (int i = 0; i < list.Count - 1; i++)
        {
            var first = list[i];
            var second = list[i + 1];

            // first < second (czyli z ujemnej na dodatnią) i różne znaki
            if (Math.Sign(first.Value) != Math.Sign(second.Value) && first.Value < second.Value)
            {
                signChanges.Add((first, second));
            }
        }

        // 5. Zwracamy wszystko zapakowane w obiekt Data
        return new Data(
            highestValue,
            lowestTemp,
            mostReadings,
            totalDuration,
            uniqueSensors,
            averageReadings,
            largestGaps,
            signChanges
        );
    }
}

// =============================================================
// TODO 5 
// =============================================================

public static class T5
{
    public static string Classify(IReadOnlyList<Reading> readings)
    {
        return readings switch
        {
            [] => "empty",
            [_] => "single",
            [{ Value: var v1 }, { Value: var v2 }] when v1 < v2 => "pair-asc",
            [{ Value: var v1 }, { Value: var v2 }] when v1 > v2 => "pair-desc",
            [_, _] => "pair-eq",
            [{ Value: > 100 }, ..] => "starts-high",
            [.., { Value: < 0 }] => "ends-low",
            _ => "other"
        };
    }
}