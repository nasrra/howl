using System;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Debug;

namespace Howl.Text;

public static class StringRegistry
{




    /******************
    
        Allocator handling.
    
    *******************/




    /// <summary>
    ///     Allocates a string allocator of a specified stride from a state instance.
    /// </summary>
    /// <remarks>
    ///         Remarks: a single string entry will always be allocated as the <c>Nil</c> element.
    /// </remarks>
    /// <param name="state">the state instance containing the string allocator to deallocate.</param>
    /// <param name="allocatorLength">the max character count of the string allocator.</param>
    /// <param name="maxStringCount">the max amount of strings that can be allocated to the allocator.</param>
    /// <returns></returns>
    public static bool AllocateAllocator(StringRegistryState state, int allocatorLength, int maxStringCount)
    {
        if(state.StringAllocators[allocatorLength] != null)
        {
            Log.WriteLine(LogType.Error, $"Text Allocator already allocated a String Allocator with a max characters count of '{allocatorLength}'");
            return false;
        }

        // append index of the allocator to the active array.
        state.DenseIndices[allocatorLength] = state.Active.Count;
        SwapBackArray.Append(state.Active, allocatorLength);

        state.StringAllocators[allocatorLength] = new StringAllocatorState(allocatorLength, maxStringCount);
        return true;
    }

    /// <summary>
    ///     Deallocates a string allocator of a specified stride from a state instance.
    /// </summary>
    /// <param name="state">the state instance containing the string allocator to deallocate.</param>
    /// <param name="allocatorLength">the max character count of the string allocator.</param>
    /// <returns>true, if the allocator was successfully deallocated; otherwise false.</returns>
    public static bool DeallocateAllocator(StringRegistryState state, int allocatorLength)
    {
        if(state.StringAllocators[allocatorLength] == null)
        {
            Log.WriteLine(LogType.Error, $"Text Allocator already deallocated a String Allocator with a max characters count of '{allocatorLength}'");
            return false;
        }

        int sparseIndex = allocatorLength;
        int denseIndex = state.DenseIndices[sparseIndex];

        // get the dense index that is going to be swapped.
        int swappedSparseIndex = state.Active[state.Active.Count-1];
        
        // set its sparse index to the one that it will be swapped with during removal in the swapback array.
        state.DenseIndices[swappedSparseIndex] = denseIndex;
        
        // set the newly inactive component's dense index to point to the Nil value.
        state.DenseIndices[sparseIndex] = 0;

        // remove the requested id.
        SwapBackArray.RemoveAt(state.Active, denseIndex);

        StringAllocator.Dispose(state.StringAllocators[sparseIndex]);
        state.StringAllocators[sparseIndex] = null;
        return true;        
    }

    /// <summary>
    ///     Gets a reference to an allocated string allocator of a specified stride within a state instance.
    /// </summary>
    /// <param name="state">the state instance containing the allocated string allocator.</param>
    /// <param name="allocatorLength">the max character count of the string allocator.</param>
    /// <returns>a reference to the allocated string allocator; otherwise null if it wasnt allocated.</returns>
    public static StringAllocatorState GetAllocator(StringRegistryState state, int allocatorLength)
    {
        return state.StringAllocators[allocatorLength]; 
    } 





    /******************
    
        String Handling.
    
    *******************/




    /// <summary>
    ///     Allocates a string into a state instance.
    /// </summary>
    /// <remarks>
    ///     Remarks: the stringId will be written to with the Nil string id of the allocator if this allocation fails.
    /// </remarks>
    /// <param name="state">the state instance to allocate into.</param>
    /// <param name="characters">the characters of the string.</param>
    /// <param name="allocatorLength">the amount of characters to allocate for the string.</param>
    /// <param name="stringId">output for the id of the newly allocated string.</param>
    /// <returns>true, if the string was successfully allocated; otherwise false.</returns>
    public static bool AllocateString(StringRegistryState state, Span<char> characters, int allocatorLength, ref StringId stringId)
    {
        StringAllocatorState allocator = state.StringAllocators[allocatorLength]; 
        if(allocator == null)
        {
            Log.WriteLine(LogType.Error, $"Cannot allocate string '{characters}', the requested string allocator at index '{stringId.AllocatorIndex}' has not been intialised.");
            return false;
        }
        
        // attempt to allocate the string.
        int stringIndex = 0;
        bool valid = StringAllocator.Allocate(allocator, characters, ref stringIndex);
        
        stringId = new(stringIndex, allocatorLength);

        return valid;
    }

