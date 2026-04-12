public static class FixedStrideArray
{
    /// <summary>
    ///     Gets the element index of an entry's element in a fixed stride array. 
    /// </summary>
    /// <param name="entryIndex">the index of the entry in the array.</param>
    /// <param name="stride">the stride of each entry in the array.</param>
    /// <param name="entryElementIndex">the index of the element in the entry.</param>
    /// <returns>the index in the fixed stride array to the entry's element.</returns>
    public static int GetElementIndex(int entryIndex, int stride, int entryElementIndex)
    {
        return entryIndex * stride + entryElementIndex;
    }
}