using System.Text;

namespace Matrices.Matrix;

public abstract class Matrix
{
    public int Rows { get; set; }
    public int Columns { get; set; }

    protected Matrix(int rows, int columns) // zeby dziedziczace mogly uzyc
    {
        if (rows < 0 || columns < 0)
            throw new ArgumentException("Row and Column count must be greater than zero");

        Rows = rows;
        Columns = columns;
    }

    public abstract float this[int row, int col] { get; set; }

    public override string ToString()
    {
        StringBuilder result = new StringBuilder();

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
                result.AppendFormat($"{this[row, col],8:f3} ");
                // wyrownanie do 8 znakow (dodaje spacje)
                // kazda liczba ma 3 cyfry po przecinku

            if (row < Rows - 1)
                result.Append('\n');
        }

        return result.ToString();
    }

    protected bool ValidIndices(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Columns;
    }

    public abstract void Identity();
    public abstract Matrix Transpose();
    public abstract float Norm();

    protected abstract Matrix GetInstance(int rows, int cols);

    private static bool SameSize(Matrix a, Matrix b)
    {
        if (a.Rows == b.Rows && a.Columns == b.Columns)
            return true;
        return false;
    }

    public static Matrix operator +(Matrix a, Matrix b)
    // STATIC, bo ta metoda nie nalezy do zadnego obiektu klasy, tylko ogolnie
    {
        if (!SameSize(a, b))
            throw new ArgumentException("Invalid matrices sizes for addition");

        Matrix result = a.GetInstance(a.Rows, a.Columns);

        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Columns; j++)
                result[i, j] = a[i, j] + b[i, j];

        return result;
    }

    public static Matrix operator -(Matrix a, Matrix b)
    {
        if (!SameSize(a, b))
            throw new ArgumentException("Invalid matrices sizes for addition");

        Matrix result = a.GetInstance(a.Rows, a.Columns);

        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Columns; j++)
                result[i, j] = a[i, j] - b[i, j];

        return result;
    }

    public static Matrix operator *(float scalar, Matrix a)
    {
        Matrix result = a.GetInstance(a.Rows, a.Columns);

        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < a.Columns; j++)
                result[i, j] = scalar * a[i, j];

        return result;
    }

    public static Matrix operator -(Matrix a, float scalar)
    {
        return scalar * a;
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.Columns != b.Rows)
            throw new ArgumentException("Invalid matrices sizes for multiplication");

        Matrix result = a.GetInstance(a.Rows, b.Columns);

        for (int i = 0; i < a.Rows; i++)
            for (int j = 0; j < b.Columns; j++)
                for (int k = 0; k < a.Columns; k++)
                    result[i, j] += a[i, k] * b[k, j];

        return result;
    }
}