namespace Howl.Input;

public enum GamePadButton : byte
{
    // Dpad buttons.
    DpadNorth,
    DpadEast,
    DpadSouth,
    DpadWest,
    
    // Face bnuttons (X,Y,B,A, etc...)
    FaceNorth,
    FaceEast,
    FaceSouth,
    FaceWest,

    // Shoulders.
    ShoulderRight,
    ShoulderLeft,

    // Triggers.
    TriggerRight,
    TriggerLeft,

    // Special.
    Start,
    Menu,

    // Thumbsticks.
    LeftThumbstick,
    RightThumbstick
}