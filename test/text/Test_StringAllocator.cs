using Howl.Text;

namespace Howl.Test.Text;

public class Test_StringAllocator
{
    [Fact]
    public void Allocate_Test()
    {
        bool expectedAllocated = true;
        char[] charSet = ['a', 'b', 'c', 'd' , 'e', 'f', 'g', 'h'];

        // note that both loops start at two, this is because of the Nil value.`
        for(int maxStringCount = StringAllocatorState.MinMaxStringCount; maxStringCount < charSet.Length; maxStringCount++)
        {
            
            for(int stride = StringAllocatorState.MinMaxCharacterCount; stride < charSet.Length; stride++)
            {
                int stringIndex = 0;
                StringAllocatorState state = new(stride, maxStringCount);
                
                // fail case: string character length is too long.
                Debug.Log.Suppress = true;

                Assert.False(StringAllocator.Allocate(state, ['f', 'a', 'i', 'l', ' ', 'c', 'a', 's', 'e'], ref stringIndex));
                Assert.Equal(0, stringIndex);
                Assert.Equal(0, state.AllocatedStringCount);

                Debug.Log.Suppress = false;

                char[] chars = new char[stride];
                int j = 0;
                for(int i = 0; i < maxStringCount-1; i++)
                {
                    
                    // index should be 0 + 1 as there should be a Nil.
                    int expectedStringIndex = i+1;

                    // the expected terminator index is one full stride after, as the chars 
                    // being written are the exact length of the stride.
                    int expectedTerminatorIndex = (expectedStringIndex * stride) + stride;

                    // write the new chars.
                    chars = new char[stride];
                    for(int q = 0; q < stride; q++)
                    {
                        j+=1;
                        j%=charSet.Length;
                        chars[q] = charSet[j];
                    }


                    // allocate the string.
                    Assert.True(StringAllocator.Allocate(state, chars, ref stringIndex));

                    Assert.Equal(expectedStringIndex, stringIndex);

                    // ensure that the count went up.
                    Assert.Equal(expectedStringIndex, state.AllocatedStringCount);

                    // assert the allocation.
                    Assert_StringAllocatorState.EntryEqual(chars, expectedTerminatorIndex, expectedAllocated, stringIndex, state);   
                }

                // fail case: allocator is full.
                Debug.Log.Suppress = true;

                Assert.False(StringAllocator.Allocate(state, chars, ref stringIndex));
                Assert.Equal(maxStringCount-1, stringIndex);
                Assert.Equal(maxStringCount-1, state.AllocatedStringCount);

                Debug.Log.Suppress = false;
            }
        }
    }

    [Fact]
    public void Deallocate_Test()
    {
        bool expectedAllocated = true;
        char[] charSet = ['a', 'b', 'c', 'd' , 'e', 'f', 'g', 'h'];        
        
        // note that both loops start at two, this is because of the Nil value.`
        for(int maxStringCount = StringAllocatorState.MinMaxStringCount; maxStringCount < charSet.Length; maxStringCount++)
        {
            for(int stride = StringAllocatorState.MinMaxCharacterCount; stride < charSet.Length; stride++)
            {
                int stringIndex = 0;
                StringAllocatorState state = new(stride, maxStringCount);
                
                char[] chars;
                int j = 0;
                // populate with strings.
                for(int i = 0; i < maxStringCount-1; i++)
                {                    
                    chars = new char[stride];
                    for(int q = 0; q < stride; q++)
                    {
                        j+=1;
                        j%=charSet.Length;
                        chars[q] = charSet[j];
                    }
                    StringAllocator.Allocate(state, chars, ref stringIndex);
                }

                // deallocate every second string.
                for(int deallocateIndex = 1; deallocateIndex < maxStringCount-1; deallocateIndex += 2)
                {
                    Assert.True(StringAllocator.Deallocate(state, deallocateIndex));
                    Assert.Equal(deallocateIndex, StackArray.Peek(state.FreeStringIndices));
                    Assert.False(state.Allocated[deallocateIndex]);
                }

                // populate the deallocated strings with new strings.
                for(int allocateIndex = 1; allocateIndex < maxStringCount-1; allocateIndex += 2)
                {

                    // write the new chars.
                    chars = new char[stride];
                    for(int q = 0; q < stride; q++)
                    {
                        j+=1;
                        j%=charSet.Length;
                        chars[q] = charSet[j];
                    }

                    // allocate the string.
                    Assert.True(StringAllocator.Allocate(state, chars, ref stringIndex));
                    // the expected terminator index is one full stride after, as the chars 

                    // being written are the exact length of the stride.
                    int expectedTerminatorIndex = (stringIndex * stride) + stride;

                    // assert the allocation.
                    Assert_StringAllocatorState.EntryEqual(chars, expectedTerminatorIndex, expectedAllocated, stringIndex, state);    
                }
            }
        }
    }

    [Fact]
    public void GetChars_Test()
    {
        char[] charSet = ['a', 'b', 'c', 'd' , 'e', 'f', 'g', 'h'];

        // note that both loops start at two, this is because of the Nil value.`
        for(int maxStringCount = StringAllocatorState.MinMaxStringCount; maxStringCount < charSet.Length; maxStringCount++)
        {
            for(int stride = StringAllocatorState.MinMaxCharacterCount; stride < charSet.Length; stride++)
            {
                bool isValid = false;
                int stringIndex = 0;
                StringAllocatorState state = new(stride, maxStringCount);

                char[] chars;
                int j = 0;
                for(int i = 0; i < maxStringCount-1; i++)
                {
                    chars = new char[stride];
                    for(int q = 0; q < stride; q++)
                    {
                        j+=1;
                        j%=charSet.Length;
                        chars[q] = charSet[j];
                    }
                    StringAllocator.Allocate(state, chars, ref stringIndex);
                    Span<char> retrievedChars = StringAllocator.GetChars(state, stringIndex, ref isValid);
                    Assert.True(isValid);
                    Assert.Equal(chars.AsSpan(), retrievedChars); 
                }

                Debug.Log.Suppress = true;

                StringAllocator.Deallocate(state, stringIndex);
                // fail case.
                Span<char> failChars = StringAllocator.GetChars(state, stringIndex, ref isValid);
                Assert.False(isValid);

                Debug.Log.Suppress = false;
            }
        }
    }
}