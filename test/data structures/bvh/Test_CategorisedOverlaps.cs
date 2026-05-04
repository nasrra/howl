using Howl.DataStructures.Bvh;
using Howl.Physics;

namespace Howl.Test.DataStructures.Bvh;

public class Test_CategorisedOverlaps
{
    public const int MaxOverlaps = 1000;

    [Fact]
    public void BuildChunks_Test()
    {
        CategorisedOverlaps overlaps = new(3, MaxOverlaps);
        overlaps.CategoryLengths[0] = 2;
        overlaps.CategoryLengths[1] = 3;
        overlaps.CategoryLengths[2] = 1;
        
        int[] expectedStartIndices = [0,2,5,6,12,21];

        CategorisedOverlaps.BuildChunks(overlaps);

        Assert.Equal(expectedStartIndices, overlaps.SubCategoryStartIndices);
    }

    [Fact]
    public void GetElementIndex_Test()
    {
        CategorisedOverlaps overlaps = new(3, MaxOverlaps);
        overlaps.CategoryLengths[0] = 2;
        overlaps.CategoryLengths[1] = 3;
        overlaps.CategoryLengths[2] = 1;

        int categoryA;
        int categoryB;

        categoryA = 2;
        categoryB = 1;
        Assert.Equal(1, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 2;
        Assert.Equal(1, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 0;
        Assert.Equal(3, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));

        categoryA = 0;
        categoryB = 1;
        Assert.Equal(3, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));

        categoryA = 0;
        categoryB = 0;
        Assert.Equal(5, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 1;
        Assert.Equal(4, CategorisedOverlaps.GetElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum));
    }

    [Fact]
    public void Test()
    {
        int maxOverlaps = 25;
        CategorisedOverlaps overlaps = new(3, maxOverlaps);
        overlaps.CategoryLengths[0] = 2;
        overlaps.CategoryLengths[1] = 3;
        overlaps.CategoryLengths[2] = 1;
        int[] expectedOwners = [2,4,1,3,6,8,5,7,9,5,7,9,1,3,5,7,9,1,3,5,7,1,3,5,7];
        int[] expectedOthers = [3,5,2,4,7,9,6,8,1,6,8,1,2,4,6,8,0,2,4,6,8,2,4,6,8];
        int[] expectedSubCounts = [2,3,1,6,9,4];

        CategorisedOverlaps.BuildChunks(overlaps);

        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 2, otherLeafIndex: 3, ownerCategory: 2, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 4, otherLeafIndex: 5, ownerCategory: 0, otherCategory: 2));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 0, otherCategory: 2));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 2, otherCategory: 0));

        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 1, otherLeafIndex: 2, ownerCategory: 2, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 3, otherLeafIndex: 4, ownerCategory: 1, otherCategory: 2));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 6, otherLeafIndex: 7, ownerCategory: 1, otherCategory: 2));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 2, otherCategory: 1));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 1, otherCategory: 2));

        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 8, otherLeafIndex: 9, ownerCategory: 2, otherCategory: 2)); 
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 2, otherCategory: 2));

        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 5, otherLeafIndex: 6, ownerCategory: 1, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 7, otherLeafIndex: 8, ownerCategory: 1, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 9, otherLeafIndex: 1, ownerCategory: 0, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 5, otherLeafIndex: 6, ownerCategory: 1, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 7, otherLeafIndex: 8, ownerCategory: 1, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 9, otherLeafIndex: 1, ownerCategory: 0, otherCategory: 1));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 1, otherCategory: 0));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 0, otherCategory: 1));

        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 1, otherLeafIndex: 2, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 3, otherLeafIndex: 4, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 5, otherLeafIndex: 6, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 7, otherLeafIndex: 8, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 9, otherLeafIndex: 0, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 1, otherLeafIndex: 2, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 3, otherLeafIndex: 4, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 5, otherLeafIndex: 6, ownerCategory: 1, otherCategory: 1));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 7, otherLeafIndex: 8, ownerCategory: 1, otherCategory: 1));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 1, otherCategory: 1));
    
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 1, otherLeafIndex: 2, ownerCategory: 0, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 3, otherLeafIndex: 4, ownerCategory: 0, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 5, otherLeafIndex: 6, ownerCategory: 0, otherCategory: 0));
        Assert.True(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 7, otherLeafIndex: 8, ownerCategory: 0, otherCategory: 0));
        Assert.False(CategorisedOverlaps.AppendOverlap(overlaps, ownerLeafIndex: 0, otherLeafIndex: 0, ownerCategory: 0, otherCategory: 0));
    
        Assert.Equal(expectedOwners, overlaps.OwnerLeafIndices);
        Assert.Equal(expectedOthers, overlaps.OtherLeafIndices);
        Assert.Equal(expectedSubCounts, overlaps.SubCategoryCounts);

        // get overlaps test.

        OverlapInfo info;
        int[] expectedInfoOtherIndices;
        int[] expectedInfoOwnerIndices;

        expectedInfoOwnerIndices = [2,4];
        expectedInfoOtherIndices = [3,5];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 2, categoryB: 0);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 0, categoryB: 2);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);

        expectedInfoOwnerIndices = [1,3,6];
        expectedInfoOtherIndices = [2,4,7];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 2, categoryB: 1);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 1, categoryB: 2);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);

        expectedInfoOwnerIndices = [8];
        expectedInfoOtherIndices = [9];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 2, categoryB: 2);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);

        expectedInfoOwnerIndices = [5,7,9,5,7,9];
        expectedInfoOtherIndices = [6,8,1,6,8,1];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 0, categoryB: 1);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 1, categoryB: 0);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);

        expectedInfoOwnerIndices = [1,3,5,7,9,1,3,5,7];
        expectedInfoOtherIndices = [2,4,6,8,0,2,4,6,8];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 1, categoryB: 1);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);

        expectedInfoOwnerIndices = [1,3,5,7];
        expectedInfoOtherIndices = [2,4,6,8];
        info = CategorisedOverlaps.GetOverlaps(overlaps, categoryA: 0, categoryB: 0);
        Assert.Equal(expectedInfoOwnerIndices, info.OwnerLeafIndices);
        Assert.Equal(expectedInfoOtherIndices, info.OtherLeafIndices);
    
        // == clear count test ==.

        CategorisedOverlaps.ClearCounts(overlaps);

        Assert.Equal([0,0,0,0,0,0], overlaps.SubCategoryCounts);
    }
}