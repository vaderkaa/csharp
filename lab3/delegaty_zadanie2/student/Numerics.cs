namespace Lab10EN;

public static class Numerics
{
    // deklaracja delegata
    public delegate double Function(double x);

    public static double HalleyRootFinding(Function f, Function? df = null,
    Function? ddf = null, double x0 = 0, double eps = 1e-6)
    {
        if (df is null)
            df = (x) => (f(x + eps) - f(x - eps)) / (2 * eps);
        if (ddf is null)
            ddf = (x) => (df(x + eps) - df(x - eps)) / (2 * eps);

        double xn = x0;
        double x;
        do
        {
            x = xn;
            xn = x - 2 * f(x) * df(x) / (2 * df(x) * df(x) - f(x) * ddf(x));
        } while (Math.Abs(x - xn) > eps);

        return xn;
    }

    public static double FindLinearRoot()
    {
        // TODO: FindLinearRoot f(x) = 2 * x - 1
        return HalleyRootFinding((x) => 2 * x - 1, _ => 2, _ => 0);
        // return default;
    }

    public static double FindQuadraticRoot()
    {
        // TODO: FindQuadraticRoot f(x) = 2 * x * x + 3 * x + 1
        return HalleyRootFinding((x) => 2 * x * x + 3 * x + 1);
        // return default;
    }

    public static double FindLogRoot()
    {
        // TODO: FindLogRoot f(x) = log(x)
        return HalleyRootFinding(double.Log, 
        double.ReciprocalEstimate, x0: 2);
        // funkcje przekazane jako referencje - delegat Function oczekuje
        // double i zwraca double, a double.Log pasuje idealnie
        // public static double Log(double x)

        // parametry z etykietami moge w dowolnej kolejnosci

        // return default;
    }
}