using System;
using System.Runtime.CompilerServices;

namespace Howl.DataStructures;

public class HString
{
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
    /// <param name="source">the string to append.</param>
    /// <param name="destination">the string that will be appended to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(HString source, HString destination)
    {
        Span<char> sourceChars = source.Buffer.AsSpan(0, source.Count);
        Span<char> destinationChars = destination.Buffer.AsSpan(destination.Count, destination.Buffer.Length);
        sourceChars.CopyTo(destinationChars);
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