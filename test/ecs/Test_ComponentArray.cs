namespace Howl.Test.ECS;

public class Test_ComponentArray
{
    [Fact]
    public void Constructor_Test()
    {
        int start = ComponentArray.MinLength;
        for(int length = start; length < start+8; length++)
        {
            ComponentArray<float> nums = new(length);
            Assert_ComponentArray.LengthEqual(length, nums);
            Assert.False(nums.Disposed);
        }
    }

    [Fact]
    public void Allocate_Test()
    {
        int start = ComponentArray.MinLength;
        for(int length = start; length < start + 8; length++)
        {
            ComponentArray<float> nums = new(length);
            
            EntityRegistry entities = new(length);
            GenId placeholderGenId = default;

            // allocate entities.
            for(int i = 1; i < length; i++)
            {
                EntityRegistry.Allocate(entities, ref placeholderGenId);
            }

            for(int i = 1; i < length; i++)
            {
                int index = i;
                int generation = 0;
                GenId genId = new(index, generation);
                float component = i+1;
                bool allocated = true;

                // allocate the data.
                ComponentArray.Allocate(nums, entities, genId, component);
                
                // ensure the correct data was written.
                Assert_ComponentArray.EntryEqual(component, allocated, i, nums); 
            
                // ensure it is active.
                Assert_ComponentArray.EntryIsActive(genId, nums);
                Assert.Equal(i+1, nums.Active.Count);
            }
        }
    }

    [Fact]
    public void Deallocate_Test()
    {
        int start = ComponentArray.MinLength;
        for(int length = start; length < start+8; length++)
        {
            ComponentArray<float> nums = new(length);
            EntityRegistry entities = new(length);
            GenId genId = default;

            // allocate entities.
            for(int i = 1; i < length; i++)
            {
                EntityRegistry.Allocate(entities, ref genId);
            }

            // allocate entries.
            for(int i = 1; i < length; i++)
            {
                int index = i;
                int generation = 0;
                ComponentArray.Allocate(nums, entities, new(index, generation), i);
            }

            // deallocate entries.
            for(int i = 1; i < length; i++)
            {
                int index = i;
                int generation = 0;
                GenId deallocate = new(index, generation);

                // successfully deallocate.
                Assert.Equal(GenIdResult.Ok, ComponentArray.Deallocate(nums, entities, deallocate));

                // unsuccessfuly deallocate.
                // entry has already been deallocated.
                Assert.Equal(GenIdResult.ComponentNotAllocated, ComponentArray.Deallocate(nums, entities, deallocate));

                // ensure it is no longer active.
                Assert_ComponentArray.EntryIsInactive(deallocate, nums);
                Assert.Equal(length-i, nums.Active.Count);
            }
        }
    }

    [Fact]
    public void GetData_Test()
    {
        int maxEntities = 12;
        float component = 0;
        ComponentArray<float> nums = new(maxEntities);
        EntityRegistry entities = new(maxEntities);
        GenIdResult result = default;

        GenId validId = default;
        GenId staleId = new(10,13);
        EntityRegistry.Allocate(entities, ref validId);

        // fail cases.
        ComponentArray.GetData(nums, entities, staleId, ref result);
        Assert.Equal(GenIdResult.StaleGenId, result);

        ComponentArray.GetData(nums, entities, validId, ref result);
        Assert.Equal(GenIdResult.ComponentNotAllocated, result);
    
        // success cases.
        ComponentArray.Allocate(nums, entities, validId, component);
        ComponentArray.GetData(nums, entities, validId, ref result);
        Assert.Equal(GenIdResult.Ok, result);
    }

    [Fact]
    public void SetActiveState_Test()
    {
        int start = ComponentArray.MinLength;
        for(int length = start; length < 8; length++)
        {
            ComponentArray<float> nums = new(length);
            EntityRegistry entities = new(length);

            GenId placeholderGenId = default;

            // allocate entities.
            for(int i = 1; i < length; i++)
            {
                EntityRegistry.Allocate(entities, ref placeholderGenId);
            }

            for(int i = 1; i < length; i++)
            {
                int index = i;
                int generation = 0;
                GenId genId = new(index, generation);
                float component = i+1;

                // allocate the data.
                ComponentArray.Allocate(nums, entities, genId, component);
            }

            // set inactive.
            for(int i = nums.Active.Count-1; i > 0; i--)
            {
                GenId validId = nums.Active[i];
                GenId staleId = GenId.IncrementGeneration(validId);
                
                // success case.
                Assert.Equal(GenIdResult.Ok, ComponentArray.SetInactive(nums, entities, validId));
                Assert_ComponentArray.EntryIsInactive(validId, nums);

                // ensure count has changed.
                Assert.Equal(i, nums.Active.Count);

                // fail case.
                Assert.Equal(GenIdResult.StaleGenId, ComponentArray.SetInactive(nums, entities, staleId));
                Assert.Equal(GenIdResult.StaleGenId, ComponentArray.SetActive(nums, entities, staleId));

                // ensure count has not changed.
                Assert.Equal(i, nums.Active.Count);

                // ensure the valid entry wasnt changed.
                Assert_ComponentArray.EntryIsInactive(validId, nums);
            }

            for(int i = 1; i < length; i++)
            {
                int index = i;
                int generation = 0;
                GenId validId = new(index, generation);
                GenId staleId = new(index, generation+1);

                // success case.
                Assert.Equal(GenIdResult.Ok, ComponentArray.SetActive(nums, entities, validId));
                Assert_ComponentArray.EntryIsActive(validId, nums);
                
                // ensure count has changed.
                Assert.Equal(i+1, nums.Active.Count);

                // fail case.
                Assert.Equal(GenIdResult.StaleGenId, ComponentArray.SetActive(nums, entities, staleId));
                Assert.Equal(GenIdResult.StaleGenId, ComponentArray.SetInactive(nums, entities, staleId));

                // ensure count has not changed.
                Assert.Equal(i+1, nums.Active.Count);

                // ensure the valid entry wasnt changed.
                Assert_ComponentArray.EntryIsActive(validId, nums);
            }
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        int start = ComponentArray.MinLength;
        for(int length = start; length < start+9; length++)
        {
            ComponentArray<float> nums = new(length);
            EntityRegistry entities = new EntityRegistry(length);

            
            for(int i = 1; i < length; i++)
            {
                int index = 1;
                int generation = 0;
                GenId genId = new(index, generation);
                ComponentArray.Allocate(nums, entities, genId, i);
            }

            ComponentArray.Dispose(nums);

            Assert_ComponentArray.Disposed(nums);
        }
    }
}