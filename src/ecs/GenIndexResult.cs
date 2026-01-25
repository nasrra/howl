namespace Howl.ECS;

public enum GenIndexResult : byte
{
    Success,    
    DenseNotAllocated,
    StaleGenIndex,
}