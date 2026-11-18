namespace Lab10EN;

public static class DictionaryExtensions
{
    public static TValue? Merge<TKey, TValue>
    (this IDictionary<TKey, TValue> source,
    TKey key, TValue value, Func<TValue, TValue, TValue?> remappingFunction)
    {
        ArgumentNullException.ThrowIfNull(value); // jesli value jest null

        TValue? newValue = source.TryGetValue(key, out TValue? oldValue)
        && oldValue is not null ? remappingFunction(oldValue, value) : value;
        // jesli klucz istnieje i wartosc nie jest null, to obliczamy ja na nowo
        // jesli klucz nie istnieje lub jest przypisany do null to nowa wartosc

        if (newValue is null) // jesli funkcja zwrocila null, to usuwamy
            source.Remove(key);
        else
            source[key] = newValue;

        return newValue; // zwracamy nowa wartosc, podana wartosc lub null
    }

    public static IDictionary<DateTime, string> MergeLogs(IDictionary<DateTime, string> source, IDictionary<DateTime, string> appendix)
    {
        foreach(var kvp in appendix)
        {
            source.Merge(kvp.Key, kvp.Value, (oldVal, newVal)
            => string.Concat(oldVal, newVal));
        }
        return source;
    }
}