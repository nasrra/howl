namespace Howl.Text;

public struct StringId
{
    /// <summary>
    ///     The index of the string in a string allocator instance.
    /// </summary>
    public int StringIndex;

    /// <summary>
    ///     The index of the string's allocator in a string registry instance.
    /// </summary>
    public int AllocatorIndex;

    /// <summary>
    ///     Constructs a StringId.
    /// </summary>
    /// <param name="stringIndex">the index of the string in a string allocator instance.</param>
    /// <param name="allocatorIndex">the index of the string's allocator in a string registry instance.</param>
    public StringId(int stringIndex, int allocatorIndex)
    {
        StringIndex = stringIndex;
        AllocatorIndex = allocatorIndex;
    }
}