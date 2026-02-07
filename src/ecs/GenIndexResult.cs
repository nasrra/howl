namespace Howl.ECS;

public enum GenIndexResult : byte
{
    Ok,    

    /// <summary>
    /// Returned when the querying GenIndex has no associated dense entry allocated.
    /// </summary>
    DenseNotAllocated,

    /// <summary>
    /// Returned when the querying GenIndex is of a generation that does not equal the allocated GenIndex generation.
    /// </summary>
    StaleGenIndex,

    /// <summary>
    /// Returned when allocating data to a slot in a GenIndexList already has data associated with the querying GenIndex.    
    /// </summary>
    DenseAlreadyAllocated, 

    /// <summary>
    /// Returned when querying a GenIndexList with a GenIndex that has and index is beyond the array bounds.
    /// </summary>
    InvalidGenIndex,

    /// <summary>
    /// Returned when querying a GenIndexList finds a dense entry of a previous generation.
    /// </summary>
    StaleDenseAllocation
}