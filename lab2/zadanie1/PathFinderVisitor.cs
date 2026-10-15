using System.Text;

namespace Graphs;

public class PathFinderVisitor : IVertexVisitor
{
    private int _target { get; }

    public List<int> Path { get; }
    public bool Found { get; private set; }

    public PathFinderVisitor(int target)
    {
        Path = new List<int>();
        Found = false;
        _target = target;
    }

    public void PreVisit(int vertex)
    {
        if (Found) //juz wczesniej znalezlismy, nie ma co dopisywac
            return;
        Path.Add(vertex); //zapisujemy, ze tu weszlismy
        if(vertex == _target)
            Found = true;
    }

    public void PostVisit(int vertex)
    {
        if (Found)
            return;
        Path.RemoveAt(Path.Count - 1); //cofamy sie ze slepego zaulka
    }

    public override string ToString()
    {
        if (Found == false)
            return "not found";
        string result = string.Join(" --> ", Path);
        return result;
    }
}