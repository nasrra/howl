using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl.DataStructures;
using Howl.Unmanaged.Collections;

namespace Howl.IO;

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

    public static bool Write(string filePath, Buffer<byte> source, ModeFlag mode)
    {
        String str = default;
        String.Initialise(ref str, filePath);
        return Write(str, source.Pointer, source.Count, mode);
    }

    public static bool Write(String filePath, Buffer<byte> source, ModeFlag mode)
    {
        return Write(filePath, source.Pointer, source.Count, mode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool Write(String filePath, byte* source, long sourceLength, ModeFlag mode)
    {
        {   // Platform Dispatch.
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Debug.Panic("Unsupported Platform!");
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Unix.File.OpenFlags openFlags = ToUnixFlag(mode) | ToUnixFlag(AccessFlag.WriteOnly);
                return Unix.File.Write(filePath, source, sourceLength, openFlags);
            }
            else
            {
                Debug.Panic("Unsupported Platform!");            
            }
        }

        return false;
    }

    public static bool Read(string filePath, ref Buffer<byte> destination)
    {
        String str = default;
        String.Initialise(ref str, filePath);
        return Read(str, ref destination);
    }

    public static bool Read(String filePath, ref Buffer<byte> destination)
    {
        long totalBytesRead = 0;
        if(Read(filePath, destination.Pointer, destination.Length, ref totalBytesRead))
        {
            destination.Count = (int)totalBytesRead;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Read(String filePath, byte* destination, long destinationLength, ref long totalBytesRead)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.Panic("Unsupported Platform!");
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Unix.File.OpenFlags openFlags = ToUnixFlag(AccessFlag.ReadOnly);
            return Unix.File.Read(filePath, destination, destinationLength, openFlags, ref totalBytesRead);
        }
        else
        {
            Debug.Panic("Unsupported Platform!");            
        }
        return false;
    }

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
}