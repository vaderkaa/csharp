namespace Matrices.Matrix;

public class SparseMatrix : Matrix
{
    private Dictionary<(int, int), float> _data;

    public SparseMatrix(int rows, int columns) : base(rows, columns)
    {
        _data = new Dictionary<(int, int), float>();
    }

    public override float this[int row, int col]
    {
        get
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Indes out of range");

            return _data.GetValueOrDefault((row, col), 0.0f);
            // jesli nic nie ma pod tym kluczem, to zwroci 0
        }

        set
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Index out of range");

            if (value == 0.0f)
            {
                _data.Remove((row, col));
                return;
            }

            _data[(row,col)] = value;
        }
    }

    public override void Identity()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix is not a square matrix");

        _data.Clear();

        for (int i = 0; i < Rows; i++)
            _data[(i, i)] = 1.0f;
    }

    public override Matrix Transpose()
    {
        Matrix result = new SparseMatrix(Columns, Rows);

        foreach(var item in _data)
        {
            (int row, int col) = item.Key;
            result[col, row] = item.Value;
            // dziala polimorficznie, bo Matrix ma operator
        }

        return result;
    }

    public override float Norm()
    {
        float result = 0.0f;

        foreach(var val in _data.Values)
            result += val * val;

        return MathF.Sqrt(result);
    }

    protected override Matrix GetInstance(int rows, int cols)
    {
        return new SparseMatrix(rows, cols);
    }
}