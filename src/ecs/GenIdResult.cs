public enum GenIdResult : byte
{
    /// <summary>
    ///     Returned when a gen index process has completed successfully.
    /// </summary>
    Ok,

    /// <summary>
    ///     Returned when the querying GenId is of a generation that does not equal the allocated GenId generation.
    /// </summary>
    StaleGenId,

    /// <summary>
    ///     Returned when a GenId accesses an unallocated value in a array when attempting to return an allocated entry. 
    /// </summary>
    NotAllocated,

    /// <summary>
    ///     Returned when the amount of allocated memory of a given GenIndexArray has been reached; meaning that all slots in the backing arrays are allocated.
    /// </summary>
    MemoryLimitHit
}