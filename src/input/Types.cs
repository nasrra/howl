namespace N_Howl.N_Input;

public enum Key : int
{
    // Basic Keys
    None,
    BackSpace,
    Tab,
    Enter,
    Escape,
    Space,
    PageUp,
    PageDown,
    End,
    Home,
    Left,
    Up,
    Right,
    Down,
    Select,
    Execute,
    PrintScreen,
    Insert,
    Delete,
    Help,
    Pause,
    Equals,
    Minus,
    LeftBracket,
    RightBracket,
    Backslash,
    Semicolon,
    Apostrophe,
    BackTick,
    Comma,
    Period,
    Slash,
    Capslock,

    // Digit Keys (0-9)
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,

    // Letter Keys (A-Z)
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z,

    // NumPad keys
    NP0,
    NP1,
    NP2,
    NP3,
    NP4,
    NP5,
    NP6,
    NP7,
    NP8,
    NP9,

    // NumPad operators
    NPMultiply,
    NPAdd,
    NPSeparator,
    NPSubtract,
    NPDecimal,
    NPDivide,

    // Function keys
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
    F13,
    F14,
    F15,
    F16,
    F17,
    F18,
    F19,
    F20,
    F21,
    F22,
    F23,
    F24,

    // Modifier Keys
    LeftShift,
    RightShift,
    LeftControl,
    RightControl,
    LeftAlt,
    RightAlt,

    /// <summary>
    ///     the total length of entries in this enum
    /// </summary>
    Length
}

public enum MouseButton{
    Left,
    Right,
    Middle,
    /// <summary>
    ///     the total length of entries in this enum
    /// </summary>
    Length   
}

public enum InputState{
    Released,
    Pressed,
    JustPressed,
    JustReleased   
}

public enum GamePadId : byte
{
    One,
    Two,
    Three,
    Four,

    /// <summary>
    /// Used to know how many gamepads are available.
    /// </summary>
    Count,
}

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