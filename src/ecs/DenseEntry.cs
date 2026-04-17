namespace Howl.Ecs;

public struct DenseEntry<T>
{
    /// <summary>
    /// The index in a generational index arrays sparse list that serves as an associative link
    /// from the generational index to the value/data stored in this entry.
    /// </summary>

    public int sparseIndex;

    /// <summary>
    /// Gets and sets the stored data.
    /// </summary>

    public T Value;

    public override string ToString()
    {
        return $"[DenseEntry] sparseIndex:{sparseIndex}";
    }
}