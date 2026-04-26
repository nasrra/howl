using Howl.Test.Collections;
using Howl.Text;

namespace Howl.Test.Text;

public static class Assert_StringAllocatorState
{
    /// <summary>
    ///     Asserts the length of backing arrays of a state instance.
    /// </summary>
    /// <param name="maxCharacterCount">the expected max character count.</param>
    /// <param name="maxStringCount">the expected max string count.</param>
    /// <param name="state">the state instance to assert against.</param>
    public static void LengthEqual(int maxCharacterCount, int maxStringCount, StringAllocatorState state)
    {
        int charLength = maxCharacterCount * maxStringCount;
        Assert.Equal(maxCharacterCount, state.MaxCharacterCount);
        Assert.Equal(maxStringCount, state.MaxStringCount);
        Assert.Equal(maxStringCount, state.Allocated.Length);
        Assert_StackArray.LengthEqual(maxStringCount, state.FreeStringIndices);
        Assert.Equal(maxStringCount, state.TerminatorIndices.Length);
        Assert.Equal(charLength, state.Characters.Length);
    }

    /// <summary>
    ///     Asserts the euality of a string entry in a string allocator state.
    /// </summary>
    /// <param name="characters">the expected characters.</param>
    /// <param name="terimatorIndex">the expected terminator indice.</param>
    /// <param name="allocated">the expected allocated flag.</param>
    /// <param name="stringIndex">the index of the string in the state.</param>
    /// <param name="state">the state instance to assert against.</param>
    public static void EntryEqual(Span<char> characters, int terimatorIndex, bool allocated, int stringIndex, StringAllocatorState state)
    {
        int firstCharIndex = StringAllocator.GetFirstCharIndex(state, stringIndex);
        for(int i = 0; i < characters.Length; i++)
        {
            Assert.Equal(characters[i], state.Characters[i+firstCharIndex]);
        }
        Assert.Equal(terimatorIndex, state.TerminatorIndices[stringIndex]);
        Assert.Equal(allocated, state.Allocated[stringIndex]);
    }
}