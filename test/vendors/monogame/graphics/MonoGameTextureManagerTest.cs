// using Howl.ECS;
// using Howl.Generic;
// using Howl.Graphics;
// using Howl.Vendors.MonoGame;
// using Howl.Vendors.MonoGame.Graphics;
// using Microsoft.Xna.Framework.Graphics;

// namespace Howl.Test.MonoGame.Graphics;

// public class MonoGameTextureManagerTest
// {
//     private MonoGameApp monogameApp;
//     private HowlApp howlApp; 

//     public MonoGameTextureManagerTest()
//     {
//         howlApp = new(HowlAppBackend.MonoGame);
//         monogameApp = new();
//     }

//     [Fact]
//     public void LoadTexture()
//     {
//         TextureManager<Texture2D> textureManager = new MonoGameTextureManager(); 
//         Assert.True(textureManager.LoadTextureFromDisc("Dog.png", out Texture2D texture));
//         Assert.True(textureManager.LoadTexture("Dog.png", out GenIndex dogId));
//         ReadonlyRef<Texture2D> dog = textureManager.GetTextureReadonlyRef(dogId);
//         Assert.True(dog.Valid);
//     }

//     [Fact]
//     public void UnloadTexture()
//     {
        
//     }
// }