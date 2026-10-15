namespace Graphs;

public abstract class Graph
{
    public int VertexCount { get; }

    protected Graph(int vertexCount)
    {
        VertexCount = vertexCount;
    }

    public abstract List<int> GetOutVertices(int vertex);

    public abstract void AddEdge(int from, int to);

    public void Display()
    {
        for(int i = 0; i < VertexCount; i++)
        {
            Console.Write($"{i} -> [");
            var list = GetOutVertices(i);
            for (int index = 0; index < list.Count - 1; index++)
                Console.Write($"{list[index]},");
            if(list.Any())
                Console.Write(list[^1]);
            Console.WriteLine("]");
        }
    }

    public void DepthFirstSearch(int start, IVertexVisitor visitor)
    {
        bool[] visited = new bool[VertexCount];
        DepthFirstSearchRecursive(start, visited, visitor);
    }
    
    void DepthFirstSearchRecursive(int vertex, bool[] visited, IVertexVisitor visitor)
    {
        visited[vertex] = true;
        visitor.PreVisit(vertex);

        foreach(var neighbor in GetOutVertices(vertex))
        {
            if (visited[neighbor] == false)
                DepthFirstSearchRecursive(neighbor, visited, visitor);
        }

        visitor.PostVisit(vertex);
    }
}