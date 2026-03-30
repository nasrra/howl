
using System;

namespace Howl.Algorithms;

public static class Swap
{
    /// <summary>
    /// Swaps elements in a span in accordance with a permutation span of indices.
    /// </summary>
    /// <typeparam name="T">the type of data that will be permutated.</typeparam>
    /// <param name="input">the input values to be permutated by the <paramref name="indices"/> order of elements.</param>
    /// <param name="indices">the span of indices in the '<paramref name="input"/>' span that define the order elements of the the resulting swap.</param>
    /// <param name="output">the output span to store the newly permutated span.</param>
    public static void PermuteTo<T>(Span<T> input, Span<int> indices, Span<T> output)
    {
        for(int i = 0; i < input.Length; i++)
        {
            output[i] = input[indices[i]];
        }
    }

    /// <summary>
    /// Swaps elements in a span in accordance with a permutation span of indices.
    /// </summary>
    /// <typeparam name="T">the type of data that will be permutated.</typeparam>
    /// <param name="input">the input values to be permutated by the <paramref name="indices"/> order of elements.</param>
    /// <param name="indices">the span of indices in the '<paramref name="input"/>' span that define the order elements of the the resulting swap.</param>
    /// <param name="output">the output span to store the newly permutated span.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void PermuteTo<T>(Span<T> input, Span<int> indices, Span<T> output, int start, int length)
    {
        int end = length + start;
        Span<int> indicesSlice = indices.Slice(start, length);
        for(int i = start; i < end; i++)
        {
            output[i] = input[indicesSlice[i-start]];
        }

        // insert unchanged the 'head' of the input values back into their places.
        for(int i = 0; i < start; i++)
        {
            output[i] = input[i];
        }

        // insert unchanged the 'tail' of the input values back into their places.
        for(int i = end; i < input.Length; i++)
        {
            output[i] = input[i];            
        }
    }

    /// <summary>
    /// Swaps elements in a span in accordance with a permutation span of indices.
    /// </summary>
    /// <typeparam name="T">the type of data that will be permutated.</typeparam>
    /// <param name="values">the input values to be permutated by the <paramref name="indices"/> order of elements; storing the end permutation result.</param>
    /// <param name="indices">the span of indices in the '<paramref name="input"/>' span that define the order elements of the the resulting swap.</param>
    /// <param name="buffer">a scratch buffer used for swapping values in the '<paramref name="values"/>' span.</param>
    public static void PermuteInPlace<T>(Span<T> values, Span<int> indices, Span<T> buffer)
    {
        for(int i = 0; i < values.Length; i++)
        {
            buffer[i] = values[indices[i]];            
        }
        buffer.CopyTo(values);
    }

    /// <summary>
    /// Swaps elements in a span in accordance with a permutation span of indices.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T">the type of data that will be permutated.</typeparam>
    /// <param name="values">the input values to be permutated by the <paramref name="indices"/> order of elements; storing the end permutation result.</param>
    /// <param name="indices">the span of indices in the '<paramref name="input"/>' span that define the order elements of the the resulting swap.</param>
    /// <param name="buffer">a scratch buffer used for swapping values in the '<paramref name="values"/>' span.</param>
    /// <param name="start">the index of the first element to process.</param>
    /// <param name="length">the total number of elements after '<paramref name="start"/>' to process.</param>
    public static void PermuteInPlace<T>(Span<T> values, Span<int> indices, Span<T> buffer, int start, int length)
    {
        int end = length + start;
        Span<int> indicesSlice = indices.Slice(start, length);
        
        for(int i = start; i < end; i++)
        {
            buffer[i] = values[indicesSlice[i-start]];
        }

        Span<T> valuesSlice = values.Slice(start, length);
        Span<T> tempSlice = buffer.Slice(start, length);
        tempSlice.CopyTo(valuesSlice);
    }
}