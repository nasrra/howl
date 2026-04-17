using Howl.Ecs;
using Howl.Test.Collections;

namespace Howl.Test.Ecs;

public class Test_GenIndexArray{
    
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 2; length < 7; length++)
        {
            GenIndexArray<float> array = new(length);
            Assert_GenIndexArray.LengthEqual(length, array);
            Assert.Equal(0, array.Count);
            Assert.False(array.Disposed);
        }
    }

    [Fact]
    public void Allocate_Test()
    {
        for(int length = 2; length < 7; length++)
        {
            int totalAllocations = length-1;

            GenIndex genIndex = default;
            GenIndexArray<float> nums = new(length);

            // successful allocation test.
            for(int i = 0; i < totalAllocations; i++)
            {
                int value = i;
                // increment by one as there is a Nil in the collection.
                int numIndex = i+1;
                int flag = i;
                bool allocated = true;

                // allocation should be successful.
                Assert.Equal(GenIndexResult.Ok, GenIndexArray.Allocate(nums, value, ref genIndex, flag));

                // gen index was correctly generated.
                GenIndex expectedGenIndex = new(numIndex, 0);
                Assert_GenIndex.Equal(expectedGenIndex, genIndex);

                // internal count should have incremented.
                Assert.Equal(numIndex, nums.Count);

                // ensure that the entry has been correctly written to.
                Assert_GenIndexArray.EntryEqual(value, expectedGenIndex.Generation, flag, allocated, genIndex.Index, nums);
            }

            // memory limit reached test.
            GenIndex previousGenIndex = genIndex;
            Assert.Equal(GenIndexResult.MemoryLimitHit, GenIndexArray.Allocate(nums, 12, ref genIndex));
            
            // internal count should not have incremented.
            Assert.Equal(totalAllocations, nums.Count);

            // gen index should not have been written to.
            Assert.Equal(previousGenIndex, genIndex);

            // verify that the collection hasnt changed.
            for(int i = 0; i < totalAllocations; i++)
            {
                int value = i;
                // increment by one as there is a Nil in the collection.
                int numIndex = i+1;

                Assert.Equal(value, nums[numIndex]);
            }
        }
    }

    [Fact]
    public void Deallocate_Test()
    {
        // == deallocate then immediaately allocate. == 
        for(int length = 2; length < 7; length++)
        {
            // setup test dataset.
            int totalAllocations = length-1;
            GenIndexArray<float> nums = new(length);
            GenIndex genIndex = default;

            // allocate entries.
            for(int i = 0; i < totalAllocations; i++)
            {
                GenIndexArray.Allocate(nums, i, ref genIndex);
            }

            for(int generation = 0; generation < 6; generation++)
            {
                int nextGeneration = generation+1;
                int freeCount = 0;

                for(int j = length - 1; j > 0; j-=2)
                {
                    float value = j;
                    int flag = j+1;

                    // deallocate should be successful.
                    GenIndex deallocateIndex = new(j, generation);
                    Assert.Equal(GenIndexResult.Ok, GenIndexArray.Deallocate(nums, deallocateIndex));
                    Assert.False(nums.Allocated[j]);
                    
                    // ensure that the slot has been put back into the free slots pool for later reuse.
                    freeCount++;
                    Assert.Equal(freeCount, nums.FreeSlots.Count);

                    // allocate into the newly freed slot.
                    GenIndex allocateIndex = default;
                    Assert.Equal(GenIndexResult.Ok, GenIndexArray.Allocate(nums, value, ref allocateIndex, flag));

                    // ensure generational increment.
                    Assert.Equal(nextGeneration, allocateIndex.Generation);
                    
                    // ensure the entry has been written to.
                    bool allocated = true;
                    Assert_GenIndexArray.EntryEqual(value, allocateIndex.Generation, flag, allocated, allocateIndex.Index, nums);
                
                    // ensure that the slot has been reused and removed from the free slots pool.
                    freeCount--;
                    Assert.Equal(freeCount, nums.FreeSlots.Count);
                }
            }

        }

        // == deallocate all then allocate all. ==
        for(int length = 2; length < 7; length++)
        {
            // setup test dataset.
            int totalAllocations = length-1;
            GenIndexArray<float> nums = new(length);
            GenIndex genIndex = default;

            // allocate entries.
            for(int i = 0; i < totalAllocations; i++)
            {
                GenIndexArray.Allocate(nums, i, ref genIndex);
            }            

            for(int generation = 0; generation < 6; generation++)
            {
                int nextGeneration = generation+1;

                int freeCount = 0;
                for(int j = length - 1; j > 0; j-=2)
                {
                    float value = j;
                    int flag = j+1;

                    // deallocate should be successful.
                    GenIndex deallocateIndex = new(j, generation);
                    Assert.Equal(GenIndexResult.Ok, GenIndexArray.Deallocate(nums, deallocateIndex));
                    Assert.False(nums.Allocated[j]);

                    // ensure that the slot has been put back into the free slots pool for later reuse.
                    freeCount++;
                    Assert.Equal(freeCount, nums.FreeSlots.Count);
                }

                for(int j = length - 1; j > 0; j-=2)
                {
                    float value = j;
                    int flag = j+1;

                    // allocate into the newly freed slot.
                    GenIndex allocateIndex = default;
                    Assert.Equal(GenIndexResult.Ok, GenIndexArray.Allocate(nums, value, ref allocateIndex, flag));

                    // ensure generational increment.
                    Assert.Equal(nextGeneration, allocateIndex.Generation);
                    
                    // ensure the entry has been written to.
                    bool allocated = true;
                    Assert_GenIndexArray.EntryEqual(value, allocateIndex.Generation, flag, allocated, allocateIndex.Index, nums);

                    // ensure that the slot has been reused and removed from the free slots pool.
                    freeCount--;
                    Assert.Equal(freeCount, nums.FreeSlots.Count);
                }
            }
        }
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int length = 2; length < 7; length++)
        {
            // there is a Nil in the collection, so the total number of
            // values that can be added is the length - 1. 
            int nilAdjustedLength = length-1;

            GenIndexArray<float> nums = new(length);
            GenIndex genIndex = default;
            float val = 0;
            for(int i = 0; i < nilAdjustedLength; i++)
            {
                GenIndexArray.Allocate(nums, val, ref genIndex);
            }

            Assert.Equal(nilAdjustedLength, nums.Count);
            Assert.False(nums.Disposed);

            GenIndexArray.Dispose(nums);

            Assert.Equal(0, nums.Count);
            Assert.True(nums.Disposed);
        }
    }
}