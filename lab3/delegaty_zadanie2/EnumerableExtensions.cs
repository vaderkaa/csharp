using System.Text;

namespace Lab10EN;

public static class EnumerableExtensions
{
    public static TAccumulate Collect<TSource, TAccumulate>(this IEnumerable<TSource> source,
    TAccumulate collection, Action<TAccumulate, TSource> accumulator)
    {
        foreach(var item in source)
            accumulator(collection, item);
        // do wczesniej zaalokowanej kolekcji dodajemy w jakis sposob po kolei rzeczy
        return collection;
    }

    public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        foreach(var item in source)
        {
            if (predicate(item) == true)
                yield return item;
        }
    }

    public static string ConcatStrings<T>(this IEnumerable<T> source)
    {
        return source.Filter((text) => (text is string))
        .Collect(new StringBuilder(), (sb, item) => sb.Append(item)).ToString();
        // trzeba dac StringBuilder do ToString!
        //return string.Empty;
    }
}