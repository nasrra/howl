using System;

namespace Howl.ECS;

/// <summary>
/// Thrown when querying a GenIndexList finds a dense entry of a previous generation.
/// </summary>
public sealed class StaleDenseAllocationException: Exception
{
    public StaleDenseAllocationException(GenIndex staleAllocationGenIndex, GenIndex queriedGenIndex)
    : base($"Stale allocation '{staleAllocationGenIndex}' found for queried GenIndex '{queriedGenIndex}'.")
    {}
}