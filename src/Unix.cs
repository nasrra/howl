using System.Runtime.InteropServices;
using Howl.Text;
using N_Howl.N_Collections;

namespace Howl.Unix;

public static class Constants
{
    // Note: .NET Core 3.0+ has an internal mapped in the execution engine that
    // swaps the 'libc' for 'libSystem' on macos.
    public const string LibName = "libc";
}

public unsafe static class File
{

    /******************
    
        imports
    
    *******************/

    // Note: Unix reads the file path bytes until it hits a null terminator (\0) or zero byte (0x0000)
    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int open(byte* filePath, int flags);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int open(byte* filePath, int flags, int accessPermissions);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern nint read(int fileDescriptor, void* destination, nuint fileSizeInBytes);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern nint write(int fileDescriptor, void* source, nuint sourceSizeInBytes);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int close(int fileDescriptor);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int fstat(int fileDescriptor, out LinuxStat stat);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int fstat(int fileDescriptor, out MacosStat stat);

    [DllImport(Constants.LibName, SetLastError = true)]
    public static extern int access(byte* filePath, int mode);

    public struct LinuxStat
    {
        public ulong st_dev; // id of device containing the file.
        public ulong st_ino; // Inode number.
        public ulong st_nlink; // number of hard links.
        public uint st_mode; // file protection mode.
        public uint st_uid; // user id of owner.
        public uint st_gid; // group id of owner.
        public uint __pad0; // padding for 64 but alignment.
        public uint st_rdev; // device id (if special file).
        public long st_size; // total size in bytes.
        public long st_blksize; // block size for filesystem i/o.
        public long st_blocks; // number of 512B blocks allocated.

        // Timestamps.
        public long st_atime_sec; // time of last access (seconds)
        public long st_atime_nsec; // time of last access (nanoseconds)
        public long st_mtime_sec; // time of last modification (seconds)
        public long st_mtime_nsec; // time of last modification (nanoseconds)
        public long st_ctime_sec; // time of last status change (seconds)
        public long st_ctime_nsec; // time of last status change (nanoseconds)

        // unused padding reserved by the linux kernel.
        public long __unused1;
        public long __unused2;
        public long __unused3;
    }

    public struct MacosStat
    {
        public int   st_dev; // id of device containing file
        public uint  st_mode; // file protection mode
        public ushort st_nlink; // number of hard links
        public ulong st_ino; // inode number
        public uint  st_uid; // user ID of owner
        public uint  st_gid; // group ID of owner
        public int   st_rdev; // device ID (if special file)

        // --- Timestamps ---
        public long  st_atime_sec; // last access time (seconds)
        public long  st_atime_nsec; // last access time (nanoseconds)
        public long  st_mtime_sec; // last modification time (seconds)
        public long  st_mtime_nsec; // last modification time (nanoseconds)
        public long  st_ctime_sec; // last status change time (seconds)
        public long  st_ctime_nsec; // last status change time (nanoseconds)
        public long  st_birthtime_sec; // file creation time (seconds) - macOS exclusive!
        public long  st_birthtime_nsec; // file creation time (nanoseconds)

        public long  st_size; // total size in bytes
        public long  st_blocks; // number of blocks allocated
        public int   st_blksize; // optimal blocksize for I/O
        public uint  st_flags; // user defined flags for file
        public uint  st_gen; // file generation number
        public int   st_lspare; // reserved
        public long __qspare0; // reserved padding
        public long __qspare1; // reserved padding
    }
  
    /******************
    
        Constants
    
    *******************/

    public const int Ok = 0; 

    /******************
    
        Definitions
    
    *******************/

    [System.Flags]
    public enum OpenFlags : int
    {
        ReadOnly = 0x0000,
        WriteOnly = 0x0001,
        ReadWrite = 0x0002,
        Create = 0x0040, // create the file if it doesnt exist.
        AtomicCreate = 0x0080, // create the file ONLY if it doesnt exist; otherwise fail.
        Truncate = 0x0200, // If the file exists, clear all file bytes upon opening.
        Append = 0x0400 // move the write pointer to the end of the  file before every write.
    }

    [System.Flags]
    public enum PermissionFlags : int
    {
        UserRead = 0x0100,
        UserWrite = 0x0080,
        GroupRead = 0x0020,
        OtherRead = 0x0004,
        Standrard = UserRead | UserWrite | GroupRead | OtherRead
    }

