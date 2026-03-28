using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math;

public class Soa_Vector2SliceTest
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 25; length++)
        {
            Soa_Vector2 soa = new(length);

            // populate soa.
            int q = 0;
            for(int i = 0; i < length; i++)
            {
                Soa_Vector2.Insert(soa, i, q++, q++);
            }

            // construct and assert slices.
            for(int i = 0; i < length; i++)
            {
                Soa_Vector2Slice slice;

                slice = new(soa, 0, i);
                for(int j = 0; j < i; j++)
                {
                    Soa_Vector2SliceAssert.EntryEqual(soa.X[j], soa.Y[j], j, slice);
                }

                slice = new(soa, i, length-i);
                for(int j = i; j < length-i; j++)
                {
                    int index = j+i;
                    Soa_Vector2SliceAssert.EntryEqual(soa.X[index], soa.Y[index], j, slice);
                }
            }
        }
    }
}