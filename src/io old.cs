using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.Text;
using Howl.Unmanaged.Collections;

namespace Howl.IO;

/**
    TODO:

    add null terminators to custom String utf8 file path conversions, as the custom string is not guaranteed to be
    null terminated and could lead to nasty headaches later.

**/

public unsafe static class File
{
    public enum AccessFlag
    {
        ReadOnly,
        WriteOnly,
        ReadWrite
    }

    public enum ModeFlag
    {
        Create,
        AtomicCreate,
        Truncate
    }

    public static bool Exists(string filePath)
    {
        String str = default;
        String.Initialise(ref str, filePath);
        return Exists(str);
    }

    public static bool Exists(String filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.Panic("Unsupported Platform!");
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Unix.File.Exists(filePath);
        }
        else
        {
            Debug.Panic("Unsupported Platform!");            
        }
        return false;
    }

    /******************
    
        File Writing.
    
    *******************/

    public static bool Write<T>(string filePath, Buffer<T> source, ModeFlag mode) where T : unmanaged, System.Numerics.INumber<T>
    {fixed (char* pFilePath = filePath){
        return Write(pFilePath, filePath.Length, (byte*)source.Pointer, Memory.ArraySizeInBytes<T>(source.Count), mode);
    }}

    public static bool Write<T>(String filePath, Buffer<T> source, ModeFlag mode) where T : unmanaged, System.Numerics.INumber<T>
    {
        return Write(filePath.Pointer, filePath.Length, (byte*)source.Pointer, Memory.ArraySizeInBytes<T>(source.Count), mode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Write(char* pFilePath, int filePathLength, byte* pSource, long sourceLength, ModeFlag mode)
    {
        {   // Platform Dispatch.
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Debug.Panic("Unsupported Platform!");
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Unix.File.OpenFlags openFlags = ToUnixFlag(mode) | ToUnixFlag(AccessFlag.WriteOnly);
                return Unix.File.Write(pFilePath, filePathLength, pSource, sourceLength, openFlags);
            }
            else
            {
                Debug.Panic("Unsupported Platform!");
            }
        }

        return false;
    }

    /******************
    
        File Reading.
    
    *******************/

    public static bool Read(
        String filePath, ref Memory.Arena arena
    ){
        long totalBytesReadOutput = 0; 
        bool success = Read(filePath.Pointer, filePath.Count, arena.StartPtr, (int)arena.Capacity, ref totalBytesReadOutput);
        if(success){
            arena.Capacity = (nuint)totalBytesReadOutput;
        }
        return success;
    }

    public static bool Read(
        string filePath, ref Memory.Arena arena
    ){fixed(char* pFilePath = filePath){

        long totalBytesReadOutput = 0; 
        bool success = Read(pFilePath, filePath.Length, arena.StartPtr, (int)arena.Capacity, ref totalBytesReadOutput);
        if(success){
            arena.Capacity = (nuint)totalBytesReadOutput;
        }
        return success;
    }}

    public static bool Read<T>(string filePath, ref Buffer<T> destination) where T : unmanaged 
    {fixed(char* pFilePath = filePath){
        return Read(pFilePath, filePath.Length, ref destination);            
    }}

    public static bool Read<T>(String filePath, ref Buffer<T> destination) where T : unmanaged
    {
        return Read(filePath.Pointer, filePath.Count, ref destination);
    }

    public static bool Read<T>(char* pFilePath, int filePathLength, ref Buffer<T> destination) where T : unmanaged
    {
        long totalBytesRead = 0;
        if(Read(pFilePath, filePathLength, (byte*)destination.Pointer, Memory.ArraySizeInBytes<T>(destination.Length), ref totalBytesRead))
        {
            // calculate count relative to the size of T.
            destination.Count = Memory.ArrayLengthFromBytes<T>(totalBytesRead);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Read<T>(string filePath, T* pDestination, int destinationLength, ref long totalBytesReadOutput) where T : unmanaged
    {fixed(char* pFilePath = filePath)
        return Read(pFilePath, filePath.Length, (byte*)pDestination, Memory.ArraySizeInBytes<T>(destinationLength), ref totalBytesReadOutput);
    }

    public static bool Read(char* pFilePath, int filePathLength, byte* destination, int destinationLength, ref long totalBytesReadOutput)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.Panic("Unsupported Platform!");
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Unix.File.OpenFlags openFlags = ToUnixFlag(AccessFlag.ReadOnly);
            return Unix.File.Read(pFilePath, filePathLength, destination, destinationLength, openFlags, ref totalBytesReadOutput);
        }
        else
        {
            Debug.Panic("Unsupported Platform!");            
        }
        return false;
    }

    /******************
    
        Utilities.
    
    *******************/

    public static Unix.File.OpenFlags ToUnixFlag(AccessFlag flag)
    {
        switch (flag)
        {
            case AccessFlag.ReadOnly:
                return Unix.File.OpenFlags.ReadOnly;
            case AccessFlag.WriteOnly:
                return Unix.File.OpenFlags.WriteOnly;
            case AccessFlag.ReadWrite:
                return Unix.File.OpenFlags.ReadWrite;
            default:
                Debug.Panic("Unknown Access Flag.");
                return Unix.File.OpenFlags.ReadOnly;
        }
    }

    public static Unix.File.OpenFlags ToUnixFlag(ModeFlag flag)
    {
        switch (flag)
        {
            case ModeFlag.Create:
                return Unix.File.OpenFlags.Create;
            case ModeFlag.AtomicCreate:
                return Unix.File.OpenFlags.AtomicCreate;
            case ModeFlag.Truncate:
                return Unix.File.OpenFlags.Truncate;
            default:
                Debug.Panic("Unknown Mode Flag.");
                return Unix.File.OpenFlags.Create;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool ToStringUTF8(Buffer<byte> source, ref String destination)
    {        
        int charCount = System.Text.Encoding.UTF8.GetCharCount(source.Pointer, source.Count);
        if(destination.Length < charCount)
        {
            Debug.Panic("Insufficient string length.");
            return false;
        }
        System.Text.Encoding.UTF8.GetChars(source.Pointer, source.Count, destination.Pointer, charCount);
        destination.Count = charCount;
        return true;
    }

}