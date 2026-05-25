using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.DataStructures;

namespace Howl;

public unsafe static class Memory
{
    public struct State
    {
        public byte* Buffer;
        public nuint Capacity; 
        public nuint Offset;
        public bool IsInitialised;
    }

    public struct Arena
    {
        public byte* StartPtr;
        public nuint Used;
        public nuint Capacity;
        public bool IsInitialised;
    }

    public static void 
    InitialiseState(ref State state, nuint capacity)
    {
        state.IsInitialised = true;
        state.Capacity = capacity;
        state.Buffer = (byte*)NativeMemory.Alloc(capacity);
    }

    public static void 
    IntialiseArena(ref State state, ref Arena arena, nuint capacity)
    {
        state.Offset += capacity;
        System.Diagnostics.Debug.Assert(state.Offset <= state.Capacity, "Memory Limit Exceeded.");
        arena.StartPtr = state.Buffer + state.Offset;
        arena.Capacity = capacity;
    }

    public static HArray<T>
    PushArray<T>(ref Arena arena, int length) where T : unmanaged
    {
        /**
        NOTE: gemini wrote this, dont know why it is needed so i commented it out,
        honestly doesnt make any sense lol,        

        Ensure alignment (Odin and Jai do this automatically under the hood)
             nuint alignment = (nuint)sizeof(nuint);
             _offset = (_offset + alignment - 1) & ~(alignment - 1);
        **/

        nuint sizeNeeded = (nuint)length * (nuint)sizeof(T);
        arena.Used += sizeNeeded;
        System.Diagnostics.Debug.Assert(arena.Used <= arena.Capacity, "Memory Limit Exceeded.");
        T* ptr = (T*)(arena.StartPtr + arena.Used);
        
        HArray<T> array = default;
        HArray<T>.Intialise(ref array, ptr, length);
        return array;
    }

    public static ref T
    PushStruct<T>(ref Arena arena) where T : unmanaged
    {
        nuint sizeNeeded = (nuint)sizeof(T);
        arena.Used += sizeNeeded;
        System.Diagnostics.Debug.Assert(arena.Used <= arena.Capacity, "Memory Limit Exceeded.");
        void* ptr = arena.StartPtr + arena.Used;
        return ref Unsafe.AsRef<T>(ptr);
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

    public static void 
    Free(ref State state)
    {
        System.Diagnostics.Debug.Assert(state.IsInitialised);
        NativeMemory.Free(state.Buffer);
        state.IsInitialised = false;
    }
}
