using System.Collections;

namespace tasks;

/// <summary>
/// Interface for an array-based binary tree.
/// </summary>
public interface IArrayBinaryTree<T> : IEnumerable<T>
{
    /// <summary>
    /// Gets the number of nodes in the tree.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the index of the root.
    /// </summary>
    int RootIndex { get; }

    /// <summary>
    /// Sets the value of the root node.
    /// </summary>
    void SetRoot(T value);

    /// <summary>
    /// Returns the indices of the left and right children for a given parent.
    /// </summary>
    (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex);

    /// <summary>
    /// Sets the value of the left child of a given parent.
    /// </summary>
    void SetLeftChild(int parentIndex, T value);

    /// <summary>
    /// Sets the value of the right child of a given parent.
    /// </summary>
    void SetRightChild(int parentIndex, T value);

    /// <summary>
    /// Gets the node at the specified index.
    /// </summary>
    T this[int index] { get; }

    /// <summary>
    /// Checks if a node at the specified index exists.
    /// </summary>
    bool Exists(int index);

    /// <summary>
    /// Clears all nodes from the tree.
    /// </summary>
    void Clear();
}

/// <summary>
/// An implementation of a binary tree based on an array.
/// Indices are calculated as:
/// - Left Child: 2 * i + 1
/// - Right Child: 2 * i + 2
/// - Parent: (i - 1) / 2
/// </summary>
public class ArrayBinaryTree<T> : IArrayBinaryTree<T>
{
    private T[] _nodes;
    private bool[] _exists;

    // konstruktor
    public ArrayBinaryTree(int initialCapacity = 8)
    {
        if (initialCapacity < 1)
            initialCapacity = 8;
        _nodes = new T[initialCapacity];
        _exists = new bool[initialCapacity];
        Count = 0;
    }

    public T this[int index]
    {
        get
        {
            if (!Exists(index))
                throw new ArgumentOutOfRangeException(nameof(index));
            return _nodes[index];
        }
    }

    public int Count { get; private set; }

    public int RootIndex => 0;

    public void Clear()
    {
        Array.Clear(_nodes, 0, _nodes.Length);
        Array.Clear(_exists, 0, _exists.Length);
        Count = 0;
    }

    public bool Exists(int index)
    {
        return index >= RootIndex && index < _exists.Length && _exists[index];
    }

    // metoda rozszerzajaca tablice
    private void EnsureCapacity(int index)
    {
        if (index >= _nodes.Length)
        {
            // jesli size = 0, to 0*2 da nam 0, wiec musimy ustawic na 1
            int newSize = _nodes.Length == 0 ? 1 : _nodes.Length;
            while (newSize <= index)
                newSize *= 2;
            Array.Resize(ref _nodes, newSize);
            Array.Resize(ref _exists, newSize);
        }
    }

    // do otrzymywania indeksow dzieci
    public (int leftIndex, int rightIndex) GetChildrenIndices(int parentIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(parentIndex, nameof(parentIndex));

        return (2 * parentIndex + 1, 2 * parentIndex + 2);
    }

    private void SetAt(int index, T value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

        if (index != RootIndex) // jesli nie jest rootem, to ma rodzica
        {
            var parent = (index - 1) / 2;
            if (!Exists(parent))
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        EnsureCapacity(index);

        if (!_exists[index]) // jesli nie istnieje, to istnieje i ustawiamy count
        {
            _exists[index] = true;
            Count++;
        }

        _nodes[index] = value;
    }

    public void SetRoot(T value)
    {
        SetAt(RootIndex, value);
    }

    public void SetLeftChild(int parentIndex, T value)
    {
        if (!Exists(parentIndex))
            throw new ArgumentOutOfRangeException(nameof(parentIndex));

        var left = GetChildrenIndices(parentIndex).leftIndex;
        SetAt(left, value);
    }

    public void SetRightChild(int parentIndex, T value)
    {
        if (!Exists(parentIndex))
            throw new ArgumentOutOfRangeException(nameof(parentIndex));

        var right = GetChildrenIndices(parentIndex).rightIndex;
        SetAt(right, value);
    }

    private IEnumerable<T> InOrder(int index)
    {
        if (!Exists(index))
            yield break;

        var (left, right) = GetChildrenIndices(index);

        foreach (var v in InOrder(left))
            yield return v;

        yield return _nodes[index];

        foreach (var v in InOrder(right))
            yield return v;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return InOrder(RootIndex).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}