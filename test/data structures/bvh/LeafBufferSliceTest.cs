using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public class LeafBufferSliceTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 4; length++)
        {
            LeafBuffer buffer = new(length);
            
            // populate buffer.
            int q = 0;
            for(int i = 0 ; i < length; i++)
            {
                LeafBuffer.Append(buffer, q++, q++, q++, q++, q++, q++, q++);
            }

            // construct and assert slices.
            for(int i = 0; i < length; i++)
            {
                LeafBufferSlice slice;

                slice = new(buffer, 0, i);
                for(int j = 0; j < i; j++)
                {
                    LeafBufferSliceAssert.EntryEqual(buffer.Aabbs.MinX[j], buffer.Aabbs.MinY[j], buffer.Aabbs.MaxX[j], buffer.Aabbs.MaxY[j],
                        buffer.GenIndices.Indices[j], buffer.GenIndices.Generations[j], buffer.Flags[j], j, slice
                    );
                }

                slice = new(buffer, i, length-i);
                for(int j = i; j < length-1; j++)
                {
                    int index = j+i;
                    LeafBufferSliceAssert.EntryEqual(buffer.Aabbs.MinX[index], buffer.Aabbs.MinY[index], buffer.Aabbs.MaxX[index], buffer.Aabbs.MaxY[index],
                        buffer.GenIndices.Indices[index], buffer.GenIndices.Generations[index], buffer.Flags[index], j, slice
                    );
                }
            }
        }
    }
}