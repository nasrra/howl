using System.Runtime.CompilerServices;

namespace Howl.DataStructures;

public unsafe struct HArray<T> where T : unmanaged
{
    public T* Pointer;
    public int Length;

    public static void Intialise(ref HArray<T> array, T* pointer, int length)
    {
        array.Pointer = pointer;
        array.Length = length;
    }

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < Length, $"Index: '{index}' is Out Of Bounds; Array Length: '{Length}' .");
            return ref Pointer[index];
        } 
    }
}