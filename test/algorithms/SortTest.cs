using Howl.Algorithms;

namespace Howl.Test.Algorithms;

public class SortTest
{
    [Fact]
    public void FloatToUintSortable_Test(){
        
        float additive = 1.12397f;             
        
        for(int i = 0; i < 10; i++)
        {
            additive += additive;
            
            // the floating values to convert.
            float fA = -123.34f + additive;
            float fB = -3.123f + additive;
            float fC = 2.1239f + additive;
            float fD = 98.98f + additive;
            
            // convert to uint.
            uint uA = Sort.FloatToUintSortable(fA);
            uint uB = Sort.FloatToUintSortable(fB);
            uint uC = Sort.FloatToUintSortable(fC);
            uint uD = Sort.FloatToUintSortable(fD);

            // test uints.
            Assert.True((uA < uB) && (uA < uC) && (uA < uD));
            Assert.True((uB > uA) && (uB < uC) && (uB < uD));
            Assert.True((uC > uA) && (uC > uB) && (uC < uD));
            Assert.True((uD > uA) && (uD > uB) && (uD > uC));

            // convert back to float.
            Assert.Equal(fA, Sort.UintSortableToFloat(uA));
            Assert.Equal(fB, Sort.UintSortableToFloat(uB));
            Assert.Equal(fC, Sort.UintSortableToFloat(uC));
            Assert.Equal(fD, Sort.UintSortableToFloat(uD));
        }
    }
}