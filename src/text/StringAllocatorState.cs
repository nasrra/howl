using Howl.Collections;

namespace Howl.Text;

public class StringAllocatorState
{
    /// <summary>
    ///     The minimum amount strings that qualifies as a valid maximum amount of strings a state instance can store.
    /// </summary>
    public const int MinMaxStringCount = 2;

    /// <summary>
    ///     The minimum amount of characters that qualifies as a valid maximum amount of characters a string can store.
    /// </summary>
    public const int MinMaxCharacterCount = 1;

    /// <summary>
    ///     The characters for all text.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Elements should be accessed via a <c>characterIndex</c>.</item>
    ///     </list>
    /// </remarks>
    public char[] Characters;
    
    /// <summary>
    ///     The indices where a string terminates.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Contains a <c>Nil</c> element.</item>
    ///         <item>Elements should be accessed via <c>stringIndex</c>.</item>
    ///     </list>
    /// </remarks>
    public int[] TerminatorIndices;
    
    /// <summary>
    ///     Whether a string has been allocated.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Contains a <c>Nil</c> element.</item>
    ///         <item>Elements should be accessed via <c>stringIndex</c>.</item>
    ///     </list>
    /// </remarks>
    public bool[] Allocated;

    /// <summary>
    ///     The free indices for string allocation.
    /// </summary>
    public StackArray<int> FreeStringIndices;

    /// <summary>
    ///     The amount of allocated strings this collection currently has. 
    /// </summary>
    /// <remarks>
    ///     Note: this starts at one as there is a <c>Nil</c> element.
    /// </remarks>
    public int AllocatedStringCount;

    /// <summary>
    ///     The maximum amount of allocated strings this collection can store.
    /// </summary>
    public int MaxStringCount;

    /// <summary>
    ///     The fixed stride of characters for each string entry.
    /// </summary>
    public int MaxCharacterCount;

    /// <summary>
    ///     Whether this instance has been diposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new string allocator state instance.
    /// </summary>
    /// <remarks>
    ///     Remarks:
    ///     <list type="bullet">
    ///         <item><c><paramref name="maxCharacterCount"/></c> will be clamped to <c><see cref="MinMaxCharacterCount"/></c>.</item>
    ///         <item><c><paramref name="maxStringCount"/></c> will be clamped to <c><see cref="MinMaxStringCount"/></c>.</item>
    ///         <item>a single string entry will always be allocated as the <c>Nil</c> element.</item>
    ///     </list>
    /// </remarks>
    /// <param name="maxCharacterCount">The fixed stride of characters for each string entry.</param>
    /// <param name="maxStringCount">the maximum amount of strings this collection can store.</param>
    public StringAllocatorState(int maxCharacterCount, int maxStringCount)
    {
        System.Diagnostics.Debug.Assert(maxStringCount >= MinMaxStringCount, 
            $"String Allocator State cannot be intialised with a MaxStringCount of '{maxStringCount}' as it is less than '{MinMaxStringCount}'"
        );

        System.Diagnostics.Debug.Assert(maxCharacterCount >= MinMaxCharacterCount, 
            $"String Allocator State cannot be intialised with a MaxCharacterCount of '{maxCharacterCount}' as it is less than '{MinMaxCharacterCount}'"
        );

        Math.Math.Clamp(maxStringCount, MinMaxStringCount, int.MaxValue);
        Math.Math.Clamp(maxCharacterCount, MinMaxCharacterCount, int.MaxValue);

        int dataLength = maxCharacterCount*maxStringCount;
        Characters = new char[dataLength];
        TerminatorIndices = new int[maxStringCount];
        Allocated = new bool[maxStringCount];
        MaxCharacterCount = maxCharacterCount;
        MaxStringCount = maxStringCount;
 
        FreeStringIndices = new(MaxStringCount);
        for(int i = MaxStringCount; i > 0; i--)
        {
            StackArray.Push(FreeStringIndices, i);
        }
    }

    ~StringAllocatorState()
    {
        StringAllocator.Dispose(this);
    }
}