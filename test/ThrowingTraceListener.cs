using System.Diagnostics;

namespace Howl.Test;


/// <summary>
/// Why you need this:
/// By default when a <c>Debug.Assert</c> fails, it tries to open a Windows/Linux dialog box or writes to the "Output"
/// window; xUnit doesn't "see" that. By adding this listener to <c>Trace.Listeners</c>, you are telling .NET: "Instead of 
/// showing a dialog box, throw a standard C# Exception." Once that exception is thrown, a <c>Assert.Throws<Exception>(...)</c> 
/// will catch the error instead of halting the test case.
/// </summary>
public class ThrowingTraceListener : TraceListener
{
    public override void Fail(string? message) 
        => throw new Exception(message);

    public override void Fail(string? message, string? detailMessage) 
        => throw new Exception($"{message}: {detailMessage}");

    public override void Write(string? message) { }
    public override void WriteLine(string? message) { }
}


// copy and paste this code into a test class constructor that expects Debug asserts.

// public Test_MyClass()
// {
//     // Clear default listeners that show dialogs
//     System.Diagnostics.Trace.Listeners.Clear();
//     // Add a listener that throws an exception on failure
//     System.Diagnostics.Trace.Listeners.Add(new ThrowingTraceListener());
// }
