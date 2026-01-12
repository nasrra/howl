using Howl.Input;

namespace Howl.Monogame.Input;

public class MonogameKeyboard : IKeyboard
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
    public MonogameKeyboard()
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
        return currentState.IsKeyDown(KeysTranslator.ToMonogameKeys(key));
    }

    public bool IsKeyUp(Key key)
    {
        return previousState.IsKeyUp(KeysTranslator.ToMonogameKeys(key));
    }

    public bool IsKeyJustPressed(Key key)
    {
        return previousState.IsKeyUp(KeysTranslator.ToMonogameKeys(key)) && currentState.IsKeyDown(KeysTranslator.ToMonogameKeys(key));
    }

    public bool IsKeyJustReleased(Key key)
    {
        return previousState.IsKeyDown(KeysTranslator.ToMonogameKeys(key)) && currentState.IsKeyUp(KeysTranslator.ToMonogameKeys(key));
    }

}