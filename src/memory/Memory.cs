using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.Text;

namespace N_Howl.N_Memory;
public unsafe static class Memory{

public static MemoryArena PersistentArena;
public static MemoryArena TransientArena;

public static bool Init(
    ref MemoryArena pool, nuint capacity
){
    if (pool.IsInitialised){
        Howl.Debug.Panic("Already Intialised.");
        return false;
    }

    pool.IsInitialised = true;
    pool.Capacity = capacity;
    pool.StartPtr = (byte*)NativeMemory.Alloc(capacity);            
    return true;
}

public static bool Init(
    ref MemoryArena arena, ref MemoryArena parent, nuint capacity
){
    if (arena.IsInitialised){
        Howl.Debug.Panic("Already Intialised.");
        return false;
    }

    arena.IsInitialised = true;
    if(Init(ref arena.StartPtr, ref parent, capacity)){
        arena.Capacity = capacity;
        return true;
    }

    return false;
}     

public static bool Init(
    ref byte* childArenaPtr, ref MemoryArena parentArena, nuint capacity
){
    nuint newUsed = parentArena.Used + capacity;
    if(newUsed > parentArena.Capacity){
        Howl.Debug.Panic("Memory Limit Exceeded: Requested address space is too large for the remaining Memory Arena.");
        return false; 
    }

    childArenaPtr = parentArena.StartPtr + parentArena.Used;
    parentArena.Used = newUsed;
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref MemoryArena arena
){
    arena.Used = 0;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearZeroed(
    ref MemoryArena arena
){
    NativeMemory.Clear(arena.StartPtr, arena.Used);
    arena.Used = 0;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static T* PushArray<T>(
    ref MemoryArena arena, int length
) where T : unmanaged{
    
    /**
    NOTE: gemini wrote this, dont know why it is needed so i commented it out,
    honestly doesnt make any sense lol,        

    Ensure alignment (Odin and Jai do this automatically under the hood)
            nuint alignment = (nuint)sizeof(nuint);
            _offset = (_offset + alignment - 1) & ~(alignment - 1);
    **/

    nuint sizeNeeded = (nuint)length * (nuint)sizeof(T);
    nuint newUsed = arena.Used + sizeNeeded;
    System.Diagnostics.Debug.Assert(newUsed <= arena.Capacity, "Memory Limit Exceeded.");
    T* ptr = (T*)(arena.StartPtr + arena.Used);
    arena.Used = newUsed;
    return ptr;
}

public static T* PushStructRaw<T>(
    ref MemoryArena arena
) where T : unmanaged{

    nuint sizeNeeded = (nuint)sizeof(T);
    void* ptr = arena.StartPtr + arena.Used;
    arena.Used += sizeNeeded;
    System.Diagnostics.Debug.Assert(arena.Used <= arena.Capacity, "Memory Limit Exceeded.");
    return (T*)ptr;
}


public static ref T PushStruct<T>(
    ref MemoryArena arena) 
where T : unmanaged{        
    return ref Unsafe.AsRef<T>(PushStructRaw<T>(ref arena));
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static nuint Kilobytes(
    nuint value
){
    return value * 1024;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static nuint Megabytes(
    nuint value
){
    return Kilobytes(value) * 1024;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static nuint Gigabytes(
    nuint value
){
    return Megabytes(value) * 1024;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Free(
    ref MemoryArena arena
){
    System.Diagnostics.Debug.Assert(arena.IsInitialised);
    NativeMemory.Free(arena.StartPtr);
    arena.IsInitialised = false;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool ToStringUTF8(
    byte* source, ref String destination
){
    int sourceLength = 0;
    while (true){
        if(source[sourceLength] == '\0'){
            break;
        }
        sourceLength++;
    }

    int charCount = System.Text.Encoding.UTF8.GetCharCount(source, sourceLength);
    if(destination.Length < charCount){
        Howl.Debug.Panic("Insufficient string length.");
        return false;
    }
    System.Text.Encoding.UTF8.GetChars(source, sourceLength, destination.Pointer, charCount);
    destination.Count = charCount;
    return true;
}

public static void Copy<T>(
    byte* source, byte* destination
) where T : unmanaged{
    System.Span<byte> sourceBytes = new System.Span<byte>(source, sizeof(T));
    System.Span<byte> destinationBytes = new System.Span<byte>(destination, sizeof(T));
    sourceBytes.CopyTo(destinationBytes); 
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Copy<T>(
    byte* source, byte* destination, int length
) where T : unmanaged{
    NativeMemory.Copy(source, destination, (nuint)ArraySizeInBytes<T>(length));
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Copy(
    byte* source, byte* destination, int length, int elementSizeInBytes
){
    NativeMemory.Copy(source, destination, (nuint)(length * elementSizeInBytes));
}

public static int ArrayLengthFromBytes<T>(
    nuint sizeInBytes
) where T : unmanaged
{
    int bytes = 0;
#if DEBUG
    // throw an exception in debug if there isnt enough space in the int.
    bytes = int.CreateChecked(sizeInBytes);
#else
    // drop awway the check in release; truncating the overflowed bits. 
    bytes = int.CreateTruncating(sizeInBytes);
#endif
    return bytes / sizeof(T);
}

public static int ArrayLengthFromBytes<T>(
    long sizeInBytes
) where T : unmanaged
{
    int bytes = 0;
#if DEBUG
    // throw an exception in debug if there isnt enough space in the int.
    bytes = int.CreateChecked(sizeInBytes);
#else
    // drop awway the check in release; truncating the overflowed bits. 
    bytes = int.CreateTruncating(sizeInBytes);
#endif
    return bytes / sizeof(T);
}

public static int ArrayLengthFromBytes<T>(
    int sizeInBytes
) where T : unmanaged
{
    int bytes = 0;
#if DEBUG
    // throw an exception in debug if there isnt enough space in the int.
    bytes = int.CreateChecked(sizeInBytes);
#else
    // drop awway the check in release; truncating the overflowed bits. 
    bytes = int.CreateTruncating(sizeInBytes);
#endif
    return bytes / sizeof(T);
}

public static int ArraySizeInBytes<T>(int arrayLength) where T : unmanaged
{
    return arrayLength * sizeof(T);
}

}
