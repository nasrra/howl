using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class Test_Soa_Overlap
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 7; length++)
        {
            Soa_Overlap soa = new(length);
            Assert_Soa_Overlap.LengthEqual(length, soa);
            Assert.False(soa.Disposed);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 7; length++)
        {
            Soa_Overlap soa = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                int owner = j++;
                int other = j++;
                Soa_Overlap.Append(soa, owner, other);
                Assert_Soa_Overlap.EntryEqual(owner, other, i, soa);
                Assert.Equal(i+1, soa.AppendCount);
            }
        }
    }

    [Fact]
    public void ClearAppendCount_Test()
    {
        for(int length = 0; length < 7; length++)
        {
            Soa_Overlap soa = new(length);
            for(int i = 0; i < length; i++)
            {
                Soa_Overlap.Append(soa, i, i);
            }
            Assert.Equal(length, soa.AppendCount);

            Soa_Overlap.ClearAppendCount(soa);

            Assert.Equal(0, soa.AppendCount); 
        }        
    }

    [Fact]
    public void Dispose_Test()
    {
        int length = 12;
        Soa_Overlap soa = new(length);
        for(int i = 0; i < length; i++)
        {
            Soa_Overlap.Append(soa, i, i);
        }
        Assert.Equal(length, soa.AppendCount);

        Soa_Overlap.Dispose(soa);

        Assert.Null(soa.OwnerLeafIndices);
        Assert.Null(soa.OtherLeafIndices);
        Assert.Equal(0, soa.AppendCount);
        Assert.Equal(0, soa.Length);
        Assert.True(soa.Disposed);
    }
}