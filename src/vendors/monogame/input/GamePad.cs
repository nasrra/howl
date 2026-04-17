using Howl.Input;
using Howl.Math;
using Howl.Vendors.MonoGame.Math;
using Microsoft.Xna.Framework.Input;

namespace Howl.Vendors.MonoGame.Input;

public class GamePad : IGamePad
{
    private GamePadId gamePadId;
    public GamePadId GamepadId => gamePadId;

    private GamePadState currentState;

    private GamePadState previousState;

    public bool IsConnected => currentState.IsConnected;

    public Vector2 ThumbstickLeft => currentState.ThumbSticks.Left.ToHowl();

    public Vector2 ThumbstickRight => currentState.ThumbSticks.Right.ToHowl();

    public float TriggerLeft => currentState.Triggers.Left;

    public float TriggerRight => currentState.Triggers.Right;

    public GamePad(GamePadId gamePadId)
    {
        this.gamePadId = gamePadId;
        currentState = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId));
        previousState = new GamePadState();
    }

    public bool IsButtonDown(GamePadButton gamePadButton)
    {
        return currentState.IsButtonDown(gamePadButton.ToMonoGame());
    }

    public bool IsButtonJustPressed(GamePadButton gamePadButton)
    {
        Buttons button = gamePadButton.ToMonoGame();
        return currentState.IsButtonDown(button) && previousState.IsButtonUp(button);
    }

    public bool IsButtonJustReleased(GamePadButton gamePadButton)
    {
        Buttons button = gamePadButton.ToMonoGame();
        return currentState.IsButtonUp(button) && previousState.IsButtonDown(button);
    }

    public bool IsButtonUp(GamePadButton gamePadButton)
    {
        return currentState.IsButtonUp(gamePadButton.ToMonoGame());        
    }

    public void SetVibration(float strength)
    {
        float value = float.Clamp(strength, 0, 1); 
        Microsoft.Xna.Framework.Input.GamePad.SetVibration(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId), value, value);
    }

    public void StopVibration()
    {
        Microsoft.Xna.Framework.Input.GamePad.SetVibration(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId), 0, 0);
    }

    public void Update(float deltaTime)
    {
        if(IsConnected == false)
        {
            return;
        }

        previousState = currentState;
        currentState = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId));
    }
}