using Howl.Vendors.MonoGame.FontStashSharp;

namespace Howl.Test.Vendors.MonoGame.FontStashSharp;

public class Test_FontManager
{
    public string FilePath0 = "assets/fonts/PlaywriteNO-Regular.ttf";
    public const int Size0 = 128;

    public string FilePath1 = "assets/fonts/Roboto-Medium.ttf";
    public const int Size1 = 64;

    public string FilePath2 = "assets/fonts/Sekuya-Regular.ttf";
    public const int Size2 = 32;

    public string NilFontFilePath = "assets/fonts/GoogleSans-Regular.ttf";
    public const int NilSize = 2;

    // NOTE:
    // A monogame app is only created here to use the Graphics device associated with it
    // in order to load textures, nothing else of it should be used.
    const int MaxFontCount = 4;

    public Test_FontManager()
    {
        // Clear default listeners that show dialogs
        System.Diagnostics.Trace.Listeners.Clear();
        // Add a listener that throws an exception on failure
        System.Diagnostics.Trace.Listeners.Add(new ThrowingTraceListener());
    }

    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            FontManagerState state = new(length);
            Assert_FontManagerState.LengthEqual(length, state);
        }
    }

    [Fact]
    public void RegisterFont_Test()
    {
        for(int length = 0; length < 8; length++)
        {       
            FontManagerState state = new(length);     
            
            int index = -1;
            int registeredCount = 0;
            string filePath;
            
            // successfully register textures.
            for(int i = 0; i < length-1; i++) // <-- minus one as there is a Nil value.
            {
                index = -1;
                filePath = $"foo_{i}.ttf";

                registeredCount++;
                Assert.True(FontManager.RegisterFont(state, filePath, ref index));
                Assert.Equal(registeredCount, index); // <-- add one because there is a Nil value.
                Assert.True(state.FilePathToIndex.ContainsKey(filePath));
                Assert.Equal(registeredCount, state.RegisteredCount);
            }

            // fail after capacity was hit.
            index = -1;
            filePath = "bar.ttf";
            Assert.Throws<Exception>(() => 
                FontManager.RegisterFont(state, filePath, ref index)
            );
            
            // no data should have changed after a fail case.
            Assert.Equal(-1, index);
            Assert.False(state.FilePathToIndex.ContainsKey(filePath));
            Assert.Equal(registeredCount, state.RegisteredCount);                
        }
    }

    [Fact]
    public void LoadFont_Test()
    {
        FontManagerState state = new(MaxFontCount);

        int id = -1;

        id = -1;
        FontManager.RegisterFont(state, FilePath0, ref id);
        Assert.True(FontManager.LoadFont(state, FilePath0, Size0));
        Assert.NotNull(state.Fonts[id]);

        id = -1;
        FontManager.RegisterFont(state, FilePath1, ref id);
        FontManager.LoadFont(state, FilePath1, Size1);
        Assert.NotNull(state.Fonts[id]);
        
        id = -1;
        FontManager.RegisterFont(state, FilePath2, ref id);
        FontManager.LoadFont(state, FilePath2, Size2);
        Assert.NotNull(state.Fonts[id]);

        Debug.Log.Suppress = true;
        
        // should return false if the file is already loaded.
        Assert.False(FontManager.LoadFont(state, FilePath2, Size2));

        // should return false if the file has not been registered..
        id = -1;
        Assert.False(FontManager.LoadFont(state, "fail case", 1));
        Assert.Equal(-1, id);
        
        Debug.Log.Suppress = false;
    }

    [Fact]
    public void UnloadFont_Test()
    {
        FontManagerState state = new(MaxFontCount);

        int f0 = -1;
        int f1 = -1;
        int f2 = -1;

        FontManager.RegisterFont(state, FilePath0, ref f0);
        FontManager.RegisterFont(state, FilePath1, ref f1);
        FontManager.RegisterFont(state, FilePath2, ref f2);

        FontManager.LoadFont(state, FilePath0, Size0);
        FontManager.LoadFont(state, FilePath1, Size1);
        FontManager.LoadFont(state, FilePath2, Size2);

        FontManager.UnloadFont(state, FilePath1);

        Assert.NotNull(state.Fonts[f0]);
        Assert.Null(state.Fonts[f1]);
        Assert.NotNull(state.Fonts[f2]);

        FontManager.UnloadFont(state, FilePath0);

        Assert.Null(state.Fonts[f0]);
        Assert.Null(state.Fonts[f1]);
        Assert.NotNull(state.Fonts[f2]);

        FontManager.UnloadFont(state, FilePath2);

        Assert.Null(state.Fonts[f0]);
        Assert.Null(state.Fonts[f1]);
        Assert.Null(state.Fonts[f2]);

        Debug.Log.Suppress = true;

        // should return false if the file has already been unloaded.
        Assert.False(FontManager.UnloadFont(state, FilePath2));

        // should return false if the file has not been registered..
        Assert.False(FontManager.UnloadFont(state, "fail case"));

        Debug.Log.Suppress = false;
    }

    [Fact]
    public void LoadNilTexture_Test()
    {
        FontManagerState state = new(MaxFontCount);
        
        Assert.Null(state.Fonts[0]);
        FontManager.LoadNilFont(state, NilFontFilePath, NilSize);
        Assert.False(state.FilePathToIndex.ContainsKey(NilFontFilePath));
        Assert.NotNull(state.Fonts[0]);
    }

    [Fact]
    public void Dispose_Test()
    {
        FontManagerState state = new(MaxFontCount);
        for(int i = 0; i < MaxFontCount-1; i++)
        {
            int id = -1;
            FontManager.RegisterFont(state, $"{i}", ref id);
        }

        FontManager.Dispose(state);
        Assert_FontManagerState.Disposed(state);
    }
}