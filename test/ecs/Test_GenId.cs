namespace Howl.Test.ECS;

public class Test_GenId
{
    [Fact]
    public void Constructor_Test()
    {
        for(int i = 0; i < 14; i++)
        {
            int generation = i+1;
            int index = i;
            GenId id = new(index, generation);
            Assert.Equal(generation, GenId.GetGeneration(id));
            Assert.Equal(index, GenId.GetIndex(id));
        }
    }

    [Fact]
    public void IncrementGeneration_Test()
    {
        int generation = 0;
        int index = 0;
        GenId id = default;

        // successful increments.
        for(int i = 0; i < 12; i++)
        {
            generation = i+1;
            index = i;
            id = new(index, generation);
            id = GenId.IncrementGeneration(id);
            Assert.Equal(generation+1, GenId.GetGeneration(id));
            Assert.Equal(index, GenId.GetIndex(id));
        }

        // wrap around test.
        generation = GenId.MaxGeneration;
        index = 12;
        id = new(index, generation);
        id = GenId.IncrementGeneration(id);
        
        // ensure id is unaffected by wrap around.
        Assert.Equal(index, GenId.GetIndex(id));

        // ensure generation is now zero.
        Assert.Equal(0, GenId.GetGeneration(id));
    }

    [Fact]
    public void IncrementIndex_Test()
    {
        int generation = 0;
        int index = 0;
        GenId id = default;

        // successful increments.
        for(int i = 0; i < 12; i++)
        {
            generation = i+1;
            index = i;
            id = new(index, generation);
            id = GenId.IncrementIndex(id);
            Assert.Equal(generation, GenId.GetGeneration(id));
            Assert.Equal(index+1, GenId.GetIndex(id));
        }

        // wrap around test.
        generation = 13;
        index = GenId.MaxIndex;
        id = new(index, generation);
        id = GenId.IncrementIndex(id);
        
        // ensure generation is unaffected by wrap around.
        Assert.Equal(generation, GenId.GetGeneration(id));

        // ensure index is now zero.
        Assert.Equal(0, GenId.GetIndex(id));
    }

}