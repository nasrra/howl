using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Howl.Unmanaged.Collections;

namespace Howl.DataStructures;

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

    public static bool Initialise(ref Memory.Arena arena, ref String str, int length)
    {
        if (str.IsInitialised)
        {
            Debug.Panic("Already Initialised");
            return false;
        }
        str.Length = length;
        str.Pointer = Memory.PushArrayRaw<char>(ref arena, length);
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
        return Encoding.UTF8.GetByteCount(str.Pointer, str.Count);
    }

    /// <returns>the amount of bytes written to the destination array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static int GetBytesUTF8(String source, ref Buffer<byte> destination)
    {
        int byteCount = Encoding.UTF8.GetByteCount(source.Pointer, source.Count);
        
        Debug.Assert(byteCount < destination.Length, "Array does not have enough space!");

        Encoding.UTF8.GetBytes(source.Pointer, source.Count, destination.Pointer, byteCount);
        destination.Count = byteCount;

        return byteCount;
    }

    public static int GetBytesUTF8(String source, byte* destination)
    {
        int byteCount = Encoding.UTF8.GetByteCount(source.Pointer, source.Count);
        Encoding.UTF8.GetBytes(source.Pointer, source.Count, destination, byteCount);
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
}