    /// <summary>
    ///     Sets the characters of an allocated string.
    /// </summary>
    /// <param name="state">the state instance to write to.</param>
    /// <param name="characters">the characters to write.</param>
    /// <param name="allocatorLength">the amount of characters that were allocated to the string.</param>
    /// <param name="stringId">the id of the string to set.</param>
    /// <returns>true, if the string was successfully set; otherwise false.</returns>
    public static bool SetString(StringRegistryState state, Span<char> characters, StringId stringId)
    {
        StringAllocatorState allocator = state.StringAllocators[stringId.AllocatorIndex]; 
        if(allocator == null)
        {
            Log.WriteLine(LogType.Error, $"Cannot set string '{characters}', the requested string allocator at index '{stringId.AllocatorIndex}' has not been intialised.");
            return false;
        }
        return StringAllocator.SetChars(allocator, characters, stringId.StringIndex);
    }

    /// <summary>
    ///     Deallocates a string from a state instance.
    /// </summary>
    /// <param name="state">the state instance deallocate from.</param>
    /// <param name="stringId">the id of the allocated string to deallocate.</param>
    /// <returns>true, if the string was deallocated; otherwise false.</returns>
    public static bool DeallocateString(StringRegistryState state, StringId stringId)
    {
        StringAllocatorState allocator = state.StringAllocators[stringId.AllocatorIndex]; 
        if(allocator == null)
        {
            Log.WriteLine(LogType.Error, $"Cannot deallocate string, the requested string allocator at index '{stringId.AllocatorIndex} ' has not been intialised.");
            return false;
        }
        return StringAllocator.Deallocate(allocator, stringId.StringIndex);
    }

    /// <summary>
    ///     Sets the Nil string entry of a string allocator instance.
    /// </summary>
    /// <param name="state">the state instance to set the Nil string in.</param>
    /// <param name="characters">the characters of the Nil string.</param>
    /// <param name="allocatorLength"></param>
    /// <returns>true, if the Nil string was successfully set; otherwise false.</returns>
    public static bool SetAllocatorNilString(StringRegistryState state, Span<char> characters, int allocatorLength)
    {
        StringAllocatorState allocator = state.StringAllocators[allocatorLength]; 
        if(allocator == null)
        {
            Log.WriteLine(LogType.Error, $"Cannot set Nil string, the requested string allocator at index '{allocatorLength} ' has not been intialised.");
            return false;
        }

        return StringAllocator.SetNil(allocator, characters);
    }

    /// <summary>
    ///     Gets a reference to the valid characters stored in a allocated string.
    /// </summary>
    /// <param name="state">the state instance tha  contains the string.</param>
    /// <param name="stringIndex">the index of the string.</param>
    /// <returns>a span to that references the string's characters; otherwise the <c>Nil</c> string if failed.</returns>
    public static Span<char> GetString(StringRegistryState state, StringId stringId, ref bool isValid)
    {
        StringAllocatorState allocator = state.StringAllocators[stringId.AllocatorIndex]; 
        if(allocator == null)
        {
            Log.WriteLine(LogType.Error, $"Cannot get string, the requested string allocator at index '{stringId.AllocatorIndex} ' has not been intialised.");
            
            // return the registry Nil string instead of the allocator's.
            isValid = false;
            return state.NilString;
        }
        return StringAllocator.GetChars(allocator, stringId.StringIndex, ref isValid);
    }




    /******************
    
        Disposal.
    
    *******************/




    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(StringRegistryState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        state.DenseIndices = null;
        
        for(int i = 1; i < state.Active.Count; i++)
        {
            StringAllocator.Dispose(state.StringAllocators[state.Active[i]]);
        }
        
        state.StringAllocators = null;
        state.NilString = null;
        
        SwapBackArray.Dispose(state.Active);
        state.Active = null;
        
        GC.SuppressFinalize(state);
    }
}