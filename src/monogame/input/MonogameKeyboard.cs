using Howl.Input;

namespace Howl.MonoGame.Input;

public class MonoGameKeyboard : IKeyboard
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
    public MonoGameKeyboard()
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
        return currentState.IsKeyDown(KeysTranslator.ToMonoGameKeys(key));
    }

    public bool IsKeyUp(Key key)
    {
        return previousState.IsKeyUp(KeysTranslator.ToMonoGameKeys(key));
    }

    public bool IsKeyJustPressed(Key key)
    {
        return previousState.IsKeyUp(KeysTranslator.ToMonoGameKeys(key)) && currentState.IsKeyDown(KeysTranslator.ToMonoGameKeys(key));
    }

    public bool IsKeyJustReleased(Key key)
    {
        return previousState.IsKeyDown(KeysTranslator.ToMonoGameKeys(key)) && currentState.IsKeyUp(KeysTranslator.ToMonoGameKeys(key));
    }

}