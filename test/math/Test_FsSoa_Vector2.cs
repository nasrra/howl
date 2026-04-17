using Howl.Math;

namespace Howl.Test.Math;

public class Test_FsSoa_Vector2
{
    [Fact]
    public void Constructor_Test()
    {
        for(int maxEntries = 0; maxEntries < 12; maxEntries++)
        {
            for(int stride = 0; stride < 6; stride++)
            {
                FsSoa_Vector2 soa = new(stride, maxEntries);
                Assert_FsSoa_Vector2.LengthEqual(stride, maxEntries, soa);
                Assert.False(soa.Disposed);        
            }
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int maxEntries = 0; maxEntries < 6; maxEntries++)
        {
            for(int stride = 0; stride < 6; stride++)
            {
                FsSoa_Vector2 soa = new(stride, maxEntries);
                float j = 0;
                for(int entryIndex = 0; entryIndex < maxEntries; entryIndex++)
                {
                    for(int elementIndex = 0; elementIndex < stride; elementIndex++)
                    {                        
                        float x = j++;
                        float y = j++;
                        FsSoa_Vector2.Append(soa, entryIndex, x, y);
                        Assert_FsSoa_Vector2.ElementEqual(x, y, elementIndex, entryIndex, soa);
                        Assert.Equal(elementIndex+1, soa.AppendCounts[entryIndex]);
                    }
                }
            }
        }
    }

    [Fact]
    public void ClearEntryAppendCount_Test()
    {
        for(int maxEntries = 0; maxEntries < 6; maxEntries++)
        {
            for(int stride = 0; stride < 6; stride++)
            {
                FsSoa_Vector2 soa = new(stride, maxEntries);                
                // append test values.
                for(int entryIndex = 0; entryIndex < maxEntries; entryIndex++)
                {
                    for(int elementIndex = 0; elementIndex < stride; elementIndex++)
                    {                        
                        FsSoa_Vector2.Append(soa, entryIndex, stride, stride);
                    }
                }

                // remove clear them.
                for(int entryIndex = 0; entryIndex < maxEntries; entryIndex += 2)
                {
                    Assert.Equal(stride, soa.AppendCounts[entryIndex]);                        
                    FsSoa_Vector2.ClearEntryAppendCount(soa, entryIndex);
                    Assert.Equal(0, soa.AppendCounts[entryIndex]);
                }
            }
        }        
    }

    [Fact]
    public void Dispose_Test()
    {
        FsSoa_Vector2 soa = new(12,12);
        FsSoa_Vector2.Dispose(soa);
        Assert_FsSoa_Vector2.Disposed(soa);
    }
}