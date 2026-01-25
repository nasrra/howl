using System;

namespace Howl.ECS;

/// <summary>
/// Thrown when attempting to deallocate a dense entry in an GenIndexList when it was never allocated.
/// </summary>
public sealed class DenseNotAllocatedException : Exception
{
    public DenseNotAllocatedException(GenIndex genIndex)
    : base($"Attempted to deallocate a dense entry '{genIndex}' that was never allocated.")
    {
        
    }
}