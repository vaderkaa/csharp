using System.Security.Cryptography;

namespace Snapshot;

public static class FileHasher
{
    public static string ComputeSha256(string path)
    {
        // using automatycznie zamyka, otwieramy tylko do odczytu
        using FileStream stream = File.OpenRead(path);
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // tu tez using!

        byte[] buffer = new byte[64 * 1024];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            hasher.AppendData(buffer, 0, read); // z czego czyta, jaki offset i ile bajtow

        byte[] hashBytes = hasher.GetHashAndReset();
        // zmiana kazdego z 32 bajtow na 3 cyfry szesnastkowe, powstaje ciag 64 snakow i zamiana duzych liter na male
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
        // return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        // tak naprawde to mozna to zrobic w tej jednej linijce
    }
}
