using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Howl;

public class HowlApp : Game
{
    public static HowlApp Instance {get; private set;}

    public static new GraphicsDevice GraphicsDevice {get; private set;}

    public static GraphicsDeviceManager GraphicsDeviceManager {get; private set;}
    
    public static SpriteBatch SpriteBatch {get; private set;}

    public HowlApp()
    {
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.WriteLine("");   
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        GraphicsDevice = base.GraphicsDevice;
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}
