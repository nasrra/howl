using System;
using System.Diagnostics;

public struct GenId
{
    /// <summary>
    ///     The max index value a genId can have.
    /// </summary>
    /// <remarks>
    ///     This is because the first 20 bits of a uint are used for a indexing. 
    /// </remarks>
    public const int MaxIndex = UniqueIndicesCount - 1;

    /// <summary>
    ///     The total amount of unique index values starting from zero.
    /// </summary>
    /// <remarks>
    ///     This is because the first 20 bits of a uint are used for a indexing. 
    /// </remarks>
    public const int UniqueIndicesCount = 1 << 20; // 1,048,576

    /// <summary>
    ///     The max generation value a gen id can have.
    /// </summary>
    /// <remarks>
    ///     This is because the last 12 bits of a uint are used for the generational value.
    /// </remarks>
    public const int MaxGeneration = UniqueGenerationsCount - 1;

    /// <summary>
    ///     The total amount of unique generation values starting from zero.
    /// </summary>
    /// <remarks>
    ///     This is because the last 12 bits of a uint are used for the generational value.
    /// </remarks>
    public const int UniqueGenerationsCount = 1 << 12; // 4,096; 

    /// <summary>
    ///     The bit-wise mask used to extract the index value from the gen id.
    /// </summary>
    public const uint IndexMask = 0xFFFFF; // binary: 0000 0000 0000 1111 1111 1111 1111 1111.

    /// <summary>
    ///     The bit-wise mask used to extract the generation value from the gen id.
    /// </summary>
    public const uint GenerationMask = 0xFFF; // binary: 0000 0000 0000 0000 0000 1111 1111 1111.

    /// <summary>
    ///     The decimal value of the bitwised packed index and generation values.
    /// </summary>
    public uint Value;

    /// <summary>
    ///     Constructs a GenId.
    /// </summary>
    /// <remarks>
    ///     <c><paramref name="index"/></c> must be between 0 and 1,048,576. <c><paramref name="generation"/></c> must be between
    ///     0 and 4,096.
    /// </remarks>
    /// <param name="index">the index value.</param>
    /// <param name="generation">the generation value.</param>
    public GenId(int index, int generation)
    {
        Debug.Assert(index >= 0 && index <= MaxIndex, $"index value '{index}' is not between minimum '0' and maximum value '{MaxIndex}'");
        Debug.Assert(generation >= 0 && generation <= MaxGeneration, $"generation value '{generation}' is not between minimum '0' and maximum value '{MaxGeneration}'");

        // shift generation up by 20 bit so its the last 12 bits in the integer. 
        Value = (uint)(generation & GenerationMask) << 20; // apply the mask anyways so there is no crash in release mode.

        // Or with the index to that the index values are the first 20 bits in the integer.
        Value |= (uint)index & IndexMask; // apply the mask anyways so there is no crash in release mode.
    }

    /// <summary>
    ///     Increments the generational value of a gen id by one.
    /// </summary>
    /// <remarks>
    ///     the generational slice of the integer will be wrapped around automatically when exceeding its max value.
    /// </remarks>
    /// <param name="genId">the gen id to increment.</param>
    /// <returns>the newly constructed incremented gen id.</returns>
    public static GenId IncrementGeneration(GenId genId)
    {
        // adding (1<<20) effectively adds 1 to the generation slice of the integer.
        // if the generation was at 4095, adding 1 makes it 4096; which would
        // "overflow" out of the 32-bit uint, wrapping back to 0 naturally.

        int nexGen = (GetGeneration(genId)+1) & (int)GenerationMask;
        return new GenId(GetIndex(genId), nexGen);        
    }

    /// <summary>
    ///     Increments the index value of a gen id by one.
    /// </summary>
    /// <param name="genId">the gen id to increment.</param>
    /// <returns>the newly constructed incremented gen id.</returns>
    public static GenId IncrementIndex(GenId genId)
    {
        // Get the current index and add 1.
        // mask it so the index value stays within th 20 bit range; wrapping around to zero if it hits max index.
        // this preserves the existing generation bits from overflow corruption of the index value.
        uint currentGen = genId.Value & ~IndexMask; // Isolate the top 12 bits;
        uint nextIndex = (genId.Value + 1) & IndexMask;
        return new GenId{Value = currentGen | nextIndex};
    }

    /// <summary>
    ///     Gets the index value that is packed inside a gen id.
    /// </summary>
    /// <param name="genId">the gen id to extract an index from.</param>
    /// <returns>the extracted index value.</returns>
    public static int GetIndex(GenId genId)
    {
        return (int)(genId.Value & IndexMask);
    }

    /// <summary>
    ///     Gets the generation value that is packed inside of a gen id.
    /// </summary>
    /// <param name="genId">the gen id to extract a generation value from.</param>
    /// <returns>the extracted generation value.</returns>
    public static int GetGeneration(GenId genId)
    {
        return (int)genId.Value >> 20;
    }

    /// <summary>
    ///     Checks if two gen ids are equal. 
    /// </summary>
    /// <param name="a">gen id a.</param>
    /// <param name="b">gen id b.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    public static bool operator ==(GenId a, GenId b)
    {
        return a.Value == b.Value;
    }

    /// <summary>
    ///     Checks if two gen ids are not equal.
    /// </summary>
    /// <param name="a">gen id a.</param>
    /// <param name="b">gen id b.</param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    public static bool operator !=(GenId a, GenId b)
    {
        return a.Value != b.Value;
    }

    /// <summary>
    ///     Checks if an object is equal to this gen id.
    /// </summary>
    /// <param name="obj">the object to check equality against.</param>
    /// <returns>true, if the object is equal to this; otherwise false.</returns>
    public override bool Equals(object obj)
    {
        return obj is GenId other && other == this; 
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}