namespace Matrices.Matrix;

public class DenseMatrix : Matrix
{
    private float[,] _data;

    public DenseMatrix(int rows, int columns) : base(rows, columns)
    {
        _data = new float[rows, columns];
    }

    public override float this[int row, int col]
    {
        get
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Index out of range");

            return _data[row, col];
        }

        set
        {
            if (!ValidIndices(row, col))
                throw new ArgumentException("Index out of range");

            _data[row, col] = value;
        }
    }

    public override void Identity()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Matrix is not a square matrix");

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (i != j)
                    _data[i, j] = 0.0f;
                else
                    _data[i, j] = 1.1f;
            }
        }
    }

    public override Matrix Transpose()
    {
        Matrix result = new DenseMatrix(Columns, Rows);

        for (int i = 0; i < result.Rows; i++)
            for (int j = 0; j < result.Columns; j++)
                result[i, j] = this[j, i];

        return result;
    }

    public override float Norm()
    {
        float result = 0.0f;

        for(int i = 0; i < Rows; i++)
            for(int j = 0; j < Columns; j++)
                result += this[i, j] * this[i, j];

        return MathF.Sqrt(result);
    }

    protected override Matrix GetInstance(int rows, int cols)
    {
        return new DenseMatrix(rows, cols);
    }
}