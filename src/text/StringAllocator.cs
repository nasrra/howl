using System;
using System.Runtime.CompilerServices;
using Howl.Debug;

namespace Howl.Text;

public static class StringAllocator{   

    /// <summary>
    ///     Allocates a string into a state instance.
    /// </summary>
    /// <param name="state">the state instance to allocate into.</param>
    /// <param name="characters">the characters of the string.</param>
    /// <param name="stringIndex">output for the index of the newly allocated string.</param>
    /// <returns>true, if the string was successfully allocated; otherwise false.</returns>
    public static bool Allocate(StringAllocatorState state, Span<char> characters, ref int stringIndex)
    {
        if(characters.Length > state.MaxCharacterCount)
        {
            Log.WriteLine(LogType.Error, $"Cannot set string '{characters}' as length is greater than max characters '{state.MaxCharacterCount}'.");
            return false;            
        }

        if(state.MaxStringCount <= state.AllocatedStringCount+1)
        {
            Log.WriteLine(LogType.Error, $"Cannot allocate string '{characters}' as the maximum number of allocated strings has been reached.");
            return false;
        }

        stringIndex = state.FreeStringIndices.Pop();
        SetCharsUnsafe(state, characters, stringIndex);
        state.Allocated[stringIndex] = true;
        state.AllocatedStringCount++;

        return true;
    }

    /// <summary>
    ///     Sets the characters of an allocated string.
    /// </summary>
    /// <param name="state">the state instance to write to.</param>
    /// <param name="characters">the characters to write.</param>
    /// <param name="stringIndex">the index of the string to set.</param>
    /// <returns>true, if the string was successfully set; otherwise false.</returns>
    public static bool SetChars(StringAllocatorState state, Span<char> characters, int stringIndex)
    {
        if(characters.Length > state.MaxCharacterCount)
        {
            Log.WriteLine(LogType.Error, $"Cannot set string '{characters}' as length is greater than max characters '{state.MaxCharacterCount}'.");
            return false;            
        }
        
        if(state.Allocated[stringIndex] == false)
        {
            Log.WriteLine(LogType.Error, $"Cannot set string '{characters}' as string index '{stringIndex}' has not been allocated.");
            return false;
        }

        SetCharsUnsafe(state, characters, stringIndex);

        return true;
    }

    /// <summary>
    ///     Sets the characters of an allocated string.
    /// </summary>
    /// <remarks>
    ///     Note: this function bypasses allocation and length checks, directly writing to the underlying characters array.
    /// </remarks>
    /// <param name="state">the state instance to write to.</param>
    /// <param name="characters">the characters to write.</param>
    /// <param name="stringIndex">the index of the string to set.</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void SetCharsUnsafe(StringAllocatorState state, Span<char> characters, int stringIndex)
    {        
        int startIndex = GetFirstCharIndex(state, stringIndex);

        for(int i = 0; i < characters.Length; i++)
        {
            state.Characters[startIndex+i] = characters[i];
        }

        state.TerminatorIndices[stringIndex] = startIndex + characters.Length;
    }

    /// <summary>
    ///     Deallocates a string from a state instance.
    /// </summary>
    /// <param name="state">the state instance deallocate from.</param>
    /// <param name="stringIndex">the index of the allocated string to deallocate.</param>
    /// <returns>true, if the string was deallocated; otherwise false.</returns>
    public static bool Deallocate(StringAllocatorState state, int stringIndex)
    {
        if(state.Allocated[stringIndex] == false)
        {
            Log.WriteLine(LogType.Error, $"Cannot deallocate string at index '{stringIndex}' as it is already deallocated.");
            return false;
        }

        state.Allocated[stringIndex] = false;
        StackArray.Push(state.FreeStringIndices, stringIndex);
        state.AllocatedStringCount--;
        return true;
    }

    /// <summary>
    ///     Sets the Nil string entry of a string allocator instance.
    /// </summary>
    /// <param name="state">the state instance to set the Nil string in.</param>
    /// <param name="characters">the characters of the Nil string.</param>
    /// <returns>true, if the Nil string was successfully set; otherwise false.</returns>
    public static bool SetNil(StringAllocatorState state, Span<char> characters)
    {
        if(characters.Length > state.MaxCharacterCount)
        {
            Log.WriteLine(LogType.Error, $"Cannot set Nil string '{characters}' as length is greater than max characters '{state.MaxCharacterCount}'.");
            return false;            
        }

        SetCharsUnsafe(state, characters, 0);
        
        return true;
    }

    /// <summary>
    ///     Gets a reference to the valid characters stored in a allocated string.
    /// </summary>
    /// <param name="state">the state instance tha  contains the string.</param>
    /// <param name="stringIndex">the index of the string.</param>
    /// <returns>a span to that references the string's characters; otherwise the <c>Nil</c> string if failed.</returns>
    public static Span<char> GetChars(StringAllocatorState state, int stringIndex, ref bool isValid)
    {
        if(state.Allocated[stringIndex] == false)
        {
            Log.WriteLine(LogType.Error, $"Cannot get string at index '{stringIndex}' as it has not been allocated.");
            isValid = false;
            return state.Characters.AsSpan(0, state.TerminatorIndices[0]);
        }

        // write the index to the span.
        int charIndex = GetFirstCharIndex(state, stringIndex);
        int terminateIndex = state.TerminatorIndices[stringIndex];

        isValid = true;
        return state.Characters.AsSpan(charIndex, terminateIndex-charIndex);
    }

    /// <summary>
    ///     Gets the index of the first character in a state instance's characters array.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <param name="stringIndex">the index of the string.</param>
    /// <returns>the index of the first character of the string in the state instance's characters array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetFirstCharIndex(StringAllocatorState state, int stringIndex)
    {
        return stringIndex * state.MaxCharacterCount;
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(StringAllocatorState state)
    {
        if (state.Disposed)
        {
            return;
        }
        state.Disposed = true;
 
        state.Characters = null;
        state.TerminatorIndices = null;
        state.Allocated = null;

        state.MaxCharacterCount = 0;
 
        GC.SuppressFinalize(state);
    }
}