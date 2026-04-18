using Howl.Math;
using Howl.Vendors.MonoGame;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Test.Vendors.MonoGame;

public class Test_TextureManager
{
    public string FilePath0 = "assets/MonoGame_0.png";
    public const int TextureWidth0 = 128;
    public const int TextureHeight0 = 127;

    public string FilePath1 = "assets/MonoGame_1.png";
    public const int TextureWidth1 = 64;
    public const int TextureHeight1 = 63;

    public string FilePath2 = "assets/MonoGame_2.png";
    public const int TextureWidth2 = 32;
    public const int TextureHeight2 = 31;

    public string NilTextureFilePath = "assets/NilTexture.png";

    // NOTE:
    // A monogame app is only created here to use the Graphics device associated with it
    // in order to load textures, nothing else of it should be used.
    const int MaxTextureCount = 5;
    public static MonoGameApp MonoGameApp = new(1,1,1,1,MaxTextureCount);

    public Test_TextureManager()
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
            TextureManagerState state = new(length);
            Assert_TextureManagerState.LengthEqual(length, state);
        }
    }

    [Fact]
    public void RegisterTexture_Test()
    {
        for(int length = 0; length < 8; length++)
        {       
            TextureManagerState state = new(length);     
            
            int index = -1;
            int registeredCount = 0;
            string filePath;
            
            // successfully register textures.
            for(int i = 0; i < length-1; i++) // <-- minus one as there is a Nil value.
            {
                index = -1;
                filePath = $"foo_{i}.png";

                registeredCount++;
                Assert.True(TextureManager.RegisterTexture(state, filePath, ref index));
                Assert.Equal(registeredCount, index); // <-- add one because there is a Nil value.
                Assert.True(state.FilePathToIndex.ContainsKey(filePath));
                Assert.Equal(registeredCount, state.RegisteredTexturesCount);
            }

            // fail after capacity was hit.
            index = -1;
            filePath = "bar.png";
            Assert.Throws<Exception>(() => 
                TextureManager.RegisterTexture(state, filePath, ref index)
            );
            
            // no data should have changed after a fail case.
            Assert.Equal(-1, index);
            Assert.False(state.FilePathToIndex.ContainsKey(filePath));
            Assert.Equal(registeredCount, state.RegisteredTexturesCount);                
        }
    }

    [Fact]
    public void LoadTexture_Test()
    {
        TextureManagerState state = new(MaxTextureCount);

        int id = -1;

        id = -1;
        TextureManager.RegisterTexture(state, FilePath0, ref id);
        Assert.True(TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath0));
        Assert.NotNull(state.Textures[id]);

        id = -1;
        TextureManager.RegisterTexture(state, FilePath1, ref id);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath1);
        Assert.NotNull(state.Textures[id]);
        
        id = -1;
        TextureManager.RegisterTexture(state, FilePath2, ref id);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath2);
        Assert.NotNull(state.Textures[id]);

        Debug.Log.Suppress = true;
        
        // should return false if the file is already loaded.
        Assert.False(TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath2));

        // should return false if the file has not been registered..
        id = -1;
        Assert.False(TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, "fail case"));
        Assert.Equal(-1, id);
        
        Debug.Log.Suppress = false;
    }

    [Fact]
    public void UnloadTexture_Test()
    {
        TextureManagerState state = new(MaxTextureCount);

        int t0 = -1;
        int t1 = -1;
        int t2 = -1;

        TextureManager.RegisterTexture(state, FilePath0, ref t0);
        TextureManager.RegisterTexture(state, FilePath1, ref t1);
        TextureManager.RegisterTexture(state, FilePath2, ref t2);

        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath0);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath1);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath2);

        TextureManager.UnloadTexture(state, FilePath1);

        Assert.NotNull(state.Textures[t0]);
        Assert.Null(state.Textures[t1]);
        Assert.NotNull(state.Textures[t2]);

        TextureManager.UnloadTexture(state, FilePath0);

        Assert.Null(state.Textures[t0]);
        Assert.Null(state.Textures[t1]);
        Assert.NotNull(state.Textures[t2]);

        TextureManager.UnloadTexture(state, FilePath2);

        Assert.Null(state.Textures[t0]);
        Assert.Null(state.Textures[t1]);
        Assert.Null(state.Textures[t2]);

        Debug.Log.Suppress = true;

        // should return false if the file has already been unloaded.
        Assert.False(TextureManager.UnloadTexture(state, FilePath2));

        // should return false if the file has not been registered..
        Assert.False(TextureManager.UnloadTexture(state, "fail case"));

        Debug.Log.Suppress = false;
    }

    [Fact]
    public void GetTextureDimensions_Test()
    {
        TextureManagerState state = new(MaxTextureCount);

        int t0 = -1;
        int t1 = -1;
        int t2 = -1;

        int width = -1;
        int height = -1;

        TextureManager.RegisterTexture(state, FilePath0, ref t0);
        TextureManager.RegisterTexture(state, FilePath1, ref t1);
        TextureManager.RegisterTexture(state, FilePath2, ref t2);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath0);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath1);
        TextureManager.LoadTexture(state, MonoGameApp.GraphicsDevice, FilePath2);

        Assert.True(TextureManager.GetTextureDimensions(state, t0, ref width, ref height));
        Assert.Equal(TextureWidth0, width);
        Assert.Equal(TextureHeight0, height);

        Assert.True(TextureManager.GetTextureDimensions(state, t1, ref width, ref height));
        Assert.Equal(TextureWidth1, width);
        Assert.Equal(TextureHeight1, height);

        Assert.True(TextureManager.GetTextureDimensions(state, t2, ref width, ref height));
        Assert.Equal(TextureWidth2, width);
        Assert.Equal(TextureHeight2, height);

        Debug.Log.Suppress = true;
        Assert.False(TextureManager.GetTextureDimensions(state, 4, ref width, ref height));
        Debug.Log.Suppress = false;
    }

    [Fact]
    public void LoadNilTexture_Test()
    {
        TextureManagerState state = new(MaxTextureCount);
        
        Assert.Null(state.Textures[0]);
        TextureManager.LoadNilTexture(state, MonoGameApp, NilTextureFilePath);
        Assert.False(state.FilePathToIndex.ContainsKey(NilTextureFilePath));
        Assert.NotNull(state.Textures[0]);
    }

    [Fact]
    public void Dispose_Test()
    {
        TextureManagerState state = new(MaxTextureCount);
        for(int i = 0; i < MaxTextureCount-1; i++)
        {
            int id = -1;
            TextureManager.RegisterTexture(state, $"{i}", ref id);
        }

        TextureManagerState.Dispose(state);
        Assert_TextureManagerState.Disposed(state);
    }
}