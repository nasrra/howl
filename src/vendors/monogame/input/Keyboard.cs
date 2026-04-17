using Howl.Input;

namespace Howl.Vendors.MonoGame.Input;

public class Keyboard : IKeyboard
{

    /// <summary>
    /// The state of the keyboard input dureing the previous update cycle.
    /// </summary>
    public Microsoft.Xna.Framework.Input.KeyboardState previousState;


    /// <summary>
    /// The state of the keyboard input durring the current update cycle.
    /// </summary>
    private Microsoft.Xna.Framework.Input.KeyboardState currentState;

    /// <summary>
    /// Creates a new Keyboard Info instance.
    /// </summary>
    public Keyboard()
    {
        currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        previousState = currentState;
    }

    public void Update()
    {
        previousState = currentState;
        currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
    }


    public bool IsKeyDown(Key key)
    {
        return currentState.IsKeyDown(key.ToMonoGame());
    }

    public bool IsKeyUp(Key key)
    {
        return currentState.IsKeyUp(key.ToMonoGame());
    }

    public bool IsKeyJustPressed(Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = key.ToMonoGame();
        return previousState.IsKeyUp(keys) && currentState.IsKeyDown(keys);
    }

    public bool IsKeyJustReleased(Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = key.ToMonoGame();
        return previousState.IsKeyDown(keys) && currentState.IsKeyUp(keys);
    }

}