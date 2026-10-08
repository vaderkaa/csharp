using PrimeChecker.Lib;

for(int i = -5; i < 15; i++)
{
    if (NumberUtils.IsPrime(i))
        Console.WriteLine($"{i}");
}
