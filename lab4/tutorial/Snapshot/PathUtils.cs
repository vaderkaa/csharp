namespace Snapshot;

public static class PathUtils
{
    public static bool IsInside(string path, string directory)
    // path - repozytorium, do ktorego mam zapisywac kopie zapasowe
    // directory - katalog z plikami, ktore chce skopiowac
    // path musi lezec poza directory, bo zrobi sie nieskonczonosc
    {
        string fullPath = Path.GetFullPath(path);
        string fullDir = Path.GetFullPath(directory);

        string relative = Path.GetRelativePath(fullDir, fullPath); // sciezka wzgledna z katalogu do repozytorium
        // jesli zaczyna sie od .. to znaczy, ze zeby dojsc do repozytorium, trzeba WYJSC z katalogu (czyli jest bezpiecznie na zewnatrz)
        // to samo, jesli jest na innym dysku (IsPathRooted, np. D:\)

        if (relative == ".."
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}")
            || relative.StartsWith($"..{Path.AltDirectorySeparatorChar}")
            || Path.IsPathRooted(relative))
        {
            return false;
        }

        return true;
    }
}