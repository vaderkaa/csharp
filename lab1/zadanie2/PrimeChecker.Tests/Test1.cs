using PrimeChecker.Lib;

namespace PrimeChecker.Tests;

[TestClass]
public sealed class NumberUtilsTests
{
    [TestMethod]
    public void IsPrime_IsMinusTen_ReturnsFalse()
    {
        int number = -10;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsPrime_IsZero_ReturnsFalse()
    {
        int number = 0;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsPrime_IsOne_ReturnsFalse()
    {
        int number = 1;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsPrime_IsTwo_ReturnsTrue()
    {
        int number = 2;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsPrime_IsNine_ReturnsFalse()
    {
        int number = 9;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsPrime_IsThirteen_ReturnsTrue()
    {
        int number = 13;

        bool result = NumberUtils.IsPrime(number);

        Assert.IsTrue(result);
    }
}
