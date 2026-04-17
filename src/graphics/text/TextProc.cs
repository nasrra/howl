using System;
using System.Runtime.CompilerServices;

namespace Howl.Graphics.Text;

public static class TextProc
{




    /******************

        Text 16
    
    ********************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text16 text, float source)
    {
        AppendCharacters(
            text.AsSpan(),
            source,
            text.Length,
            Text16.MinCharacters,
            Text16.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text16 text, ReadOnlySpan<char> characters)
    {
        AppendCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text16 text, ReadOnlySpan<char> characters, int charactersLength)
    {
        AppendCharacters(
            text.AsSpan(),
            characters,
            text.Length,
            charactersLength,
            Text16.MinCharacters,
            Text16.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text16 text, float source)
    {
        SetCharacters(
            text.AsSpan(),
            source,
            Text16.MinCharacters,
            Text16.MaxCharacters,
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text16 text, ReadOnlySpan<char> characters)
    {
        SetCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text16 text, ReadOnlySpan<char> characters, int chararctersLength)
    {
        SetCharacters(
            text.AsSpan(), 
            characters, 
            chararctersLength,
            Text16.MinCharacters, 
            Text16.MaxCharacters, 
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCharacters(ref Text16 text)
    {
        ClearCharacters(text.AsSpan(), Text16.MaxCharacters);
        text.Length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearUsedCharacters(ref Text16 text)
    {
        ClearCharacters(text.AsSpanUsed(), text.Length);        
        text.Length = 0;
    }




    /******************

        Text 16
    
    ********************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text32 text, float source)
    {
        AppendCharacters(
            text.AsSpan(),
            source,
            text.Length,
            Text32.MinCharacters,
            Text32.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text32 text, ReadOnlySpan<char> characters)
    {
        AppendCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text32 text, ReadOnlySpan<char> characters, int charactersLength)
    {
        AppendCharacters(
            text.AsSpan(),
            characters,
            text.Length,
            charactersLength,
            Text32.MinCharacters,
            Text32.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text32 text, float source)
    {
        SetCharacters(
            text.AsSpan(),
            source,
            Text32.MinCharacters,
            Text32.MaxCharacters,
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text32 text, ReadOnlySpan<char> characters)
    {
        SetCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text32 text, ReadOnlySpan<char> characters, int chararctersLength)
    {
        SetCharacters(
            text.AsSpan(), 
            characters, 
            chararctersLength,
            Text32.MinCharacters, 
            Text32.MaxCharacters, 
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCharacters(ref Text32 text)
    {
        ClearCharacters(text.AsSpan(), Text32.MaxCharacters);
        text.Length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearUsedCharacters(ref Text32 text)
    {
        ClearCharacters(text.AsSpanUsed(), text.Length);        
        text.Length = 0;
    }





    /******************

        Text 4096

    ********************/




    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text4096 text, float value)
    {
        AppendCharacters(
            text.AsSpan(),
            value,
            text.Length,
            Text4096.MinCharacters,
            Text4096.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text4096 text, ReadOnlySpan<char> characters)
    {
        AppendCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(ref Text4096 text, ReadOnlySpan<char> characters, int charactersLength)
    {
        AppendCharacters(
            text.AsSpan(),
            characters,
            text.Length,
            charactersLength,
            Text4096.MinCharacters,
            Text4096.MaxCharacters,
            out int charactersWritten
        );
        text.Length += charactersWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text4096 text, float source)
    {
        SetCharacters(
            text.AsSpan(),
            source,
            Text4096.MinCharacters,
            Text4096.MaxCharacters,
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text4096 text, ReadOnlySpan<char> characters)
    {
        SetCharacters(ref text, characters, characters.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(ref Text4096 text, ReadOnlySpan<char> characters, int chararctersLength)
    {
        SetCharacters(
            text.AsSpan(), 
            characters, 
            chararctersLength,
            Text4096.MinCharacters, 
            Text4096.MaxCharacters, 
            out text.Length
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearCharacters(ref Text4096 text)
    {
        ClearCharacters(text.AsSpan(), Text4096.MaxCharacters);
        text.Length = 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearUsedCharacters(ref Text4096 text)
    {
        ClearCharacters(text.AsSpanUsed(), text.Length);        
        text.Length = 0;
    }




    /*******************
    
        Procedures.
    
    ********************/




    /// <summary>
    /// Replaces the contents of the destination span with characters from the source span.
    /// </summary>
    /// <param name="destination">the destination to write the new characters to.</param>
    /// <param name="source">the float value to replace the destination with.</param>
    /// <param name="minCharacters">the minimum amount of characters the desintation span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    public static void SetCharacters(
        Span<char> destination, 
        float source,
        int minCharacters, 
        int maxCharacters,
        out int charactersWritten
    )
    {
        // format float into char span.
        Span<char> sourceSpan = stackalloc char[maxCharacters];
        source.TryFormat(sourceSpan, out int sourceLength);
        
        SetCharacters(
            destination,
            sourceSpan,
            sourceLength,
            minCharacters,
            maxCharacters,
            out charactersWritten
        );
    }

    /// <summary>
    /// Replaces the contents of the destination span with characters from the source span.
    /// </summary>
    /// <param name="destination">the destination to write the new characters to.</param>
    /// <param name="source">the chararcters to write to the destination.</param>
    /// <param name="minCharacters">the minimum amount of characters the desintation span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetCharacters(
        Span<char> destination, 
        ReadOnlySpan<char> source,
        int minCharacters, 
        int maxCharacters,
        out int charactersWritten
    )
    {
        SetCharacters(destination, source, source.Length, minCharacters, maxCharacters, out charactersWritten);
    }

    /// <summary>
    /// Replaces the contents of the destination span with characters from the source span.
    /// </summary>
    /// <param name="destination">the destination to write the new characters to.</param>
    /// <param name="source">the chararcters to write to the destination.</param>
    /// <param name="sourceLength">the length of source characters - starting from index 0 - to write to the destination.</param>
    /// <param name="minCharacters">the minimum amount of characters the desintation span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    public static void SetCharacters(
        Span<char> destination, 
        ReadOnlySpan<char> source,
        int sourceLength,
        int minCharacters, 
        int maxCharacters,
        out int charactersWritten
    )
    {
        ClearCharacters(destination, maxCharacters);

        charactersWritten = Math.Math.Clamp(sourceLength, minCharacters, maxCharacters);

#if DEBUG
        if(charactersWritten != sourceLength)
        {
            System.Diagnostics.Debug.Assert(
                false, 
                $"Character's length '{charactersWritten}' is not within the range of  '{minCharacters}' and '{maxCharacters}'"
            );
        }
#endif

        source[..charactersWritten].CopyTo(destination);
    }




    /*******************
    
        Append.
    
    ********************/




    /// <summary>
    /// Appends characters to the destination span from the source span. 
    /// </summary>
    /// <param name="destination">the destination to write the characters to.</param>
    /// <param name="source">the float value to append to the destination.</param>
    /// <param name="destinationLength">the amount of stored characters in the destination span.</param>
    /// <param name="minCharacters">the minimum amount of characters the destination span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(
        Span<char> destination,
        float source,
        int destinationLength,
        int minCharacters,
        int maxCharacters,
        out int charactersWritten

    )
    {
        // format the float before writing to the destination.
        Span<char> sourceSpan = stackalloc char[maxCharacters];
        source.TryFormat(sourceSpan, out int sourceLength);
        
        AppendCharacters(
            destination, 
            sourceSpan, 
            destinationLength, 
            sourceLength, 
            minCharacters, 
            maxCharacters, 
            out charactersWritten
        );       
    }

    /// <summary>
    /// Appends characters to the destination span from the source span. 
    /// </summary>
    /// <param name="destination">the destination to write the characters to.</param>
    /// <param name="source">the characters to append to the destination.</param>
    /// <param name="destinationLength">the amount of stored characters in the destination span.</param>
    /// <param name="minCharacters">the minimum amount of characters the destination span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendCharacters(
        Span<char> destination,
        ReadOnlySpan<char> source,
        int destinationLength,
        int minCharacters,
        int maxCharacters,
        out int charactersWritten
    )
    {
        AppendCharacters(
            destination,
            source,
            destinationLength,
            source.Length,
            minCharacters,
            maxCharacters,
            out charactersWritten
        );
    }

    /// <summary>
    /// Appends characters to the destination span from the source span. 
    /// </summary>
    /// <param name="destination">the destination to write the characters to.</param>
    /// <param name="source">the characters to append to the destination.</param>
    /// <param name="destinationLength">the amount of stored characters in the destination span.</param>
    /// <param name="sourceLength">the length of source characters - starting from index 0 - to write to the destination.</param>
    /// <param name="minCharacters">the minimum amount of characters the destination span can store.</param>
    /// <param name="maxCharacters">the maximum amount of characters the destination span can store.</param>
    /// <param name="charactersWritten">the amount of characters written from source to destination.</param>
    public static void AppendCharacters(
        Span<char> destination,
        ReadOnlySpan<char> source,
        int destinationLength,
        int sourceLength,
        int minCharacters, 
        int maxCharacters,
        out int charactersWritten
    )
    {        
        // get the remainding free amount of space to write to.
        maxCharacters -= destinationLength;
        
        charactersWritten = Math.Math.Clamp(sourceLength, minCharacters, maxCharacters);

#if DEBUG
        if(charactersWritten != sourceLength)
        {
            System.Diagnostics.Debug.Assert(
                false, 
                $"Character's length '{source.Length}' is not within the range of  '{minCharacters}' and '{maxCharacters}'"
            );
        }
#endif
        
        // append
        source[..charactersWritten].CopyTo(destination[destinationLength..]);
    }

    /// <summary>
    /// Clears characters in the destination.
    /// </summary>
    /// <param name="destination">the destination span to clear.</param>
    /// <param name="maxCharacters">the amount of characters to clear - starting from index 0.</param>
    public static void ClearCharacters(Span<char> destination, int maxCharacters)
    {
        for(int i = 0; i < maxCharacters; i++)
        {
            destination[i] = '\0';
        }
    }
}