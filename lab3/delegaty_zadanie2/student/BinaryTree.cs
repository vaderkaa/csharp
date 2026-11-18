namespace Lab10EN;

public class BinaryTree<T> where T : notnull
{
    public class Node
    {
        public T Value { get; set; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }

        public Node(T value)
        {
            Value = value;
        }

        public Node(T value, Node? left, Node? right)
        {
            Value = value;
            Left = left;
            Right = right;
        }
    }

    public Node? Root { get; set; }

    public BinaryTree(Node? root)
    {
        Root = root;
    }

    public void BreadthFirstSearch(Action<Node> onVisit)
    {
        Queue<Node> queue = new Queue<Node>();
        if (Root is not null) // Root jest nullowalny i moze byc null
            queue.Enqueue(Root);
        while(queue.Count > 0)
        {
            Node N = queue.Dequeue();
            onVisit?.Invoke(N); // na wypadek czy delegat nie jest nullem
            if (N.Left is not null)
                queue.Enqueue(N.Left);
            if (N.Right is not null)
                queue.Enqueue(N.Right);
        }
    }

    public int ValueCount(T value)
    {
        int count = 0;
        BreadthFirstSearch(node =>
        {
            if (Comparer<T>.Default.Compare(node.Value, value) == 0)
                count++;
        });
        return count;
        // return default;
    }
}