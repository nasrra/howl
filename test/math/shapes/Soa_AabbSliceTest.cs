using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class Soa_AabbSliceTest
{
    [Fact]
    public void Constructor_Test()
    {
        int length = 25;
        Soa_Aabb soa = new(length);


        // populate soa.
        int q = 0; 
        for(int i = 0; i < length; i++)
        {
            Soa_Aabb.Insert(soa, i, q++, q++, q++, q++);
        }

        // construct and assert slices.
        for(int i = 0; i < length; i++)
        {
            Soa_AabbSlice slice;

            slice = new(soa, 0, i);
            for(int j = 0; j < i; j++)
            {
                Soa_AabbSliceAssert.EntryEqual(soa.MinX[j], soa.MinY[j], soa.MaxX[j], soa.MaxY[j], j, slice);
            }

            slice = new(soa, i, length-i);
            for(int j = 0; j < length-i; j++)
            {
                int index = j+i;
                Soa_AabbSliceAssert.EntryEqual(soa.MinX[index], soa.MinY[index], soa.MaxX[index], 
                    soa.MaxY[index], j, slice
                );                
            }
        }
    }
}