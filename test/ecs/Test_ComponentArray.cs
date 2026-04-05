namespace Howl.Test.ECS;

public class Test_ComponentArray
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            ComponentArray<float> nums = new(length);
            Assert_ComponentArray.LengthEqual(length, nums);
            Assert.Equal(0, nums.Count);
            Assert.False(nums.Disposed);
        }
    }

    [Fact]
    public void Allocate_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            ComponentArray<float> nums = new(length);
            
            for(int i = 0; i < length; i++)
            {
                float data = i+1;
                int flags = i+2;
                bool allocated = true;
                ComponentArray.Allocate(nums, i, data, flags);
                Assert_ComponentArray.EntryEqual(data, flags, allocated, i, nums);
                Assert.Equal(i+1, nums.Count);
            }
        }
    }

    [Fact]
    public void Deallocate_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            ComponentArray<float> nums = new(length);

            for(int i = 0; i < length; i++)
            {
                ComponentArray.Allocate(nums, i, i, i);
            }

            for(int i = 0; i < length; i++)
            {
                // successfully deallocate.
                Assert.True(ComponentArray.Deallocate(nums, i));

                // ensure count has been updated.
                Assert.Equal(length-1-i, nums.Count);

                // unsuccessfuly deallocate.
                // entry has already been deallocated.
                Assert.False(ComponentArray.Deallocate(nums, i));                

                // ensure count has remained the same.
                Assert.Equal(length-1-i, nums.Count);
            }
        }
    }

    [Fact]
    public void GetData_Test()
    {
        int maxEntities = 12;
        float data = 0;
        ComponentArray<float> nums = new(maxEntities);
        EntityRegistry entities = new(maxEntities);

        GenId validId = default;
        GenId staleId = new(10,13);
        EntityRegistry.Allocate(entities, ref validId);

        // fail cases.
        Assert.Equal(GenIdResult.StaleGenId, ComponentArray.GetData(nums, entities, staleId, ref data));
        Assert.Equal(GenIdResult.ComponentNotAllocated, ComponentArray.GetData(nums, entities, validId, ref data));
    
        // success cases.
        ComponentArray.Allocate(nums, validId, data);
        Assert.Equal(GenIdResult.Ok, ComponentArray.GetData(nums, entities, validId, ref data));
    }

    [Fact]
    public void ClearCount_Test()
    {
        for(int length = 0; length < 9; length++)
        {
            ComponentArray<float> nums = new(length);
            
            for(int i = 0; i < length; i++)
            {
                ComponentArray.Allocate(nums,i,i,i);
            }

            Assert.Equal(length, nums.Count);
            ComponentArray.ClearCount(nums);
            Assert.Equal(0, nums.Count);
        }
    }

    [Fact]
    public void Disposal_Test()
    {
        for(int length = 0; length < 9; length++)
        {
            ComponentArray<float> nums = new(length);
            
            for(int i = 0; i < length; i++)
            {
                ComponentArray.Allocate(nums,i,i,i);
            }

            ComponentArray.Dispose(nums);

            Assert_ComponentArray.Disposed(nums);
        }
    }
}