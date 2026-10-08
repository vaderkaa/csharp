using TemperatureConverter.Lib;

namespace TemperatureConverter.Tests;

[TestClass]
public sealed class TemperatureUtilsTests
{
    [TestMethod]
    public void CelsiusToFahrenheit_IsZero_ReturnsThirtyTwo()
    {
        double temperature = 0;

        double result = TemperatureUtils.CelsiusToFahrenheit(temperature);

        Assert.AreEqual(32, result);
    }

    [TestMethod]
    public void CelsiusToFahrenheit_IsOneHundred_ReturnsTwoHundredTwelve ()
    {
        double temperature = 100;

        double result = TemperatureUtils.CelsiusToFahrenheit(temperature);

        Assert.AreEqual(212, result);
    }

    [TestMethod]
    public void CelsiusToFahrenheit_IsMinusFourty_ReturnsMinusForty()
    {
        double temperature = -40;

        double result = TemperatureUtils.CelsiusToFahrenheit(temperature);

        Assert.AreEqual(-40, result);
    }
}
