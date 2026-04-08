// using System.Diagnostics;

// namespace Howl.Test;

// public sealed class DebugAssertSupressor : IDisposable
// {

// #if DEBUG

//     private readonly TraceListenerCollection originalListeners;

//     public DebugAssertSupressor()
//     {
//         // save current listener.
//         originalListeners = System.Diagnostics.Debug.
//     }

//     public void Dispose()
//     {
//     }

// #else

//     public void DebugAssertSupressor();
//     public void Dispose();

// #endif

// }