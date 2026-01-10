namespace Howl.ECS;

public struct DenseEntry<T>
{
    /// <summary>
    /// The index in a generational index arrays sparse list that serves as an associative link
    /// from the generational index to the value/data stored in this entry.
    /// </summary>

    public int sparseIndex;

    /// <summary>
    /// The data stored by this entry.
    /// </summary>

    public T value;

    public override string ToString()
    {
        return $"[DenseEntry] sparseIndex:{sparseIndex}";
    }
}