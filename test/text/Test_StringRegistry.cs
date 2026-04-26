using Howl.Text;

namespace Howl.Test.Text;

public class Test_StringRegistry
{
    [Fact]
    public void AllocateAllocator_SuccessTest()
    {
        int maxStringCount = 2;
        int start = 1;

        for(int length = start; length < 8; length++)
        {
            StringRegistryState state = new(length);

            for(int maxCharacterCount = start; maxCharacterCount < length; maxCharacterCount++)
            {
                maxStringCount++;
                
                Assert.Null(state.StringAllocators[maxCharacterCount]);
                
                Assert.True(StringRegistry.AllocateAllocator(state, maxCharacterCount, maxStringCount));
                
                StringAllocatorState allocator = StringRegistry.GetAllocator(state, maxCharacterCount); 
                Assert.NotNull(allocator);
                Assert_StringAllocatorState.LengthEqual(maxCharacterCount, maxStringCount, allocator);
            }

        }
    }

    [Fact]
    public void AllocateAllocator_FailTest()
    {
        int maxCharacterCount = 12;
        int allocatorLength = 4;
        int maxStringCount = 2;

        StringRegistryState state = new(maxCharacterCount);

        // success case.
        Assert.True(StringRegistry.AllocateAllocator(state, allocatorLength, maxStringCount));
        Assert.NotNull(state.StringAllocators[allocatorLength]);

        // fail case:
        Debug.Log.Suppress = true;
        Assert.False(StringRegistry.AllocateAllocator(state, allocatorLength, maxStringCount+10));        
        Debug.Log.Suppress = false;

        // ensure success case wasnt overwritten.
        StringAllocatorState allocator = state.StringAllocators[allocatorLength];
        Assert_StringAllocatorState.LengthEqual(allocatorLength, maxStringCount, allocator);
    }

    [Fact]
    public void DeallocateAllocator_SuccessTest()
    {
        int maxStringCount = 2;
        int start = 1;

        for(int length = start; length < 8; length++)
        {
            StringRegistryState state = new(length);

            // populate.
            for(int allocatorLength = start; allocatorLength < length; allocatorLength++)
            {
                Assert.True(StringRegistry.AllocateAllocator(state, allocatorLength, maxStringCount++));
            }

            // deallocate.
            for(int allocatorLength = start; allocatorLength < length; allocatorLength += 2)
            {
                Assert.NotNull(state.StringAllocators[allocatorLength]);
                
                Assert.True(StringRegistry.DeallocateAllocator(state, allocatorLength));                
                
                Assert.Null(state.StringAllocators[allocatorLength]);
            }

            // reallocate.
            for(int allocatorLength = start; allocatorLength < length; allocatorLength += 2)
            {                
                Assert.Null(state.StringAllocators[allocatorLength]);
                
                maxStringCount++;
                Assert.True(StringRegistry.AllocateAllocator(state, allocatorLength, maxStringCount));
                
                StringAllocatorState allocator = state.StringAllocators[allocatorLength]; 
                Assert.NotNull(allocator);
                Assert_StringAllocatorState.LengthEqual(allocatorLength, maxStringCount, allocator);
            }
        }
    }

    [Fact]
    public void DeallocateAllocator_FailTest()
    {
        int maxCharacters = 12;
        int allocatorLength = 3;
        int maxStringCount = 2;

        StringRegistryState state = new(maxCharacters);

        // populate.
        StringRegistry.AllocateAllocator(state, allocatorLength, maxStringCount);
        Assert.NotNull(state.StringAllocators[allocatorLength]);

        // success case.
        Assert.True(StringRegistry.DeallocateAllocator(state, allocatorLength));
        Assert.Null(state.StringAllocators[allocatorLength]);

        // fail case:
        Debug.Log.Suppress = true;
        
        Assert.False(StringRegistry.DeallocateAllocator(state, allocatorLength));
        Assert.Null(state.StringAllocators[allocatorLength]);
        
        Debug.Log.Suppress = false;
    }

