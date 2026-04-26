using Howl.Text;

namespace Howl.Test.Text;

public class Test_StringRegistryState
{
    [Fact]
    public void Constructor_Test()
    {
        for(int maxCharacters = 0; maxCharacters < 8; maxCharacters++)
        {
            StringRegistryState state = new(maxCharacters);
            
            // note: add one to maxStringCharacters as arrays are zero indexed:
            // example:
            // [0] = characterCount 0.
            // [1] = characterCount 1.
            Assert_StringRegistryState.LengthEqual(maxCharacters+1, state);
        }
    }
}