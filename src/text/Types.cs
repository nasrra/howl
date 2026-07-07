using System.Runtime.CompilerServices;
using N_Howl.N_Collections;
using N_Howl.N_Ecs;
using N_Howl.N_Memory;

namespace N_Howl.N_Text;

public unsafe struct String{
    public char* Pointer;
    public int Length;
    public int Count;
    public bool IsInitialised;

    public ref char this[int index]{
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get{
            Howl.Debug.Assert(index >= 0 && index < Length, $"Index: '{index}' is Out Of Bounds; String Length: '{Length}' .");
            return ref Pointer[index];
        }
    }
}

public struct StringAllocator{
    public Array<StringSubAllocator> SubAllocators;
    public bool IsInitialised;
    public String FallbackString;
}

public struct StringSubAllocator{
    public GenIdAllocator GenIdAllocator;
    public MemoryArena CharArena;
    public ComponentArray<String> Strings;
    public bool IsInitialised;
}

public struct StringId{
    public GenId GenId;
    public int StringLength;
}    
