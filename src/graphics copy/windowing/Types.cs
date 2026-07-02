using SDL = Silk.NET.SDL;

namespace N_Howl.N_Windowing;

public struct WindowManagerInfo{
    public Win32Info Win32Info;
    public CocoaInfo CocoaInfo;
    public WaylandInfo WaylandInfo;
    public X11Info X11Info;
}

public unsafe struct Win32Info{
    public void* Hwnd;
    public void* HDC;
    public void* HInstance;
    public bool IsInitialised;
}

public unsafe struct CocoaInfo{
    public void* Window;
    public bool IsInitialised;
}

public unsafe struct WaylandInfo{
    public void* Display;
    public void* Surface;
    public void* ShellSurface;
    public bool IsInitialised;
}

public unsafe struct X11Info{
    public void* Display;
    public void* Window;
    public bool IsInitialised;
}
