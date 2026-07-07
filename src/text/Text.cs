
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using N_Howl.N_Collections;
using N_Howl.N_Ecs;
using N_Howl.N_Math;
using N_Howl.N_Memory;

namespace N_Howl.N_Text;
public unsafe static class Text{

/**##########################################################################################################################################
    div start: String. 
##########################################################################################################################################**/

public static bool Init(
    ref String str, ref MemoryArena arena, int length
){
    if (str.IsInitialised){
        Howl.Debug.Panic("String already Initialised");
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
public static bool Init(
    ref String dst, string src
){
fixed(char* ptr = src){ // <-- this has to be fixed otherwise C# might decide to move the string lol HHAHAHHA :))))) 
    if (dst.IsInitialised){
        Howl.Debug.Panic("String already Initialised");
        return false;
    }

    Init(ref dst, ptr, src.Length, src.Length);
    return true;
}}

/// <remarks>
///    <para>Remarks:</para>
///    <para>This function is intended for a howl string to point to a const System.String.</para>
///    <para>If not, ensure that the lifetime of the System.String isnt completed before this.</para>
/// </remarks>
public static bool Init(
    ref String dst, string src, int length
){
fixed(char* ptr = src){ // <-- this has to be fixed otherwise C# might decide to move the string lol HHAHAHHA :))))) 

    if (dst.IsInitialised){
        Howl.Debug.Panic("Already Initialised");
        return false;
    }

    Init(ref dst, ptr , Math.Clamp(src.Length, src.Length, length), length);
    return true;
}}

public static bool Init(
    ref String destination, char* ptr, int count, int length
){
    if (destination.IsInitialised){
        Howl.Debug.Panic("String already Initialised");
        return false;
    }

    destination.Length = length;
    destination.Count = count;
    destination.Pointer = ptr;       
    destination.IsInitialised = true;
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Push(
    ref String dst, char src
){
    int newCount = dst.Count + 1;
    if(newCount > dst.Length){
        Howl.Debug.LogWarning($"string length exceeded; cannot push char '{src}' to string '{ToSystemString(dst)}'");
        return false;
    }
    dst.Pointer[dst.Count] = src;
    dst.Count = newCount;
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Push(
    ref String dst, string src
){
    for(int i = 0; i < src.Length; i++){
        if(Push(ref dst, src[i])==false){
            return false;    
        }
    }
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PushLine(
    ref String dst, string src
){
    if(Push(ref dst, src) == false){
        return false;
    }

    return Push(ref dst, '\n');
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Push(
    ref String dst, String src
){
    for(int i = 0; i < src.Count; i++){
        if(Push(ref dst, src[i])==false){
            return false;    
        }
    }
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PushLine(
    ref String dst, String src
){
    if(Push(ref dst, src) == false){
        return false;
    }

    return Push(ref dst, '\n');
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Push(
    ref String dst, uint src
){
    int count = 0;
    uint value = src;
    while(value > 0){
        value /= 10;
        count++;
    }

    value = src;

    for(int i = 0; i < count; i++){
        uint div = 1;
        for(int j = i+1; j < count; j++){
            div *= 10;
        }
        
        uint write = value / div;

        if(Push(ref dst, (char)('0' + write)) == false){
            return false;
        }
        value -= write * div;
    }

    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PushLine(
    ref String dst, uint src
){
    if (Push(ref dst, src) == false){
        return false;
    }

    return Push(ref dst, src);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Push(
    ref String dst, int src
){
    if (src < 0){
        if(Push(ref dst, '-')==false){
            return false;
        }
    }

    Push(ref dst, (uint)src);
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PushLine(
    ref String dst, int src
){
    if(Push(ref dst, src) == false){
        return false;
    }

    return Push(ref dst, '\n');
}

public static void Push(
    ref String dst, double src
){
    System.Span<char> dest = GetInvalidChars(dst);
    src.TryFormat(dest, out int written);        
    dst.Count+=written;
}

public static void Push(
    ref String dst, double src, string format
){
    System.Span<char> dest = GetInvalidChars(dst);
    src.TryFormat(dest, out int written, format);        
    dst.Count+=written;        
}

public static void Push(
    ref String dst, float src
){
    System.Span<char> dest = GetInvalidChars(dst);
    src.TryFormat(dest, out int written);        
    dst.Count+=written;        
}

public static void Push(
    ref String dst, float src, string format
){
    System.Span<char> dest = GetInvalidChars(dst);
    src.TryFormat(dest, out int written, format);        
    dst.Count+=written;                
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Pop(
    ref String dst
){
    dst.Count--;
    if(dst.Count < 0){
        Howl.Debug.LogWarning($"attempted to {nameof(Pop)}() a zero count String '{(nuint)dst.Pointer}'.");
    }
    dst.Count = Math.Clamp(dst.Count, 0, dst.Length);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref String str
){
    str.Count = 0;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearZeroed(
    ref String str
){
    NativeMemory.Clear(str.Pointer, (nuint)(sizeof(char)*str.Length));
    str.Count = 0;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static string ToSystemString(
    String str
){
    return new string(str.Pointer, 0, str.Count);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int GetByteCountUTF8(
    String str
){
    return GetByteCountUTF8(str.Pointer, str.Count);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int GetByteCountUTF8(
    char* pString, int stringLength
){
    return System.Text.Encoding.UTF8.GetByteCount(pString, stringLength);
}

/// <returns>the amount of bytes written to the destination buffer.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int GetBytesUTF8(
    String src, ref Buffer<byte> dst
){
    int byteCount = System.Text.Encoding.UTF8.GetByteCount(src.Pointer, src.Count);
    
    Howl.Debug.Assert(byteCount <= dst.Length, "Buffer does not have enough space!");

    System.Text.Encoding.UTF8.GetBytes(src.Pointer, src.Count, dst.Pointer, byteCount);
    dst.Count = byteCount;

    return byteCount;
}

/// <returns>the amount of bytes written to the destination buffer.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int GetBytesUTF8(
    string src, ref Buffer<byte> dst
){
fixed(char* ptr = src){

    int byteCount = System.Text.Encoding.UTF8.GetByteCount(src);
    
    Howl.Debug.Assert(byteCount < dst.Length, "Buffer does not have enough space!");

        System.Text.Encoding.UTF8.GetBytes(ptr, src.Length, dst.Pointer, byteCount);

    dst.Count = byteCount;
    return byteCount;        
}}

public static int GetBytesUTF8(
    String src, byte* dst
){
    return GetBytesUTF8(src.Pointer, src.Length, dst);
}

public static int GetBytesUTF8(
    char* src, int srcLength, byte* dst
){        
    int byteCount = System.Text.Encoding.UTF8.GetByteCount(src, srcLength);
    System.Text.Encoding.UTF8.GetBytes(src, srcLength, dst, byteCount);
    return byteCount;        
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.ReadOnlySpan<char> AsReadOnlySpan(
    String str
){
    return new System.ReadOnlySpan<char>(str.Pointer, str.Count);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<char> AsSpan(
    String str
){
    return new System.Span<char>(str.Pointer, str.Count);
}

/// <summary>
///     Gets a reference to the characters after the <c>Count</c> index of a HString's <c>Buffer</c>.
/// </summary>
/// <param name="str">the instance to get the invalid characters from.</param>
/// <returns>a span reference to the invalid characters.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Span<char> GetInvalidChars(
    String str
){
    return new System.Span<char>(str.Pointer + str.Count, str.Length - str.Count);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool Equals(
    String a, String b
){
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
public static bool Equals(
    String a, string b
){
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

/**##########################################################################################################################################
    div end: String. 
##########################################################################################################################################**/

/**##########################################################################################################################################
    div start: StringAllocator. 
##########################################################################################################################################**/

public static bool Init(
    ref StringAllocator allocator, ref MemoryArena arena, string fallbackString, int maxStringLength
){
    if (allocator.IsInitialised){
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }

    Collections.Init(ref allocator.SubAllocators, ref arena, maxStringLength + 1);
    Init(ref allocator.FallbackString, ref arena, fallbackString.Length);
    Push(ref allocator.FallbackString, fallbackString);
    allocator.IsInitialised = true;
    return true;
}

public static bool InitSubAllocator(
    ref StringAllocator allocator, ref MemoryArena arena, int subAllocatorStringLength, int subAllocatorMaxStrings
){
    if (allocator.SubAllocators[subAllocatorStringLength].IsInitialised){
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }            
    Init(ref allocator.SubAllocators[subAllocatorStringLength], ref arena, subAllocatorStringLength, subAllocatorMaxStrings);
    return true;
}

public static bool AllocString(
    ref StringAllocator allocator, int stringLength, ref StringId stringId
){
    if (allocator.SubAllocators[stringLength].IsInitialised == false){
        Howl.Debug.Panic("Sub Allocator not initialised.");
        return false;
    }

    GenId genId = default;
    if(AllocString(ref allocator.SubAllocators[stringLength], ref genId)){
        stringId.GenId = genId;
        stringId.StringLength = stringLength;
        return true;
    } 

    return false;
}

public static bool DeallocString(
    ref StringAllocator allocator, StringId stringId
){
    if (allocator.SubAllocators[stringId.StringLength].IsInitialised == false){
        Howl.Debug.Panic("Sub Allocator not initialised.");
        return false;
    }
    return DeallocString(ref allocator.SubAllocators[stringId.StringLength], stringId.GenId); 
}

public static ref String GetString(ref StringAllocator allocator, StringId stringId, scoped out bool success)
{
    ref StringSubAllocator sub = ref allocator.SubAllocators[stringId.StringLength];
    if (sub.IsInitialised == false)
    {
        success = false;
        return ref allocator.FallbackString;
    }

    ref String str = ref GetString(ref sub, stringId.GenId, out success);

    if(success == false)
    {
        return ref allocator.FallbackString;
    }
#pragma warning disable // <-- disable warning as C# is dogshit and thinks this isnt okay; even though it is. 
    return ref str; 
#pragma warning restore
}

/**##########################################################################################################################################
    div end: StringAllocator. 
##########################################################################################################################################**/

/**##########################################################################################################################################
    div start: StringSubAllocator 
##########################################################################################################################################**/

public static bool Init(
    ref StringSubAllocator allocator, ref MemoryArena arena, int stringLength, int maxStrings
){
    if (allocator.IsInitialised){
        return false;
    }

    // acquire space for necessary chars.
    nuint requiredCharSpaceInBytes = (nuint)(maxStrings * stringLength * sizeof(char));
    Memory.Init(ref allocator.CharArena, ref arena, requiredCharSpaceInBytes);  

    // initialise string and gen ids.                
    GenIdAllocator.Initialise(ref allocator.GenIdAllocator, ref arena, maxStrings);
    Collections.Init(ref allocator.Strings, ref arena, maxStrings);

    // initialise all strings in the chars arena.
    for(int i = 0; i < maxStrings; i++){
        Init(ref allocator.Strings.Sparse[i], ref allocator.CharArena, stringLength);
    }

    allocator.IsInitialised = true;
    return true;
}

public static bool AllocString(
    ref StringSubAllocator allocator, ref GenId genId
){
    if(GenIdAllocator.Allocate(ref allocator.GenIdAllocator, ref genId)){
        int index = GenId.GetIndex(genId);
        Collections.GetDataUnsafe(allocator.Strings, index).Count = 0;
        return true;
    }
    return false;
}

public static bool DeallocString(
    ref StringSubAllocator allocator, GenId genId
){
    return GenIdAllocator.Deallocate(ref allocator.GenIdAllocator, genId);                
}

/// <remarks>
///    <para>Remarks:</para>
///    <para>Returns the <c>Nil</c> string in the case that the gen id is stale.</para>
/// </remarks>
public static ref String GetString(
    ref StringSubAllocator allocator, GenId genId, out bool success
){
    if(GenIdAllocator.IsInvalidId(allocator.GenIdAllocator, genId))
    {
        success = false;
        return ref allocator.Strings.Sparse[0]; // explicitly get the nil.
    }
    success = true;
    int index = GenId.GetIndex(genId);
    return ref Collections.GetDataUnsafe(allocator.Strings, index);
}

/**##########################################################################################################################################
    div end: StringSubAllocator. 
##########################################################################################################################################**/


/// <summary>
///     Gets the amount of characters required to write a numerical value as a string.
/// </summary>
/// <remarks>
///    <para>Remarks:</para>
///    <para></para>
/// </remarks>
public static int CalculateCharacterCount(
    int value
){
    int count = 0;
    while(value > 0)
    {
        value /= 10;
        count++;
    }
    return count;
}

}