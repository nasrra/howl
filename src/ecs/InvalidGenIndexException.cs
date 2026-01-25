using System;

namespace Howl.ECS;

/// <summary>
/// Thrown when querying a GenIndexList with a GenIndex that has and index is beyond the array bounds.
/// </summary>
public sealed class InvalidGenIndexException : Exception
{
    public InvalidGenIndexException(GenIndex queriedGenIndex)
    : base($"GenIndex '{queriedGenIndex}' is invalid.")
    {
    }
}