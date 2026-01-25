using System;

namespace Howl.ECS;

/// <summary>
/// Thrown when allocating data to a slot in a GenIndexList already has data associated with the querying GenIndex.
/// </summary>
public sealed class DuplicateDenseAllocationException : Exception
{
    public DuplicateDenseAllocationException(GenIndex queriedGenIndex)
    : base($"GenIndex {queriedGenIndex} already has data allocated to it.")
    {
        
    }
}