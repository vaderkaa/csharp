namespace Graphs;

public class AdjacencyMatrixGraph : Graph
{
    private bool[,] _matrix;

    public AdjacencyMatrixGraph(int vertexCount) : base(vertexCount)
    {
        _matrix = new bool[vertexCount,vertexCount];
    }

    public bool this[int x, int y]
    {
        get => _matrix[x, y];
        set => _matrix[x, y] = value;
    }

    public override List<int> GetOutVertices(int vertex)
    {
        var result = new List<int>();

        for(int i = 0; i < VertexCount; i++)
        {
            if(_matrix[vertex,i] == true )
                result.Add(i);
        }

        return result;
    }

    public override void AddEdge(int from, int to)
    {
        _matrix[from,to] = true;
    }
}