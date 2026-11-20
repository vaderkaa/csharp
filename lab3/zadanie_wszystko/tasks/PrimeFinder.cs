namespace tasks;

/// <summary>
/// Here you should implement the Sieve of Eratosthenes algorithm to find all prime numbers up to a given upper bound.
/// </summary>
public static class PrimeFinder
{
    public static IEnumerable<int> SieveOfEratosthenes(int upperBound)
    {
        if (upperBound < 2)
            yield break;

        bool[] tab = new bool[upperBound + 1];
        tab[0] = true;
        tab[1] = true;

        for(int i = 2; i <= upperBound; i++) // normalnie do pierwiastka, ale tutaj od razu zwracamy
        {
            if (tab[i] == false)
            {
                yield return i;

                for (int j = i * i; j <= upperBound; j += i)
                    tab[j] = true;
            }
        }

        //throw new NotImplementedException();
    }
}