using TemperatureConverter.Lib;

double[] temperatures = { -20, 0, 20, 100 };

for(int i = 0; i < temperatures.Length; i++)
{
    double fahrenheit = TemperatureUtils.CelsiusToFahrenheit(temperatures[i]);
    Console.WriteLine($"{temperatures[i]}°C = {fahrenheit}°F");
}