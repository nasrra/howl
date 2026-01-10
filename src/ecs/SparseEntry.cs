namespace Howl.ECS;

public struct SparseEntry
{
    /// <summary>
    /// The generational value in relation to a GenIndex.
    /// </summary>

    public int generation;

    /// <summary>
    /// The index in a GenIndexCollection's list that stores the data related to the generational index.
    ///     Note: when the dense index is set to a below zero value, this sparse entry is not linked to a dense entry. 
    /// </summary>

    int denseIndex;

    /// <summary>
    /// Gets the index in a GenIndexCollection's list that stores the data related to the generational index.
    /// </summary>

    public int DenseIndex => denseIndex;

    public SparseEntry()
    {
        generation = 0;
        denseIndex = -1;
    }

    public SparseEntry(int generation, int denseIndex)
    {
        this.generation = generation;
        this.denseIndex = denseIndex;
    }

    public SparseEntry(in GenIndex genIndex)
    {
        generation = genIndex.generation;
        denseIndex = -1;
    }

    /// <summary>
    /// Checks whether or not this sparse entry is linked to a dense entry.
    /// </summary>
    /// <returns>true, when linked; otherwise false.</returns>

    public bool LinkedToADenseEntry()
    {
        return denseIndex >= 0;
    }

    /// <summary>
    /// Sets the dense index value, linking this sparse entry to a dense entry in a GenIndexList.
    /// </summary>
    /// <param name="value"></param>

    public void SetDenseIndex(int value)
    {
        denseIndex = value;
    }

    /// <summary>
    /// Sets the dense index value to -1, meaning that this sparse entry is not linked to a dense entry. 
    /// </summary>

    public void ClearDenseIndex()
    {
        denseIndex = -1;
    }

    public override string ToString()
    {
        return $"[SparseEntry] generation:{generation}, denseIndex:{denseIndex}";
    }
}