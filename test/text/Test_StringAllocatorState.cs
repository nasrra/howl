using Howl.Text;

namespace Howl.Test.Text;

public class Test_StringAllocatorState
{

    public Test_StringAllocatorState()
    {
        // Clear default listeners that show dialogs
        System.Diagnostics.Trace.Listeners.Clear();
        // Add a listener that throws an exception on failure
        System.Diagnostics.Trace.Listeners.Add(new ThrowingTraceListener());
    }

    [Fact]
    public void Constructor_Test()
    {
        // fail case:
        Debug.Log.Suppress = true;
        StringAllocatorState state;
        Assert.Throws<Exception>(() =>{state = new(0,0);});
        Assert.Throws<Exception>(() =>{state = new(1,1);});
        Debug.Log.Suppress = false;

        // success cases.
        for(int i = StringAllocatorState.MinMaxStringCount; i < 8; i++)
        {
            state = new(i, i+1);
            Assert_StringAllocatorState.LengthEqual(i, i+1, state);
            Assert.Equal(0, state.AllocatedStringCount);

            state = new(i, i);
            Assert_StringAllocatorState.LengthEqual(i, i, state);
            Assert.Equal(0, state.AllocatedStringCount);
        }
    }
}