using Howl.Input;
using Howl.Math;
using Howl.MonoGame.Math;
using Microsoft.Xna.Framework.Input;

namespace Howl.MonoGame.Input;

public class MonoGameGamePad : IGamePad
{
    private GamePadId gamePadId;
    public GamePadId GamepadId => gamePadId;

    private GamePadState currentState;

    private GamePadState previousState;

    public bool IsConnected => currentState.IsConnected;

    public Vector2 ThumbstickLeft => Vector2Translator.ToHowlVector2(currentState.ThumbSticks.Left);

    public Vector2 ThumbstickRight => Vector2Translator.ToHowlVector2(currentState.ThumbSticks.Right);

    public float TriggerLeft => currentState.Triggers.Left;

    public float TriggerRight => currentState.Triggers.Right;

    public MonoGameGamePad(GamePadId gamePadId)
    {
        this.gamePadId = gamePadId;
        currentState = GamePad.GetState(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId));
        previousState = new GamePadState();
    }

    public bool IsButtonDown(GamePadButton gamePadButton)
    {
        return currentState.IsButtonDown(ButtonsTranslator.ToMonoGameButtons(gamePadButton));
    }

    public bool IsButtonJustPressed(GamePadButton gamePadButton)
    {
        Buttons button = ButtonsTranslator.ToMonoGameButtons(gamePadButton);
        return currentState.IsButtonDown(button) && previousState.IsButtonUp(button);
    }

    public bool IsButtonJustReleased(GamePadButton gamePadButton)
    {
        Buttons button = ButtonsTranslator.ToMonoGameButtons(gamePadButton);
        return currentState.IsButtonUp(button) && previousState.IsButtonDown(button);
    }

    public bool IsButtonUp(GamePadButton gamePadButton)
    {
        return currentState.IsButtonUp(ButtonsTranslator.ToMonoGameButtons(gamePadButton));        
    }

    public void SetVibration(float strength)
    {
        float value = float.Clamp(strength, 0, 1); 
        GamePad.SetVibration(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId), value, value);
    }

    public void StopVibration()
    {
        GamePad.SetVibration(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId), 0, 0);
    }

    public void Update(float deltaTime)
    {
        if(IsConnected == false)
        {
            return;
        }

        previousState = currentState;
        currentState = GamePad.GetState(PlayerIndexTranslator.GetMonogamePlayerIndex(gamePadId));
    }
}