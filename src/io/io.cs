using System.Runtime.InteropServices;
using Howl.Text;
using N_Howl.N_Collections;

namespace N_Howl.N_IO;
public unsafe static class IO{

    private const string StbiLibName = "stb_image";
    
    [DllImport(StbiLibName, EntryPoint = "stbi_load", ExactSpelling = true)]
    private static extern char* StbiLoad(byte* utf8FilePath, int* width, int* height, int* channels, int desiredChannels);

    [DllImport(StbiLibName, EntryPoint = "stbi_image_free", ExactSpelling = true)]
    private static extern void StbiImageFree(void* retval);

    public static Image LoadImage(string filePath, ref bool isValidOutput){
    fixed(char* pStr = filePath){
        byte* utf8Path = stackalloc byte [filePath.Length];
        String.GetBytesUTF8(pStr, filePath.Length, utf8Path);
        return LoadImage(utf8Path, ref isValidOutput);
    }}

    public static Image LoadImage(String str, ref bool isValidOutput){
        byte* utf8Path = stackalloc byte[str.Count+1];
        int written = String.GetBytesUTF8(str.Pointer, str.Count, utf8Path);
        // add null terminator.
        utf8Path[written] = (byte)'\0';
        return LoadImage(utf8Path, ref isValidOutput);   
    }

    public static Image LoadImage(byte* utf8FilePath, ref bool isValidOutput){
        
        Image image = default;
        int comp = 0;
        
        // 4 channels for RGBA always being output.
        int desiredChannels = 4;

        byte* ptr = (byte*)StbiLoad(utf8FilePath, &image.Width, &image.Height, &comp, desiredChannels);
    
        if(ptr == null){
            isValidOutput = false;
            return default;
        }

        int length = desiredChannels * image.Width * image.Height;
        Collections.Init(ref image.Pixels, ptr, length);
        isValidOutput = true;
        return image;
    }

    public static bool FreeImage(ref Image image){
        if(image.Pixels.Pointer == null){
            return false;
        }

        StbiImageFree(image.Pixels.Pointer);
        // zero out image once free.
        image = default;
        return true;
    }
}