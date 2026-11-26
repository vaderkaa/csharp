using System.Globalization;

namespace Snapshot;

public static class SizeFormatting
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public static string ToHumanSize(this long bytes)
    {
        double size = bytes;
        int unit = 0;

        // dzielimy przez 1024, dopoki rozmiar wynosi co najmniej 1024 i nie doszlismy do ostatniej jednostki (TB)
        while (size >= 1024 && unit < Units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        // bajty (unit == 0) wypisujemy jako liczbe całkowita, bez ulamka
        if (unit == 0)
            return $"{bytes} {Units[unit]}";

        return string.Create(CultureInfo.InvariantCulture, $"{size:0.0} {Units[unit]}");
    }
}
