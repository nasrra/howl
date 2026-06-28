using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.Text;

namespace Howl;

public unsafe static class Memory
{
    public struct State
    {
        public byte* StartPtr;
        public nuint Capacity; 
        public nuint Used;
        public bool IsInitialised;
    
        public static bool Initialise(ref State state, nuint capacity)
        {
            if (state.IsInitialised)
            {
                Debug.Panic("Already Intialised.");
                return false;
            }

            state.IsInitialised = true;
            state.Capacity = capacity;
            state.StartPtr = (byte*)NativeMemory.Alloc(capacity);            
            return true;
        }
    }

    public struct Arena
    {
        public byte* StartPtr;
        public nuint Used;
        public nuint Capacity;
        public bool IsInitialised;

        public static bool Initialise(ref Arena arena, ref State state, nuint capacity)
        {
            if (arena.IsInitialised)
            {
                Debug.Panic("Already Intialised.");
                return false;
            }
        
            arena.IsInitialised = true;
            if(InitialiseRaw(ref arena.StartPtr, ref state, capacity))
            {                
                arena.Capacity = capacity;
                return true;
            }

            return false;
        }
        
        public static bool Initialise(ref Arena childArena, ref Arena parentArena, nuint capacity)
        {
            if (childArena.IsInitialised)
            {
                Debug.Panic("Already Intialised.");
                return false;
            }
        
            childArena.IsInitialised = true;
            if(InitialiseRaw(ref childArena.StartPtr, ref parentArena, capacity))
            {                
                childArena.Capacity = capacity;
                return true;
            }

            return false;
        }
     
        public static bool InitialiseRaw(ref byte* childArenaPtr, ref Arena parentArena, nuint capacity)
        {
            nuint newUsed = parentArena.Used + capacity;
            if(newUsed > parentArena.Capacity)
            {
                Debug.Panic("Memory Limit Exceeded: Requested address space is too large for the remaining Memory Arena.");
                return false; 
            }

            childArenaPtr = parentArena.StartPtr + parentArena.Used;
            parentArena.Used = newUsed;
            return true;
        }

        public static bool InitialiseRaw(ref byte* arenaPtr, ref State state, nuint capacity)
        {
            nuint newUsed = state.Used + capacity;

            if (newUsed > state.Capacity)
            {
                Debug.Panic("Memory Limit Exceeded: Requested address space is too large for the remaining Memory State.");
                return false;
            }
            
            arenaPtr = state.StartPtr + state.Used;
            state.Used = newUsed;
            return true;
        }

        /// <summary>
        ///     Sets used to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Clear(ref Arena arena)
        {
            arena.Used = 0;
        }

        /// <summary>
        ///     Clears all used memory in an area to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void ClearZeroed(ref Arena arena){
            NativeMemory.Clear(arena.StartPtr, arena.Used);
            arena.Used = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static T*
    PushArrayRaw<T>(ref Arena arena, int length) where T : unmanaged
    {        
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

    public static T* PushStructRaw<T>(ref Arena arena) where T : unmanaged
    {        
        nuint sizeNeeded = (nuint)sizeof(T);
        void* ptr = arena.StartPtr + arena.Used;
        arena.Used += sizeNeeded;
        System.Diagnostics.Debug.Assert(arena.Used <= arena.Capacity, "Memory Limit Exceeded.");
        return (T*)ptr;
    }

    public static ref T PushStruct<T>(ref Arena arena) where T : unmanaged
    {        
        return ref Unsafe.AsRef<T>(PushStructRaw<T>(ref arena));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static nuint 
    Kilobytes(nuint value)
    {
        return value * 1024;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static nuint 
    Megabytes(nuint value)
    {
        return Kilobytes(value) * 1024;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static nuint 
    Gigabytes(nuint value)
    {
        return Megabytes(value) * 1024;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void 
    Free(ref State state)
    {
        System.Diagnostics.Debug.Assert(state.IsInitialised);
        NativeMemory.Free(state.StartPtr);
        state.IsInitialised = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool ToStringUTF8(byte* source, ref String destination)
    {
        int sourceLength = 0;
        while (true)
        {
            if(source[sourceLength] == '\0')
            {
                break;
            }
            sourceLength++;
        }

        int charCount = System.Text.Encoding.UTF8.GetCharCount(source, sourceLength);
        if(destination.Length < charCount)
        {
            Debug.Panic("Insufficient string length.");
            return false;
        }
        System.Text.Encoding.UTF8.GetChars(source, sourceLength, destination.Pointer, charCount);
        destination.Count = charCount;
        return true;
    }

    public static void Copy<T>(byte* source, byte* destination) where T : unmanaged
    {
        System.Span<byte> sourceBytes = new System.Span<byte>(source, sizeof(T));
        System.Span<byte> destinationBytes = new System.Span<byte>(destination, sizeof(T));
        sourceBytes.CopyTo(destinationBytes); 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Copy<T>(byte* source, byte* destination, int length) where T : unmanaged{
        NativeMemory.Copy(source, destination, (nuint)ArraySizeInBytes<T>(length));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Copy(byte* source, byte* destination, int length, int elementSizeInBytes){
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
