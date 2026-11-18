using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Lab10EN;

public static class Benchmarking
{
    private const int Size = 1_000;

    public static void StringAdd()
    {
        string _ = String.Empty;
        for (int i = 0; i < Size; i++)
        {
            _ += 'a';
        }
    }

    public static void StringBuilder()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Size; i++)
        {
            sb.Append('a');
        }

        string _ = sb.ToString();
    }

    public static void StringJoin()
    {
        string _ = string.Join(string.Empty, Enumerable.Repeat('a', Size));
    }

    public static Action CreateBenchmark(Action action, string name, int times = 1000)
    // times - opcjonalny parametr
    {
        Action tool = () =>
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < times; i++)
                action();
            stopwatch.Stop();

            double averageTime = stopwatch.Elapsed.TotalMilliseconds / times;
            Console.WriteLine($"Function {name}: {averageTime} ms on average");
        };
        return tool;
    }

    public static IEnumerable<Action> CreateBenchmarks()
    {
        return new Action[]
        {
            CreateBenchmark(StringAdd, "StringAdd"),
            CreateBenchmark(StringBuilder, "StringBuilder"),
            CreateBenchmark(StringJoin, "StringJoin")
        };
        //return ImmutableList.Create(StringAdd, StringBuilder, StringJoin);
    }
}