    [Fact]
    public void StringMutation_Test()
    {
        Debug.Log.Suppress = true;


        int maxStringCharacters = 10;
        int maxCharacterCount = 3;
        int maxStringCount = 4;

        bool isValid = false;

        StringId nilStringId = new(0, maxCharacterCount);
        StringId stringId1 = default;
        StringId stringId2 = default;
        StringId stringId3 = default;

        char[] allocatorNilString = ['N','i','l'];
        char[] registryNilString = [];
        
        char[] allocatedString1 = ['f','o','o'];
        char[] allocatedString2 = ['b','a','r'];
        char[] allocatedString3 = ['f','a','i','l'];

        char[] modifiedString1 = ['q','w','e'];
        char[] modifiedString2 = ['d','s','a'];
        char[] modifiedString3 = ['e','r','o','r'];

        // create the state.
        StringRegistryState state = new(maxStringCharacters);
        Assert.True(StringRegistry.AllocateAllocator(state, maxCharacterCount, maxStringCount));
        
        // assert the registry nil string.
        Assert.Equal(registryNilString, StringRegistry.GetString(state, default, ref isValid));
        
        // set the nil string for the allcocator.
        Assert.True(StringRegistry.SetAllocatorNilString(state, allocatorNilString, maxCharacterCount));
        Assert.Equal(allocatorNilString, StringRegistry.GetString(state, nilStringId, ref isValid));

        // attempt to allocate the strings.
        Assert.True(StringRegistry.AllocateString(state, allocatedString1, maxCharacterCount, ref stringId1));
        Assert.True(StringRegistry.AllocateString(state, allocatedString2, maxCharacterCount, ref stringId2));
        Assert.False(StringRegistry.AllocateString(state, allocatedString3, maxCharacterCount, ref stringId3));

        // == attempt to get the strings ==. 

        Span<char> c1 = StringRegistry.GetString(state, stringId1, ref isValid);
        Assert.True(isValid);
        Assert.Equal(allocatedString1, c1);

        Span<char> c2 = StringRegistry.GetString(state, stringId2, ref isValid);
        Assert.True(isValid);
        Assert.Equal(allocatedString2, c2);

        Span<char> c3 = StringRegistry.GetString(state, stringId3, ref isValid);
        Assert.False(isValid);
        Assert.Equal(registryNilString, c3);

        // == attemp to set the strings.

        Assert.True(StringRegistry.SetString(state, modifiedString1, stringId1));
        Span<char> c4 = StringRegistry.GetString(state, stringId1, ref isValid);
        Assert.True(isValid);
        Assert.Equal(modifiedString1, c4);

        Assert.True(StringRegistry.SetString(state, modifiedString2, stringId2));
        Span<char> c5 = StringRegistry.GetString(state, stringId2, ref isValid);
        Assert.True(isValid);
        Assert.Equal(modifiedString2, c5);

        Assert.False(StringRegistry.SetString(state, modifiedString3, stringId3));
        Span<char> c6 = StringRegistry.GetString(state, stringId3, ref isValid);
        Assert.False(isValid);
        Assert.Equal(registryNilString, c6);

        // == deallocate the strings. ==.

        Assert.True(StringRegistry.DeallocateString(state, stringId1));
        Span<char> c7 = StringRegistry.GetString(state, stringId1, ref isValid);
        Assert.False(isValid);
        Assert.Equal(allocatorNilString, c7);

        Assert.True(StringRegistry.DeallocateString(state, stringId2));
        Span<char> c8 = StringRegistry.GetString(state, stringId2, ref isValid);
        Assert.False(isValid);
        Assert.Equal(allocatorNilString, c8);

        Assert.False(StringRegistry.DeallocateString(state, stringId3));
        Span<char> c9 = StringRegistry.GetString(state, stringId3, ref isValid);
        Assert.False(isValid);
        Assert.Equal(registryNilString, c9);


        Debug.Log.Suppress = false;
    }

    [Fact]
    public void Dispose_Test()
    {
        StringId placeholder = default;
        StringRegistryState state = new(10);
        Assert.True(StringRegistry.AllocateAllocator(state, 3, 12));
        Assert.True(StringRegistry.AllocateString(state, ['f','o','o'], 3, ref placeholder));

        StringRegistry.Dispose(state);

        Assert_StringRegistryState.Disposed(state);
    }
}