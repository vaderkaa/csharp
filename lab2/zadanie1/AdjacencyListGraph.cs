namespace Graphs;

public class AdjacencyListGraph : Graph
{
    private List<int>[] _list;

    public AdjacencyListGraph(int vertexCount) : base(vertexCount)
    {
        _list = new List<int>[vertexCount];
        for (int i = 0; i < vertexCount; i++)
            _list[i] = new List<int>();
    }

    public List<int> this[int x]
    {
        get => _list[x];
        set => _list[x] = value;
    }

    public override List<int> GetOutVertices(int vertex)
    {
        return _list[vertex];
    }

    public override void AddEdge(int from, int to)
    {
        _list[from].Add(to);
    }
}