namespace Howl.ECS;

public enum GenIndexResult : byte
{
    Success,
    StaleAllocationFound,
    InvalidGenIndex,
    DenseNotAllocated,
    DoubleAllocationAttempted
}