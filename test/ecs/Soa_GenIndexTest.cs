using Howl.ECS;

namespace Howl.Test.ECS;

public class Soa_GenIndexTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 25; length++)
        {
            Soa_GenIndex soa = new(length);
            Soa_GenIndexAssert.LengthEqual(length, soa);
            Assert.False(soa.Disposed);
        }
    }

    [Fact]
    public void Insert_Test()
    {
        for(int length = 0; length < 10; length++)
        {
            int q = 0;
            Soa_GenIndex soa = new(length);
            for(int j = 0; j < length; j++)
            {
                int index = q++;
                int gen = q++;

                Soa_GenIndex.Insert(soa, j, index, gen);
                Soa_GenIndexAssert.EntryEqual(index, gen, j, soa);
            } 
        }
    }

    [Fact]
    public void Disposal_Test()
    {   
        Soa_GenIndex soa = new(12);
        Soa_GenIndex.Dispose(soa);
        Assert.Null(soa.Indices);
        Assert.Null(soa.Generations);
        Assert.Equal(0, soa.Length);
        Assert.True(soa.Disposed);
    }
}