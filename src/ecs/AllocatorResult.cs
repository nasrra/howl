namespace Howl.ECS;

public enum AllocatorResult : byte
{
    ReusedGenIndex,
    InvalidGenIndex,
    AllocatedNewGenIndex,
    DeallocatedGenIndex,
}