using Howl.Math;
using Xunit;

namespace Howl.Test.Math;

public class Soa_Vector2Test
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 25; length++)
        {
            Soa_Vector2 soa = new(length);
            Soa_Vector2Assert.LengthEqual(length, soa);
            Assert.False(soa.Disposed);            
        }
    }

    [Fact]
    public void Insert_Test()
    {
        for(int length = 0; length < 25; length++)
        {
            Soa_Vector2 soa = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                float x = j++;
                float y = j++;
                Soa_Vector2.Insert(soa, i, x, y);
                Soa_Vector2Assert.EntryEqual(x, y, i, soa);
            }
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 5; length++)
        {
            Soa_Vector2 soa = new(length);
            int j = 0;
            for(int i = 0; i < length; i++)
            {
                float x = j++;
                float y = j++;
                Soa_Vector2.Append(soa, x, y);
                Soa_Vector2Assert.EntryEqual(x, y, i, soa);
                Assert.Equal(i+1, soa.AppendCount);
            }
        }
    }

    [Fact]
    public void ResetCount_Test()
    {
        int length = 6;
        Soa_Vector2 soa = new(length);
        for(int i = 0; i < length; i++)
        {
            Soa_Vector2.Append(soa, i, i);
        }
        Assert.Equal(length, soa.AppendCount);
        Soa_Vector2.ResetCount(soa);
        Assert.Equal(0, soa.AppendCount);
    }

    [Fact]
    public void Disposal_Test()
    {
        Soa_Vector2 soa = new(12);
        Soa_Vector2.Dispose(soa);
        Assert.Null(soa.X);
        Assert.Null(soa.Y);
        Assert.Equal(0, soa.Length);
        Assert.True(soa.Disposed);
    }
}