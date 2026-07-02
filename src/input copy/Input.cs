using System.Runtime.InteropServices;
using N_Howl.N_Math;
using N_Howl.N_Rendering;
using N_Howl.N_Windowing;

namespace N_Howl.N_Input;
public unsafe static class Input{

public static class GlobalState{
    public static bool[] KeyDown = new bool[(int)Key.Length];
    public static InputState[] PreviousKeyStates = new InputState[(int)Key.Length];
    public static InputState[] KeyStates = new InputState[(int)Key.Length];    
    public static bool[] MouseButtonDown = new bool[(int)Key.Length];
    public static InputState[] PreviousMouseButtonStates = new InputState[(int)MouseButton.Length];
    public static InputState[] MouseButtonStates = new InputState[(int)MouseButton.Length];
    public static int MouseBackBufferX;
    public static int MouseBackBufferY;
}

public static void SetKeyDown(
    Key key, bool isDown
){
    GlobalState.KeyDown[(int)key] = isDown;
}

public static void SetMouseButtonDown(
    MouseButton button, bool isDown
){
    GlobalState.MouseButtonDown[(int)button] = isDown;
}

public static void SetKeyState(
    Key key, InputState state
){
    GlobalState.KeyStates[(int)key] = state;
}

public static bool IsKeyPressed(
    Key key
){
    ref InputState state = ref GlobalState.KeyStates[(int)key]; 
    return state == InputState.Pressed || state == InputState.JustPressed;
}

public static bool IsKeyJustPressed(
    Key key
){
    return GlobalState.KeyStates[(int)key] == InputState.JustPressed;
}

public static bool IsKeyReleased(
    Key key
){
    ref InputState state = ref GlobalState.KeyStates[(int)key];
    return state == InputState.Released || state == InputState.JustReleased;
}

public static bool IsKeyJustReleased(
    Key key
){
    return GlobalState.KeyStates[(int)key] == InputState.JustReleased;
}

public static bool IsMouseButtonPressed(
    MouseButton button
){
    ref InputState state = ref GlobalState.MouseButtonStates[(int)button]; 
    return state == InputState.Pressed || state == InputState.JustPressed;
}

public static bool IsMouseButtonJustPressed(
    MouseButton button
){
    return GlobalState.MouseButtonStates[(int)button] == InputState.JustPressed;
}

public static bool IsMouseButtonReleased(
    MouseButton button
){
    ref InputState state = ref GlobalState.MouseButtonStates[(int)button];
    return state == InputState.Released || state == InputState.JustReleased;
}

public static bool IsMouseButtonJustReleased(
    MouseButton button
){
    return GlobalState.MouseButtonStates[(int)button] == InputState.JustReleased;
}

public static void Update(

){
    // update key states
    for(int i = 0; i < (int)Key.Length; i++){
        ref InputState keyState = ref GlobalState.PreviousKeyStates[i];
        if(GlobalState.KeyDown[i] == true){
            switch(keyState){
                case InputState.JustPressed:
                    keyState = InputState.Pressed;
                    break;
                case InputState.JustReleased:
                    keyState = InputState.JustPressed;
                    break;
                case InputState.Released:
                    keyState = InputState.JustPressed;
                    break;
                default: 
                    break;
            }
        }
        else{
            switch(keyState){
                case InputState.JustReleased:
                    keyState = InputState.Released;
                    break;
                case InputState.JustPressed:
                    keyState = InputState.JustReleased;
                    break;
                case InputState.Pressed:
                    keyState = InputState.JustReleased;
                    break;
                default: 
                    break;
            }
        }
    }

    // update mouse button states.
    for(int i = 0; i < (int)MouseButton.Length; i++){
        ref InputState buttonState = ref GlobalState.PreviousMouseButtonStates[i];
        if(GlobalState.KeyDown[i] == true){
            switch(buttonState){
                case InputState.JustPressed:
                    buttonState = InputState.Pressed;
                    break;
                case InputState.JustReleased:
                    buttonState = InputState.JustPressed;
                    break;
                case InputState.Released:
                    buttonState = InputState.JustPressed;
                    break;
                default: 
                    break;
            }
        }
        else{
            switch(buttonState){
                case InputState.JustReleased:
                    buttonState = InputState.Released;
                    break;
                case InputState.JustPressed:
                    buttonState = InputState.JustReleased;
                    break;
                case InputState.Pressed:
                    buttonState = InputState.JustReleased;
                    break;
                default: 
                    break;
            }
        }
    }

    // swap the states.
#pragma warning disable
    InputState[] temp = null; 
#pragma warning enable

    temp = GlobalState.KeyStates; 
    GlobalState.KeyStates = GlobalState.PreviousKeyStates;
    GlobalState.PreviousKeyStates = temp;

    temp = GlobalState.MouseButtonStates;
    GlobalState.MouseButtonStates = GlobalState.PreviousMouseButtonStates;
    GlobalState.PreviousMouseButtonStates = temp;
}

public static void ClearStates(){
    // set all key down states to false when the user clicks off the window.
    fixed(void* ptr = GlobalState.KeyDown){
        NativeMemory.Clear(ptr, (nuint)(sizeof(bool) * GlobalState.KeyDown.Length));
    }
    fixed(void* ptr = GlobalState.KeyStates){
        NativeMemory.Clear(ptr, (nuint)(sizeof(InputState) * GlobalState.KeyStates.Length));
    }
    fixed(void* ptr = GlobalState.PreviousKeyStates){
        NativeMemory.Clear(ptr, (nuint)(sizeof(InputState) * GlobalState.PreviousKeyStates.Length));
    }
    fixed(void* ptr = GlobalState.MouseButtonDown){
        NativeMemory.Clear(ptr, (nuint)(sizeof(bool) * GlobalState.MouseButtonDown.Length));
    }
    fixed(void* ptr = GlobalState.MouseButtonStates){
        NativeMemory.Clear(ptr, (nuint)(sizeof(InputState) * GlobalState.MouseButtonStates.Length));
    }
}

}