namespace Howl.Test.Ecs;

public class Test_EntityRegistry
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = EntityRegistry.MinEntityCount; length < EntityRegistry.MinEntityCount + 12; length++)
        {
            EntityRegistry registry = new(length);
            Assert_EntityRegistry.LengthEqual(length, registry);
        }
    }

    [Fact]
    public void Allocate_Test()
    {
        for(int length = EntityRegistry.MinEntityCount; length < EntityRegistry.MinEntityCount + 12; length++)
        {
            int nilAdjustedLength = length - 1;
            GenId genId = default;
            EntityRegistry registry = new(length);

            // successful allocation test.
            for(int i = 0; i < nilAdjustedLength; i++)
            {
                int value = i;

                // allocation should be successful.
                Assert.Equal(GenIdResult.Ok, EntityRegistry.Allocate(registry, ref genId));

                // increment by one as there is a Nil in the collection.
                int entityIndex = i+1;

                // gen id was correctly generated.
                GenId expectedGenId = new(entityIndex, 0);
                Assert.True(expectedGenId == genId);
                
                // ensure that it has been allocated.
                Assert.True(registry.Allocated[GenId.GetIndex(genId)]);
            }

            // memory limit reached test.
            GenId previousGenId = genId;
            Assert.Equal(GenIdResult.MemoryLimitHit, EntityRegistry.Allocate(registry, ref genId));
            
            // gen id should not have been written to.
            Assert.Equal(previousGenId, genId);
        }
    }

    [Fact]
    public void Deallocate_Test()
    {
        // == deallocate then immediaately allocate. == 
        for(int length = EntityRegistry.MinEntityCount; length < EntityRegistry.MinEntityCount + 12; length++)
        {
            int nilAdjustedLength = length - 1;

            // setup test dataset.
            EntityRegistry registry = new(length);
            GenId genId = default;

            // allocate entries.
            for(int i = 0; i < nilAdjustedLength; i++)
            {
                EntityRegistry.Allocate(registry, ref genId);
            }

            for(int generation = 0; generation < 6; generation++)
            {
                int nextGeneration = generation+1;
                int freeCount = 0;

                for(int j = length - 1; j > 0; j-=2)
                {
                    // deallocate should be successful.
                    GenId deallocatedId = new(j, generation);
                    Assert.Equal(GenIdResult.Ok, EntityRegistry.Deallocate(registry, deallocatedId));
                    Assert.False(registry.Allocated[j]);

                    // ensure that the slot has been put back into the free slots pool for later reuse.
                    freeCount++;
                    Assert.Equal(freeCount, registry.FreeSlots.Count);

                    // allocate into the newly freed slot.
                    GenId allocatedId = default;
                    Assert.Equal(GenIdResult.Ok, EntityRegistry.Allocate(registry, ref allocatedId));

                    // ensure generational increment.
                    Assert.Equal(nextGeneration, GenId.GetGeneration(allocatedId));
                    
                    // ensure that it has been allocated.
                    Assert.True(registry.Allocated[GenId.GetIndex(allocatedId)]);
                
                    // ensure that the slot has been reused and removed from the free slots pool.
                    freeCount--;
                    Assert.Equal(freeCount, registry.FreeSlots.Count);
                }
            }

        }

        // == deallocate all then allocate all. ==
        for(int length = EntityRegistry.MinEntityCount; length < EntityRegistry.MinEntityCount + 12; length++)
        {
            // setup test dataset.
            int totalAllocations = length-1;
            EntityRegistry registry = new(length);
            GenId genId = default;

            // allocate entries.
            for(int i = 0; i < totalAllocations; i++)
            {
                EntityRegistry.Allocate(registry, ref genId);
            }            

            for(int generation = 0; generation < 6; generation++)
            {
                int nextGeneration = generation+1;

                int freeCount = 0;
                for(int j = length - 1; j > 0; j-=2)
                {
                    // deallocate should be successful.
                    GenId deallocatedId = new(j, generation);
                    Assert.Equal(GenIdResult.Ok, EntityRegistry.Deallocate(registry, deallocatedId));
                    Assert.False(registry.Allocated[j]);

                    // ensure that the slot has been put back into the free slots pool for later reuse.
                    freeCount++;
                    Assert.Equal(freeCount, registry.FreeSlots.Count);
                }

                for(int j = length - 1; j > 0; j-=2)
                {
                    // allocate into the newly freed slot.
                    GenId allocatedId = default;
                    Assert.Equal(GenIdResult.Ok, EntityRegistry.Allocate(registry, ref allocatedId));

                    // ensure generational increment.
                    Assert.Equal(nextGeneration, GenId.GetGeneration(allocatedId));
                    
                    // ensure that it has been allocated.
                    Assert.True(registry.Allocated[GenId.GetIndex(allocatedId)]);

                    // ensure that the slot has been reused and removed from the free slots pool.
                    freeCount--;
                    Assert.Equal(freeCount, registry.FreeSlots.Count);
                }
            }
        }
    }


    [Fact]
    public void Disposal_Test()
    {
        for(int length = EntityRegistry.MinEntityCount; length < EntityRegistry.MinEntityCount + 12; length++)
        {
            EntityRegistry registry = new(length);
            Assert.False(registry.Disposed);
            EntityRegistry.Dispose(registry);
            Assert_EntityRegistry.Disposed(registry);
        }   
    }
}