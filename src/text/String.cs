using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using N_Howl.N_Collections;
using N_Howl.N_Ecs;
using N_Howl.N_Memory;

namespace Howl.Text;

public unsafe struct String
{
    public char* Pointer;
    public int Length;
    public int Count;
    bool IsInitialised;

    public ref char this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < Length, $"Index: '{index}' is Out Of Bounds; String Length: '{Length}' .");
            return ref Pointer[index];
        }
    }

    public static bool Initialise(ref String str, ref MemoryArena arena, int length)
    {
        if (str.IsInitialised)
        {
            Debug.Panic("Already Initialised");
            return false;
        }
        str.Length = length;
        str.Pointer = Memory.PushArray<char>(ref arena, length);
        str.IsInitialised = true;
        return true;
    }

    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>This function is intended for a howl string to point to a const System.String.</para>
    ///    <para>If not, ensure that the lifetime of the System.String isnt completed before this.</para>
    /// </remarks>
    public static bool Initialise(ref String destination, string source)
    {
        if (destination.IsInitialised)
        {
            Debug.Panic("Already Initialised");
            return false;
        }

        int length = source.Length;
        destination.Length = length;
        destination.Count = length;

        fixed (char* p = source)
        {
            destination.Pointer = p;
        }

        destination.IsInitialised = true;
        return true;
    }

    public static bool Initialise(ref String destination, char* ptr, int count, int length)
    {
        if (destination.IsInitialised)
        {
            Debug.Panic("Already Initialised");
            return false;
        }

        destination.Length = length;
        destination.Count = count;
        destination.Pointer = ptr;       
        destination.IsInitialised = true;
        return true;
    }

    /******************
    
        Append Logic
    
    *******************/

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append(ref String destination, char source)
    {
        int newCount = destination.Count + 1;
        if(newCount > destination.Length)
        {
            System.Diagnostics.Debug.Assert(false, $"String length '{destination.Length}' exceeded, cannot append '{source}' to string '{new System.Span<char>(destination.Pointer, destination.Count)}'");
            return false;
        }
        destination.Pointer[destination.Count] = source;
        destination.Count = newCount;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append(ref String destination, string source)
    {
        for(int i = 0; i < source.Length; i++)
        {
            if(Append(ref destination, source[i])==false)
            {
                return false;    
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendLine(ref String destination, string source)
    {
        if(Append(ref destination, source) == false)
        {
            return false;
        }

        return Append(ref destination, '\n');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append(ref String destination, String source)
    {
        for(int i = 0; i < source.Count; i++)
        {
            if(Append(ref destination, source[i])==false)
            {
                return false;    
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendLine(ref String destination, String source)
    {
        if(Append(ref destination, source) == false)
        {
            return false;
        }

        return Append(ref destination, '\n');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append(ref String destination, uint source)
    {
        int count = 0;
        uint value = source;
        while(value > 0)
        {
            value /= 10;
            count++;
        }

        value = source;

        for(int i = 0; i < count; i++)
        {
            uint div = 1;
            for(int j = i+1; j < count; j++)
            {
                div *= 10;
            }
            
            uint write = value / div;

            if(Append(ref destination, (char)('0' + write)) == false)
            {
                return false;
            }
            value -= write * div;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendLine(ref String destination, uint source)
    {
        if (Append(ref destination, source) == false)
        {
            return false;
        }

        return Append(ref destination, source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Append(ref String destination, int source)
    {
        if (source < 0)
        {
            if(Append(ref destination, '-')==false)
            {
                return false;
            }
        }

        Append(ref destination, (uint)source);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AppendLine(ref String destination, int source)
    {
        if(Append(ref destination, source) == false)
        {
            return false;
        }

        return Append(ref destination, '\n');
    }

    public static void Append(ref String destination, double source)
    {
        System.Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written);        
        destination.Count+=written;
    }

    public static void Append(ref String destination, double source, string format)
    {
        System.Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written, format);        
        destination.Count+=written;        
    }

    public static void Append(ref String destination, float source)
    {
        System.Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written);        
        destination.Count+=written;        
    }

    public static void Append(ref String destination, float source, string format)
    {
        System.Span<char> dest = GetInvalidChars(destination);
        source.TryFormat(dest, out int written, format);        
        destination.Count+=written;                
    }

    /******************
    
        Clear logic
    
    *******************/

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(ref String str)
    {
        str.Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearZeroed(ref String str)
    {
        for(int i = 0; i < str.Length; i++)
        {
            str.Pointer[i] = '\0';
        }

        str.Count = 0;
    }

    /******************
    
        C# System logic.
    
    *******************/

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static string ToSystemString(String str)
    {
        return new string(str.Pointer, 0, str.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetByteCountUTF8(String str)
    {
        return GetByteCountUTF8(str.Pointer, str.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetByteCountUTF8(char* pString, int stringLength)
    {
        return Encoding.UTF8.GetByteCount(pString, stringLength);
    }

    /// <returns>the amount of bytes written to the destination buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetBytesUTF8(String source, ref Buffer<byte> destination)
    {
        int byteCount = Encoding.UTF8.GetByteCount(source.Pointer, source.Count);
        
        Debug.Assert(byteCount <= destination.Length, "Buffer does not have enough space!");

        Encoding.UTF8.GetBytes(source.Pointer, source.Count, destination.Pointer, byteCount);
        destination.Count = byteCount;

        return byteCount;
    }

    /// <returns>the amount of bytes written to the destination buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetBytesUTF8(string source, ref Buffer<byte> destination)
    {
        int byteCount = Encoding.UTF8.GetByteCount(source);
        
        Debug.Assert(byteCount < destination.Length, "Buffer does not have enough space!");

        fixed(char* ptr = source)
        {
            Encoding.UTF8.GetBytes(ptr, source.Length, destination.Pointer, byteCount);
        }

        destination.Count = byteCount;
        return byteCount;        
    }

    public static int GetBytesUTF8(String source, byte* destination)
    {
        return GetBytesUTF8(source.Pointer, source.Length, destination);
    }

    public static int GetBytesUTF8(char* pSource, int sourceLength, byte* pDestination)
    {        
        int byteCount = Encoding.UTF8.GetByteCount(pSource, sourceLength);
        Encoding.UTF8.GetBytes(pSource, sourceLength, pDestination, byteCount);
        return byteCount;        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.ReadOnlySpan<char> AsReadOnlySpan(String str)
    {
        return new System.ReadOnlySpan<char>(str.Pointer, str.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.Span<char> AsSpan(String str)
    {
        return new System.Span<char>(str.Pointer, str.Count);
    }

    /// <summary>
    ///     Gets a reference to the characters after the <c>Count</c> index of a HString's <c>Buffer</c>.
    /// </summary>
    /// <param name="str">the instance to get the invalid characters from.</param>
    /// <returns>a span reference to the invalid characters.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static System.Span<char> GetInvalidChars(String str)
    {
        return new System.Span<char>(str.Pointer + str.Count, str.Length - str.Count);
    }

    /******************
    
        Eqaul logic.
    
    *******************/

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Equals(String a, String b)
    {
        if (a.Count != b.Count)
            return false;

        char* ap = a.Pointer;
        char* bp = b.Pointer;

        for (int i = 0; i < a.Count; i++)
        {
            if (ap[i] != bp[i])
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Equals(String a, string b)
    {
        if (a.Count != b.Length)
            return false;

        char* ap = a.Pointer;

        for (int i = 0; i < a.Count; i++)
        {
            if (ap[i] != b[i])
                return false;
        }

        return true;
    }

    /******************
    
        Arena
    
    *******************/

    public struct Allocator
    {
        public Array<SubAllocator> SubAllocators;
        public bool IsInitialised;
        String FallbackString;

        public static bool Initialise(ref Allocator allocator, ref MemoryArena arena, string fallbackString, int maxStringLength)
        {
            if (allocator.IsInitialised)
            {
                Debug.Panic("Already Initialised.");
                return false;
            }

            N_Howl.N_Collections.Collections.Init(ref allocator.SubAllocators, ref arena, maxStringLength + 1);
            String.Initialise(ref allocator.FallbackString, ref arena, fallbackString.Length);
            Append(ref allocator.FallbackString, fallbackString);
            allocator.IsInitialised = true;
            return true;
        }

        public static bool InitialiseSubAllocator(ref Allocator allocator, ref MemoryArena arena, int subAllocatorStringLength, int subAllocatorMaxStrings)
        {
            if (allocator.SubAllocators[subAllocatorStringLength].IsInitialised)
            {
                Debug.Panic("Already Initialised.");
                return false;
            }            
            SubAllocator.Initialise(ref allocator.SubAllocators[subAllocatorStringLength], ref arena, subAllocatorStringLength, subAllocatorMaxStrings);
            return true;
        }

        public static bool Allocate(ref Allocator allocator, int stringLength, ref StringId stringId)
        {
            if (allocator.SubAllocators[stringLength].IsInitialised == false)
            {
                Debug.Panic("Sub Allocator not initialised.");
                return false;
            }

            GenId genId = default;
            if(SubAllocator.Allocate(ref allocator.SubAllocators[stringLength], ref genId)){
                StringId.Initialise(ref stringId, genId, stringLength);
                return true;
            } 

            return false;
        }

        public static bool Deallocate(ref Allocator allocator, StringId stringId)
        {
            if (allocator.SubAllocators[stringId.StringLength].IsInitialised == false)
            {
                Debug.Panic("Sub Allocator not initialised.");
                return false;
            }
            return SubAllocator.Deallocate(ref allocator.SubAllocators[stringId.StringLength], stringId.GenId); 
        }

        public static ref String GetString(ref Allocator allocator, StringId stringId, scoped out bool success)
        {
            ref SubAllocator sub = ref allocator.SubAllocators[stringId.StringLength];
            if (sub.IsInitialised == false)
            {
                success = false;
                return ref allocator.FallbackString;
            }

            ref String str = ref SubAllocator.GetString(ref sub, stringId.GenId, out success);

            if(success == false)
            {
                return ref allocator.FallbackString;
            }
#pragma warning disable // <-- disable warning as C# is dogshit and thinks this isnt okay; even though it is. 
            return ref str; 
#pragma warning restore
        }

        public struct SubAllocator
        {
            public GenIdAllocator GenIdAllocator;
            public MemoryArena CharArena;
            public ComponentArray<String> Strings;
            public bool IsInitialised;

            public static bool Initialise(ref SubAllocator allocator, ref MemoryArena arena, int stringLength, int maxStrings)
            {
                if (allocator.IsInitialised)
                {
                    return false;
                }

                // acquire space for necessary chars.
                nuint requiredCharSpaceInBytes = (nuint)(maxStrings * stringLength * sizeof(char));
                Memory.Init(ref allocator.CharArena, ref arena, requiredCharSpaceInBytes);  

                // initialise string and gen ids.                
                GenIdAllocator.Initialise(ref allocator.GenIdAllocator, ref arena, maxStrings);
                N_Howl.N_Collections.Collections.Init(ref allocator.Strings, ref arena, maxStrings);

                // initialise all strings in the chars arena.
                for(int i = 0; i < maxStrings; i++)
                {
                    String.Initialise(ref allocator.Strings.Sparse[i], ref allocator.CharArena, stringLength);
                }

                allocator.IsInitialised = true;
                return true;
            }

            public static bool Allocate(ref SubAllocator allocator, ref GenId genId)
            {
                if(GenIdAllocator.Allocate(ref allocator.GenIdAllocator, ref genId)){
                    int index = GenId.GetIndex(genId);
                    N_Howl.N_Collections.Collections.GetDataUnsafe(allocator.Strings, index).Count = 0;
                    return true;
                }
                return false;
            }

            public static bool Deallocate(ref SubAllocator allocator, GenId genId)
            {
                return GenIdAllocator.Deallocate(ref allocator.GenIdAllocator, genId);                
            }

            /// <remarks>
            ///    <para>Remarks:</para>
            ///    <para>Returns the <c>Nil</c> string in the case that the gen id is stale.</para>
            /// </remarks>
            public static ref String GetString(ref SubAllocator allocator, GenId genId, out bool success)
            {
                if(GenIdAllocator.IsInvalidId(allocator.GenIdAllocator, genId))
                {
                    success = false;
                    return ref allocator.Strings.Sparse[0]; // explicitly get the nil.
                }
                success = true;
                int index = GenId.GetIndex(genId);
                return ref N_Howl.N_Collections.Collections.GetDataUnsafe(allocator.Strings, index);
            }
        }
    }

}

public struct StringId
{
    public GenId GenId;
    public int StringLength;

    public static void Initialise(ref StringId stringId, GenId stringGenId, int StringLength)
    {
        stringId.GenId = stringGenId;
        stringId.StringLength = StringLength;
    }
}    
