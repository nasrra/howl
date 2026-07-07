using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl;
using N_Howl.N_Input;
using N_Howl.N_Math;
using SDL = Silk.NET.SDL;

namespace N_Howl.N_Windowing;
public unsafe static class Windowing{

public static SDL.Sdl SdlApi = SDL.Sdl.GetApi();

static Windowing(){
    SdlApi.SetMainReady();
}

public unsafe static class GlobalState{
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Copied value from initialisation and resizing; mutating this value will NOT alter the active window resolution.</para>
    /// </remarks>
    public static Vector2UI WindowResolution;
    public static SDL.Window* SdlWindow;
    public static WindowManagerInfo WindowManagerInfo;
    public static bool ShouldClose;
    public static delegate*<void> OnWindowResize;
    public static void* ResizeUserDataPointer;
    public static bool IsInitialised;
}

public static void Init(
    string windowName, uint width, uint height 
){
    // validation step.
    if(GlobalState.IsInitialised){
        // crash.
        Debug.Panic("Cannot initialise window that has already been initialised.");
        return;
    }
    
    // init sdl window.
        /**
        Silk.NET SDL 2.23 windowing inits a X11 window, right now it doesnt really matter as it
        goes through waylands X11 translator (XWayland); however you may need to switch to
        Silk.NET SDL 3.0 whenever that comes out or a different windowing solution.
    **/
    if(SdlApi.Init(SDL.Sdl.InitVideo) < 0){
        Debug.Panic($"Failed to initialise SDL with error msg: '{SdlApi.GetErrorS()}'");
    }
    uint windowFlags = 0;
    GlobalState.SdlWindow = SdlApi.CreateWindow(
        windowName, SDL.Sdl.WindowposCentered, SDL.Sdl.WindowposCentered, (int)width, (int)height, windowFlags
    );

    // fetch native OS handles.
    SDL.SysWMInfo wmInfo;
    SdlApi.GetVersion(&wmInfo.Version);
    bool success = SdlApi.GetWindowWMInfo(GlobalState.SdlWindow, &wmInfo);
    Debug.Assert(success, "Failed to get SDL window manager info.");

    if(wmInfo.Subsystem == SDL.SysWMType.Windows){
        GlobalState.WindowManagerInfo.Win32Info.HInstance = (void*)wmInfo.Info.Win.HInstance;
        GlobalState.WindowManagerInfo.Win32Info.Hwnd = (void*)wmInfo.Info.Win.Hwnd;
        GlobalState.WindowManagerInfo.Win32Info.IsInitialised = true;
    }
    // x11 (linux).
    else if(wmInfo.Subsystem == SDL.SysWMType.X11){
        GlobalState.WindowManagerInfo.X11Info.Display = wmInfo.Info.X11.Display;
        GlobalState.WindowManagerInfo.X11Info.Window = wmInfo.Info.X11.Window;
        GlobalState.WindowManagerInfo.X11Info.IsInitialised = true;
    }
    // wayland (linux).
    else if(wmInfo.Subsystem == SDL.SysWMType.Wayland){
        GlobalState.WindowManagerInfo.WaylandInfo.Display = wmInfo.Info.Wayland.Display;
        GlobalState.WindowManagerInfo.WaylandInfo.Surface = wmInfo.Info.Wayland.Surface;
        GlobalState.WindowManagerInfo.WaylandInfo.IsInitialised = true;
    }
    // cocoa (macos)
    else if(wmInfo.Subsystem == SDL.SysWMType.Cocoa){
        GlobalState.WindowManagerInfo.CocoaInfo.Window = wmInfo.Info.Cocoa.Window;
        GlobalState.WindowManagerInfo.CocoaInfo.IsInitialised = true;
    }

    GlobalState.WindowResolution = new(){X = width, Y = height};
    GlobalState.IsInitialised = true;
}

public static void Update(

){
    SDL.Event e = default;
    // poll all events in the queue in a given frame.
    while(SdlApi.PollEvent(&e) != 0){

        if(e.Type == (uint)SDL.EventType.Quit){
            GlobalState.ShouldClose = true;      
        }

        if (e.Type == (uint)SDL.EventType.Keydown && e.Key.Repeat == 0) {
            SDL.Scancode keyCode = e.Key.Keysym.Scancode;
            switch(keyCode){
                case SDL.Scancode.ScancodeA: Input.SetKeyDown(Key.A, true); break;
                case SDL.Scancode.ScancodeB: Input.SetKeyDown(Key.B, true); break; 
                case SDL.Scancode.ScancodeC: Input.SetKeyDown(Key.C, true); break;
                case SDL.Scancode.ScancodeD: Input.SetKeyDown(Key.D, true); break; 
                case SDL.Scancode.ScancodeE: Input.SetKeyDown(Key.E, true); break;
                case SDL.Scancode.ScancodeF: Input.SetKeyDown(Key.F, true); break; 
                case SDL.Scancode.ScancodeG: Input.SetKeyDown(Key.G, true); break;
                case SDL.Scancode.ScancodeH: Input.SetKeyDown(Key.H, true); break; 
                case SDL.Scancode.ScancodeI: Input.SetKeyDown(Key.I, true); break;
                case SDL.Scancode.ScancodeJ: Input.SetKeyDown(Key.J, true); break; 
                case SDL.Scancode.ScancodeK: Input.SetKeyDown(Key.K, true); break; 
                case SDL.Scancode.ScancodeL: Input.SetKeyDown(Key.L, true); break; 
                case SDL.Scancode.ScancodeM: Input.SetKeyDown(Key.M, true); break; 
                case SDL.Scancode.ScancodeN: Input.SetKeyDown(Key.N, true); break; 
                case SDL.Scancode.ScancodeO: Input.SetKeyDown(Key.O, true); break; 
                case SDL.Scancode.ScancodeP: Input.SetKeyDown(Key.P, true); break; 
                case SDL.Scancode.ScancodeQ: Input.SetKeyDown(Key.Q, true); break; 
                case SDL.Scancode.ScancodeR: Input.SetKeyDown(Key.R, true); break; 
                case SDL.Scancode.ScancodeS: Input.SetKeyDown(Key.S, true); break; 
                case SDL.Scancode.ScancodeT: Input.SetKeyDown(Key.T, true); break; 
                case SDL.Scancode.ScancodeU: Input.SetKeyDown(Key.U, true); break; 
                case SDL.Scancode.ScancodeV: Input.SetKeyDown(Key.V, true); break; 
                case SDL.Scancode.ScancodeW: Input.SetKeyDown(Key.W, true); break; 
                case SDL.Scancode.ScancodeX: Input.SetKeyDown(Key.X, true); break; 
                case SDL.Scancode.ScancodeY: Input.SetKeyDown(Key.Y, true); break; 
                case SDL.Scancode.ScancodeZ: Input.SetKeyDown(Key.Z, true); break; 
                case SDL.Scancode.Scancode1: Input.SetKeyDown(Key.D1, true); break; 
                case SDL.Scancode.Scancode2: Input.SetKeyDown(Key.D2, true); break; 
                case SDL.Scancode.Scancode3: Input.SetKeyDown(Key.D3, true); break; 
                case SDL.Scancode.Scancode4: Input.SetKeyDown(Key.D4, true); break; 
                case SDL.Scancode.Scancode5: Input.SetKeyDown(Key.D5, true); break; 
                case SDL.Scancode.Scancode6: Input.SetKeyDown(Key.D6, true); break; 
                case SDL.Scancode.Scancode7: Input.SetKeyDown(Key.D7, true); break; 
                case SDL.Scancode.Scancode8: Input.SetKeyDown(Key.D8, true); break; 
                case SDL.Scancode.Scancode9: Input.SetKeyDown(Key.D9, true); break; 
                case SDL.Scancode.Scancode0: Input.SetKeyDown(Key.D0, true); break; 
                case SDL.Scancode.ScancodeEscape: Input.SetKeyDown(Key.Escape, true); break;
                case SDL.Scancode.ScancodeBackspace: Input.SetKeyDown(Key.BackSpace, true); break;
                case SDL.Scancode.ScancodeTab: Input.SetKeyDown(Key.Tab, true); break;
                case SDL.Scancode.ScancodeSpace: Input.SetKeyDown(Key.Space, true); break;
                case SDL.Scancode.ScancodeMinus: Input.SetKeyDown(Key.Minus, true); break;
                case SDL.Scancode.ScancodeEquals: Input.SetKeyDown(Key.Equals, true); break;
                case SDL.Scancode.ScancodeLeftbracket: Input.SetKeyDown(Key.LeftBracket, true); break;
                case SDL.Scancode.ScancodeRightbracket: Input.SetKeyDown(Key.RightBracket, true); break;
                case SDL.Scancode.ScancodeBackslash: Input.SetKeyDown(Key.Backslash, true); break;
                case SDL.Scancode.ScancodeSemicolon: Input.SetKeyDown(Key.Semicolon, true); break;
                case SDL.Scancode.ScancodeApostrophe: Input.SetKeyDown(Key.Apostrophe, true); break;
                case SDL.Scancode.ScancodeGrave: Input.SetKeyDown(Key.BackTick, true); break;
                case SDL.Scancode.ScancodeComma: Input.SetKeyDown(Key.Comma, true); break;
                case SDL.Scancode.ScancodePeriod: Input.SetKeyDown(Key.Period, true); break;
                case SDL.Scancode.ScancodeSlash: Input.SetKeyDown(Key.Slash, true); break;
                case SDL.Scancode.ScancodeCapslock: Input.SetKeyDown(Key.Capslock, true); break;
                case SDL.Scancode.ScancodeF1: Input.SetKeyDown(Key.F1, true); break;
                case SDL.Scancode.ScancodeF2: Input.SetKeyDown(Key.F2, true); break;
                case SDL.Scancode.ScancodeF3: Input.SetKeyDown(Key.F3, true); break;
                case SDL.Scancode.ScancodeF4: Input.SetKeyDown(Key.F4, true); break;
                case SDL.Scancode.ScancodeF5: Input.SetKeyDown(Key.F5, true); break;
                case SDL.Scancode.ScancodeF6: Input.SetKeyDown(Key.F6, true); break;
                case SDL.Scancode.ScancodeF7: Input.SetKeyDown(Key.F7, true); break;
                case SDL.Scancode.ScancodeF8: Input.SetKeyDown(Key.F8, true); break;
                case SDL.Scancode.ScancodeF9: Input.SetKeyDown(Key.F9, true); break;
                case SDL.Scancode.ScancodeF10: Input.SetKeyDown(Key.F10, true); break;
                case SDL.Scancode.ScancodeF11: Input.SetKeyDown(Key.F11, true); break;
                case SDL.Scancode.ScancodeF12: Input.SetKeyDown(Key.F12, true); break;
                case SDL.Scancode.ScancodePrintscreen: Input.SetKeyDown(Key.PrintScreen, true); break;
                case SDL.Scancode.ScancodeInsert: Input.SetKeyDown(Key.Insert, true); break;
                case SDL.Scancode.ScancodeHome: Input.SetKeyDown(Key.Home, true); break;
                case SDL.Scancode.ScancodePageup: Input.SetKeyDown(Key.PageUp, true); break;
                case SDL.Scancode.ScancodePagedown: Input.SetKeyDown(Key.PageDown, true); break;
                case SDL.Scancode.ScancodeDelete: Input.SetKeyDown(Key.Delete, true); break;
                case SDL.Scancode.ScancodeEnd: Input.SetKeyDown(Key.End, true); break;
                case SDL.Scancode.ScancodeRight: Input.SetKeyDown(Key.Right, true); break;
                case SDL.Scancode.ScancodeLeft: Input.SetKeyDown(Key.Left, true); break;
                case SDL.Scancode.ScancodeDown: Input.SetKeyDown(Key.Down, true); break;
                case SDL.Scancode.ScancodeUp: Input.SetKeyDown(Key.Up, true); break;
                case SDL.Scancode.ScancodeLctrl: Input.SetKeyDown(Key.LeftControl, true); break;
                case SDL.Scancode.ScancodeLshift: Input.SetKeyDown(Key.LeftShift, true); break;
                case SDL.Scancode.ScancodeLalt: Input.SetKeyDown(Key.LeftAlt, true); break;
                case SDL.Scancode.ScancodeRctrl: Input.SetKeyDown(Key.RightControl, true); break;
                case SDL.Scancode.ScancodeRshift: Input.SetKeyDown(Key.RightShift, true); break;
                case SDL.Scancode.ScancodeRalt: Input.SetKeyDown(Key.RightAlt, true); break;
                case SDL.Scancode.ScancodeReturn: Input.SetKeyDown(Key.Enter, true); break;
            }
        }

        if (e.Type == (uint)SDL.EventType.Keyup && e.Key.Repeat == 0) {
            SDL.Scancode keyCode = e.Key.Keysym.Scancode;
            switch(keyCode){
                case SDL.Scancode.ScancodeA: Input.SetKeyDown(Key.A, false); break;
                case SDL.Scancode.ScancodeB: Input.SetKeyDown(Key.B, false); break; 
                case SDL.Scancode.ScancodeC: Input.SetKeyDown(Key.C, false); break;
                case SDL.Scancode.ScancodeD: Input.SetKeyDown(Key.D, false); break; 
                case SDL.Scancode.ScancodeE: Input.SetKeyDown(Key.E, false); break;
                case SDL.Scancode.ScancodeF: Input.SetKeyDown(Key.F, false); break; 
                case SDL.Scancode.ScancodeG: Input.SetKeyDown(Key.G, false); break;
                case SDL.Scancode.ScancodeH: Input.SetKeyDown(Key.H, false); break; 
                case SDL.Scancode.ScancodeI: Input.SetKeyDown(Key.I, false); break;
                case SDL.Scancode.ScancodeJ: Input.SetKeyDown(Key.J, false); break; 
                case SDL.Scancode.ScancodeK: Input.SetKeyDown(Key.K, false); break; 
                case SDL.Scancode.ScancodeL: Input.SetKeyDown(Key.L, false); break; 
                case SDL.Scancode.ScancodeM: Input.SetKeyDown(Key.M, false); break; 
                case SDL.Scancode.ScancodeN: Input.SetKeyDown(Key.N, false); break; 
                case SDL.Scancode.ScancodeO: Input.SetKeyDown(Key.O, false); break; 
                case SDL.Scancode.ScancodeP: Input.SetKeyDown(Key.P, false); break; 
                case SDL.Scancode.ScancodeQ: Input.SetKeyDown(Key.Q, false); break; 
                case SDL.Scancode.ScancodeR: Input.SetKeyDown(Key.R, false); break; 
                case SDL.Scancode.ScancodeS: Input.SetKeyDown(Key.S, false); break; 
                case SDL.Scancode.ScancodeT: Input.SetKeyDown(Key.T, false); break; 
                case SDL.Scancode.ScancodeU: Input.SetKeyDown(Key.U, false); break; 
                case SDL.Scancode.ScancodeV: Input.SetKeyDown(Key.V, false); break; 
                case SDL.Scancode.ScancodeW: Input.SetKeyDown(Key.W, false); break; 
                case SDL.Scancode.ScancodeX: Input.SetKeyDown(Key.X, false); break; 
                case SDL.Scancode.ScancodeY: Input.SetKeyDown(Key.Y, false); break; 
                case SDL.Scancode.ScancodeZ: Input.SetKeyDown(Key.Z, false); break; 
                case SDL.Scancode.Scancode1: Input.SetKeyDown(Key.D1, false); break; 
                case SDL.Scancode.Scancode2: Input.SetKeyDown(Key.D2, false); break; 
                case SDL.Scancode.Scancode3: Input.SetKeyDown(Key.D3, false); break; 
                case SDL.Scancode.Scancode4: Input.SetKeyDown(Key.D4, false); break; 
                case SDL.Scancode.Scancode5: Input.SetKeyDown(Key.D5, false); break; 
                case SDL.Scancode.Scancode6: Input.SetKeyDown(Key.D6, false); break; 
                case SDL.Scancode.Scancode7: Input.SetKeyDown(Key.D7, false); break; 
                case SDL.Scancode.Scancode8: Input.SetKeyDown(Key.D8, false); break; 
                case SDL.Scancode.Scancode9: Input.SetKeyDown(Key.D9, false); break; 
                case SDL.Scancode.Scancode0: Input.SetKeyDown(Key.D0, false); break; 
                case SDL.Scancode.ScancodeEscape: Input.SetKeyDown(Key.Escape, false); break;
                case SDL.Scancode.ScancodeBackspace: Input.SetKeyDown(Key.BackSpace, false); break;
                case SDL.Scancode.ScancodeTab: Input.SetKeyDown(Key.Tab, false); break;
                case SDL.Scancode.ScancodeSpace: Input.SetKeyDown(Key.Space, false); break;
                case SDL.Scancode.ScancodeMinus: Input.SetKeyDown(Key.Minus, false); break;
                case SDL.Scancode.ScancodeEquals: Input.SetKeyDown(Key.Equals, false); break;
                case SDL.Scancode.ScancodeLeftbracket: Input.SetKeyDown(Key.LeftBracket, false); break;
                case SDL.Scancode.ScancodeRightbracket: Input.SetKeyDown(Key.RightBracket, false); break;
                case SDL.Scancode.ScancodeBackslash: Input.SetKeyDown(Key.Backslash, false); break;
                case SDL.Scancode.ScancodeSemicolon: Input.SetKeyDown(Key.Semicolon, false); break;
                case SDL.Scancode.ScancodeApostrophe: Input.SetKeyDown(Key.Apostrophe, false); break;
                case SDL.Scancode.ScancodeGrave: Input.SetKeyDown(Key.BackTick, false); break;
                case SDL.Scancode.ScancodeComma: Input.SetKeyDown(Key.Comma, false); break;
                case SDL.Scancode.ScancodePeriod: Input.SetKeyDown(Key.Period, false); break;
                case SDL.Scancode.ScancodeSlash: Input.SetKeyDown(Key.Slash, false); break;
                case SDL.Scancode.ScancodeCapslock: Input.SetKeyDown(Key.Capslock, true); break;
                case SDL.Scancode.ScancodeF1: Input.SetKeyDown(Key.F1, false); break;
                case SDL.Scancode.ScancodeF2: Input.SetKeyDown(Key.F2, false); break;
                case SDL.Scancode.ScancodeF3: Input.SetKeyDown(Key.F3, false); break;
                case SDL.Scancode.ScancodeF4: Input.SetKeyDown(Key.F4, false); break;
                case SDL.Scancode.ScancodeF5: Input.SetKeyDown(Key.F5, false); break;
                case SDL.Scancode.ScancodeF6: Input.SetKeyDown(Key.F6, false); break;
                case SDL.Scancode.ScancodeF7: Input.SetKeyDown(Key.F7, false); break;
                case SDL.Scancode.ScancodeF8: Input.SetKeyDown(Key.F8, false); break;
                case SDL.Scancode.ScancodeF9: Input.SetKeyDown(Key.F9, false); break;
                case SDL.Scancode.ScancodeF10: Input.SetKeyDown(Key.F10, false); break;
                case SDL.Scancode.ScancodeF11: Input.SetKeyDown(Key.F11, false); break;
                case SDL.Scancode.ScancodeF12: Input.SetKeyDown(Key.F12, false); break;
                case SDL.Scancode.ScancodePrintscreen: Input.SetKeyDown(Key.PrintScreen, false); break;
                case SDL.Scancode.ScancodeInsert: Input.SetKeyDown(Key.Insert, false); break;
                case SDL.Scancode.ScancodeHome: Input.SetKeyDown(Key.Home, false); break;
                case SDL.Scancode.ScancodePageup: Input.SetKeyDown(Key.PageUp, false); break;
                case SDL.Scancode.ScancodePagedown: Input.SetKeyDown(Key.PageDown, false); break;
                case SDL.Scancode.ScancodeDelete: Input.SetKeyDown(Key.Delete, false); break;
                case SDL.Scancode.ScancodeEnd: Input.SetKeyDown(Key.End, false); break;
                case SDL.Scancode.ScancodeRight: Input.SetKeyDown(Key.Right, false); break;
                case SDL.Scancode.ScancodeLeft: Input.SetKeyDown(Key.Left, false); break;
                case SDL.Scancode.ScancodeDown: Input.SetKeyDown(Key.Down, false); break;
                case SDL.Scancode.ScancodeUp: Input.SetKeyDown(Key.Up, false); break;
                case SDL.Scancode.ScancodeLctrl: Input.SetKeyDown(Key.LeftControl, false); break;
                case SDL.Scancode.ScancodeLshift: Input.SetKeyDown(Key.LeftShift, false); break;
                case SDL.Scancode.ScancodeLalt: Input.SetKeyDown(Key.LeftAlt, false); break;
                case SDL.Scancode.ScancodeRctrl: Input.SetKeyDown(Key.RightControl, false); break;
                case SDL.Scancode.ScancodeRshift: Input.SetKeyDown(Key.RightShift, false); break;
                case SDL.Scancode.ScancodeRalt: Input.SetKeyDown(Key.RightAlt, false); break;
                case SDL.Scancode.ScancodeReturn: Input.SetKeyDown(Key.Enter, false); break;
            }
        }
        if (e.Type == (uint)SDL.EventType.Mousebuttondown) {
            // e.Button.Button raw IDs: 1 = Left, 2 = Middle, 3 = Right
            byte buttonCode = e.Button.Button;
            
            Input.SetMouseButtonDown(MouseButton.Left,   buttonCode == 1);
            Input.SetMouseButtonDown(MouseButton.Middle, buttonCode == 2);
            Input.SetMouseButtonDown(MouseButton.Right,  buttonCode == 3);
        }

        if (e.Type == (uint)SDL.EventType.Mousebuttonup) {
            byte buttonCode = e.Button.Button;
            
            // If the button code matches, set that specific button state to false
            if (buttonCode == 1) Input.SetMouseButtonDown(MouseButton.Left, false);
            if (buttonCode == 2) Input.SetMouseButtonDown(MouseButton.Middle, false);
            if (buttonCode == 3) Input.SetMouseButtonDown(MouseButton.Right, false);
        }
        
        if(e.Type == (uint)SDL.EventType.Windowevent){
            if(
                e.Window.Event == (byte)SDL.WindowEventID.Moved ||
                e.Window.Event == (byte)SDL.WindowEventID.FocusLost ||
                e.Window.Event == (byte)SDL.WindowEventID.Leave            
            ){
                // input should be cleared otherwise the application will not pickup a key being release; because SDL is soo awesome :))))
                Input.ClearStates();
            }
        }


    }

    SdlApi.GetMouseState(
        (int*)Unsafe.AsPointer(ref Input.GlobalState.MouseBackBufferX), 
        (int*)Unsafe.AsPointer(ref Input.GlobalState.MouseBackBufferY)
    );
    
    Input.Update();
}

public static void SetWindowFullscreen(
    bool borderless
){
    { // validation.
        // crash.
        Debug.Assert(GlobalState.IsInitialised, "Cant set an unintialised window to borderless fullscreen.");
    }

    if(borderless){
        SdlApi.SetWindowFullscreen(GlobalState.SdlWindow, (uint)SDL.WindowFlags.FullscreenDesktop);
    }
    else{
        SdlApi.SetWindowFullscreen(GlobalState.SdlWindow, (uint)SDL.WindowFlags.Fullscreen);
    }
    // position setting is  needed as there is an edge case where the user moved the window, causing maximise to break.
    SdlApi.SetWindowPosition(GlobalState.SdlWindow, 0, 0);
    int width;
    int height;
    SdlApi.GetWindowSizeInPixels(GlobalState.SdlWindow, &width, &height);
    GlobalState.WindowResolution = new(){X = (uint)width, Y = (uint)height};
    if(GlobalState.OnWindowResize != null){
        GlobalState.OnWindowResize();
    }
}

public static float CalculateAspectRatio(

){
    { // validation.
        // crash.
        Debug.Assert(GlobalState.IsInitialised, "Cant calculate the aspect ratio of an uninitialised window.");
    }
    return (float)GlobalState.WindowResolution.X / GlobalState.WindowResolution.Y;
}

public static void SetWindowWindowed(
    uint width, uint height
){
    { // validation.

        // crash.
        Debug.Assert(GlobalState.IsInitialised, "Web GPU cant set an unintialised window to windowed mode.");
    }
    
    SdlApi.SetWindowBordered(GlobalState.SdlWindow, SDL.SdlBool.True);
    SdlApi.SetWindowFullscreen(GlobalState.SdlWindow, 0u);
    SdlApi.SetWindowSize(GlobalState.SdlWindow, (int)width, (int)height);
    GlobalState.WindowResolution = new(){X = width, Y = height};
    if(GlobalState.OnWindowResize != null){
        GlobalState.OnWindowResize();
    }
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2UI GetWindowResolution(

){
    return GlobalState.WindowResolution;
}

public static void FreeSDL(){
    SdlApi.DestroyWindowSurface(GlobalState.SdlWindow);
    SdlApi.DestroyWindow(GlobalState.SdlWindow);
}

}