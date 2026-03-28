using Howl.Math.Shapes;
using Howl.Test.Simd;

namespace Howl.Test.Math.Shapes;

public class Soa_AabbTest
{
    [Fact]
    public void Contructor_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            Soa_Aabb soa = new(capacity);
            Soa_AabbAssert.LengthEqual(capacity, soa);
            Assert.False(soa.Disposed);
        }
    }

    [Fact]
    public void Insert_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            Soa_Aabb soa = new(capacity);

            int j = 0;
            for(int i = 0; i < capacity; i++)
            {
                float minX = j++;
                float minY = j++;
                float maxX = j++;
                float maxY = j++;
                Soa_Aabb.Insert(soa, i, minX, minY, maxX, maxY);
                Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, i, soa);
            }
        }
    }

    [Fact]
    public void CalculateCentroids_Sisd_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            Soa_Aabb soa = new(capacity);

            float minX = 0f;
            float minY = 0f;
            float maxX = 1f;
            float maxY = 1f;
            
            for(int i = 0; i < capacity; i++)
            {
                float nMinX = minX + i;
                float nMinY = minY + i;
                float nMaxX = maxX + i;
                float nMaxY = maxY + i; 
                Soa_Aabb.Insert(soa, i, nMinX, nMinY, nMaxX, nMaxY);
            }

            float[] cX = new float[capacity];
            float[] cY = new float[capacity];
            Soa_Aabb.CalculateCentroids_Sisd(soa, cX, cY, 0, capacity);

            for(int i = 0; i < capacity; i++)
            {
                Aabb.CalculateCentroid(minX + i, minY + i, maxX + i, maxY + i, out float eX, out float eY);
                Assert.Equal(eX, cX[i]);
                Assert.Equal(eY, cY[i]);
            }
        }        
    }

    [Fact]
    public void CalculateCentroids_Simd_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            Soa_Aabb soa = new(capacity);

            float minX = 0f;
            float minY = 0f;
            float maxX = 1f;
            float maxY = 1f;
            
            for(int i = 0; i < capacity; i++)
            {
                float nMinX = minX + i;
                float nMinY = minY + i;
                float nMaxX = maxX + i;
                float nMaxY = maxY + i; 
                Soa_Aabb.Insert(soa, i, nMinX, nMinY, nMaxX, nMaxY);
            }

            float[] cX = new float[capacity];
            float[] cY = new float[capacity];
            int tailIndex = -1;
            Soa_Aabb.CalculateCentroids_Simd(soa, cX, cY, 0, capacity, ref tailIndex);
            VectorFAssert.TailIndexEqual(capacity, tailIndex);

            for(int i = 0; i < (System.Numerics.Vector<float>.Count * (capacity / System.Numerics.Vector<float>.Count)); i++)
            {
                Aabb.CalculateCentroid(minX + i, minY + i, maxX + i, maxY + i, out float eX, out float eY);
                Assert.Equal(eX, cX[i]);
                Assert.Equal(eY, cY[i]);
            }
        }
    }

    [Fact]
    public void CalculateCentroids_Test()
    {
        for(int capacity = 0; capacity < 24; capacity++)
        {            
            Soa_Aabb soa = new(capacity);

            float minX = 0f;
            float minY = 0f;
            float maxX = 1f;
            float maxY = 1f;
            
            for(int i = 0; i < capacity; i++)
            {
                float nMinX = minX + i;
                float nMinY = minY + i;
                float nMaxX = maxX + i;
                float nMaxY = maxY + i; 
                Soa_Aabb.Insert(soa, i, nMinX, nMinY, nMaxX, nMaxY);
            }

            float[] cX = new float[capacity];
            float[] cY = new float[capacity];
            Soa_Aabb.CalculateCentroids(soa, cX, cY, 0, capacity);

            for(int i = 0; i < capacity; i++)
            {
                Aabb.CalculateCentroid(minX + i, minY + i, maxX + i, maxY + i, out float eX, out float eY);
                Assert.Equal(eX, cX[i]);
                Assert.Equal(eY, cY[i]);
            }
        }        
    }
}