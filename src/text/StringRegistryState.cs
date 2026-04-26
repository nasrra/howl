using System;
using Howl.Collections;

namespace Howl.Text;

public class StringRegistryState
{
    /// <summary>
    ///     The string allocators stored in this state.
    /// </summary>
    /// <remarks>
    ///     Note: a Null element signifies that it is deallocated.
    /// </remarks>
    public StringAllocatorState[] StringAllocators;

    /// <summary>
    ///     An array of associative indicies, pointing a <c>StringAllocators</c> element to a <c>Active</c> element.
    /// </summary>
    public int[] DenseIndices;

    /// <summary>
    ///     The Nil string for this registry instance.
    /// </summary>
    public char[] NilString;

    /// <summary>
    ///     Indices of the active string allocators within <c></c>
    /// </summary>
    /// <remarks>
    ///     Remarks: This collection contains a <c>Nill</c> element.
    /// </remarks>
    public SwapBackArray<int> Active;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new Text Allocator instance.
    /// </summary>
    /// <param name="maxStringCharacters">the maximum amount of characters a string can have.</param>
    public StringRegistryState(int maxStringCharacters)
    {
        // note: add one to maxStringCharacters as arrays are zero indexed:
        // example:
        // [0] = characterCount 0.
        // [1] = characterCount 1.
        int charCount = maxStringCharacters+1;

        StringAllocators = new StringAllocatorState[charCount];
        DenseIndices = new int[charCount];
        Active = new(charCount);

        // set the nil string value to none.
        // this is done to ensure compatability between GetString() calll that fails
        // for instance, if it requested a 32 char string, but the Nil string was 64 chars long
        // the program would crash. By setting it to zero, it will always work.
        NilString = [];

        // append Nil to the first entry.
        SwapBackArray.Append(Active, default);
    }

    ~StringRegistryState()
    {
        StringRegistry.Dispose(this);
    }
}