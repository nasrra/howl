namespace N_Howl.N_Memory;

public unsafe struct MemoryArena{
    public byte* StartPtr;
    public nuint Used;
    public nuint Capacity;
    public bool IsInitialised;
}