    /******************
    
        Procedures
    
    *******************/

    public static bool Exists(String filePath)
    {        
        // allocate file path on the stack (+1 for the required null terminator).
        int byteCount = String.GetByteCountUTF8(filePath);
        byte* utf8Path = stackalloc byte[byteCount + 1];
        String.GetBytesUTF8(filePath, utf8Path);        
        // CRUCIAL: set the null terminator.
        utf8Path[byteCount] = 0;

        return Exists(utf8Path);
    }

    public static bool Exists(
        byte* utf8Path
    )
    {
        return access(utf8Path, Ok) == 0;
    }

    public static bool Read(
        char* pFilePath, int filePathLength, byte* destination, long destinationLength, OpenFlags openFlags, ref long totalBytesReadOutput
    )
    {
        // allocate file path on the stack (+1 for the required null terminator).
        int byteCount = String.GetByteCountUTF8(pFilePath, filePathLength);
        // Crucial: +1 for the null terminator; C# zeroes stack allocs so the final byte will always be zero ('\0')
        byte* utf8Path = stackalloc byte[byteCount + 1];
        String.GetBytesUTF8(pFilePath, filePathLength, utf8Path);        
        
        return Read(utf8Path, destination, destinationLength, openFlags, ref totalBytesReadOutput);
    }

    public static bool Read(byte* utf8Path, byte* destination, long destinationLength, OpenFlags openFlags, ref long totalBytesReadOutput)
    {
        int fileDescriptor = open(utf8Path, (int)openFlags);
        if(fileDescriptor < 0)
        {
            Debug.Panic("Failed to open file.");
            return false;
        }

        try
        {
            long fileLength = GetSize(fileDescriptor);
            if(fileLength >= destinationLength)
            {
                Debug.Panic("Insufficient buffer size to load file.");
                return false;
            }

            long totalBytesRead = 0;
            while(totalBytesRead < fileLength)
            {
                nuint remaining = (nuint)(fileLength - totalBytesRead);
                nint bytesRead = read(fileDescriptor, destination + totalBytesRead, remaining);

                if(bytesRead <= 0)
                {
                    if(bytesRead < 0)
                    {
                        // a negative value means that an actual os read error occured.
                        Debug.Panic("OS Read Error!");
                        return false;
                    }
                    break; // end of file reached early.
                }
                
                totalBytesRead += bytesRead;
            }

            totalBytesReadOutput = totalBytesRead;
        }
        finally
        {
            close(fileDescriptor);
        }
        return true;
    }

    public static bool Write(char* pFilePath, int filePathLength, byte* source, long sourceLength, OpenFlags openFlags)
    {
        // allocate file path on the stack (+1 for the required null terminator).
        int byteCount = String.GetByteCountUTF8(pFilePath, filePathLength);
        // Crucial: +1 for the null terminator; C# zeroes stack allocs so the final byte will always be zero ('\0')
        byte* utf8Path = stackalloc byte[byteCount + 1];
        String.GetBytesUTF8(pFilePath, filePathLength, utf8Path);        
        
        return Write(utf8Path, source, sourceLength, openFlags);
    }

    public static bool Write(byte* utf8Path, byte* source, long sourceLength, OpenFlags openFlags)
    {
        int fileDescriptor = open(utf8Path, (int)openFlags, (int)PermissionFlags.Standrard);
        if(fileDescriptor < 0)
        {
            Debug.Panic("Failed to open file.");
            return false;
        }

        try
        {
            long totalBytesWritten = 0;

            while(totalBytesWritten < sourceLength)
            {
                nuint remaining = (nuint)(sourceLength - totalBytesWritten);
                nint bytesWritten = write(fileDescriptor, source + totalBytesWritten, remaining);

                if(bytesWritten <= 0)
                {
                    Debug.Panic("Failed to write to file!"); // disc could be full.
                }

                totalBytesWritten += bytesWritten;
            }
        }
        finally
        {
            close(fileDescriptor);   
        }

        return true;
    }

    public static long GetSize(int fileDescriptor)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if(fstat(fileDescriptor, out LinuxStat stat) == 0)
            {
                return stat.st_size;
            }
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if(fstat(fileDescriptor, out MacosStat stat) == 0)
            {
                return stat.st_size;
            }            
        }
        else
        {
            Debug.Panic("Unsupported Platform!");
        }

        return 0;
    }
}