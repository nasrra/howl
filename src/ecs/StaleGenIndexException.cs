using System;

namespace Howl.ECS;

public sealed class StaleGenIndexException : Exception
{
    public StaleGenIndexException(GenIndex queriedGenIndex)
    : base($"GenIndex '{queriedGenIndex}' is stale.")
    {    
    }
}