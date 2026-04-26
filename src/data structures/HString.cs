using System;
using System.Runtime.CompilerServices;

namespace Howl.DataStructures;

public class HString
{




    /******************
    
        Member Variables.
    
    *******************/




    /// <summary>
    ///     The character array containing the characters of this string.
    /// </summary>
    public char[] Buffer;

    /// <summary>
    ///     The count of valid characters in the <c>Buffer</c>; starting from index zero.
    /// </summary>
    public int Count;

    /// <summary>
    ///     Whether or not this instance is disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new HString instance.
    /// </summary>
    /// <param name="value">the string value to populate the backing char array with.</param>
    /// <param name="length">the length of the backing char array.</param>
    public HString(string value, int length)
    {
        Buffer = new char[length];
        value.CopyTo(Buffer);
        Count = value.Length;
    }




    /******************
    
        Constructors.
    
    *******************/




    /// <summary>
    ///     Creates a new HString instance.
    /// </summary>
    /// <param name="value">the string value to populate the backing char array with.</param>
    public HString(string value)
    {
        Buffer = new char[value.Length];
        value.CopyTo(Buffer);
        Count = value.Length;
    }

    /// <summary>
    ///     Creates a new HString instance.
    /// </summary>
    /// <param name="length">the length of the backing char array.</param>
    public HString(int length)
    {
        Buffer = new char[length];
        Count = 0;
    }




    /******************
    
        Appending.
    
    *******************/




    /// <summary>
    ///     Sets the <c>Count</c> of an HString instance to zero.
    /// </summary>
    /// <param name="hString">the HString instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(HString hString)
    {
        hString.Count = 0;
    }

    /// <summary>
    ///     Appends an HString to another HString instance.
    /// </summary>
    /// <param name="destination">the string that will be appended to.</param>
    /// <param name="source">the string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(HString destination, HString source)
    {
        Append(destination, source.Buffer.AsSpan(0, source.Count));
    }

    /// <summary>
    ///     Appends a span of characters onto a HString instance.
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="source"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(HString destination, Span<char> source)
    {        
        Span<char> destinationChars = GetInvalidChars(destination);
        source.CopyTo(destinationChars);
        destination.Count += source.Length;
    }

    /// <summary>
    ///     Appends a double value to a HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the double value to append.</param>
    public static void Append(HString destination, double source)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written);        
        destination.Count+=written;
    }

    /// <summary>
    ///     Appends a double value to a HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the double value to append.</param>
    /// <param name="format">the format of the double's character representation.</param>
    public static void Append(HString destination, double source, string format)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written, format);        
        destination.Count+=written;        
    }

    /// <summary>
    ///     Appends a float value to a HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the float value to append.</param>
    public static void Append(HString destination, float source)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written);        
        destination.Count+=written;        
    }

    /// <summary>
    ///     Appends a int value to HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the float value to append.</param>
    /// <param name="format">the format of the float's character representation.</param>
    public static void Append(HString destination, float source, string format)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written, format);        
        destination.Count+=written;                
    }

    /// <summary>
    ///     Appends a int value to a HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the int value to append.</param>
    public static void Append(HString destination, int source)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written);
        destination.Count+=written;
    }

    /// <summary>
    ///     Appends an int value to a HString instance.
    /// </summary>
    /// <param name="destination">the instance to append to.</param>
    /// <param name="source">the int value to append.</param>
    /// <param name="format">the format of the integer's character representation.</param>
    public static void Append(HString destination, int source, string format)
    {
        Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written, format);
        destination.Count+=written;        
    }




    /******************
    
        Utility.
    
    *******************/




    /// <summary>
    ///     Gets a reference to the characters after the <c>Count</c> index of a HString's <c>Buffer</c>.
    /// </summary>
    /// <param name="hString">the instance to get the invalid characters from.</param>
    /// <returns>a span reference to the invalid characters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Span<char> GetInvalidChars(HString hString)
    {
        return hString.Buffer.AsSpan(hString.Count, hString.Buffer.Length - hString.Count);
    }

    /// <summary>
    ///     Gets a char span of the valid characters within a HString <c>Buffer</c>.
    /// </summary>
    /// <param name="hString">the HString to get the character span from.</param>
    /// <returns>A span of valid characters in the HString instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Span<char> AsSpan(HString hString)
    {
        return hString.Buffer.AsSpan(0, hString.Count);
    }

    /// <summary>
    ///     Gets a new <c>string</c> instance of the valid characters in a HString <c>Buffer</c>.
    /// </summary>
    /// <param name="hString">the HString instance to create a new string from.</param>
    /// <returns>the newly created string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string ToString(HString hString)
    {
        return new string(AsSpan(hString));
    }




    /******************
    
        Disposal.
    
    *******************/




    /// <summary>
    ///     Disposes of a HString instance.
    /// </summary>
    public static void Dispose(HString hString)
    {
        if (hString.Disposed)
        {
            return;
        }
        hString.Disposed = true;
        hString.Buffer = null;
        hString.Count = 0;
        GC.SuppressFinalize(hString);
    }

    ~HString()
    {
        Dispose(this);
    }
}