using Howl.Collections;

namespace Howl.Test.Collections;

public class Test_CategorisedOverlapArray
{
    public const int MaxElements = 1000;

    [Fact]
    public void BuildChunks_Test()
    {
        CategorisedOverlapArray<float> array = new CategorisedOverlapArray<float>(3, MaxElements);
        array.CategoryLengths[0] = 2;
        array.CategoryLengths[1] = 3;
        array.CategoryLengths[2] = 1;
        
        int[] expectedStartIndices = [0,2,5,6,12,21];

        CategorisedOverlapArray.BuildChunks(array);

        Assert.Equal(expectedStartIndices, array.SubCategoryStartIndices);
    }

    [Fact]
    public void GetElementIndex_Test()
    {
        CategorisedOverlapArray<float> array = new(3, MaxElements);
        array.CategoryLengths[0] = 2;
        array.CategoryLengths[1] = 3;
        array.CategoryLengths[2] = 1;

        int categoryA;
        int categoryB;

        categoryA = 2;
        categoryB = 1;
        Assert.Equal(1, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 2;
        Assert.Equal(1, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 0;
        Assert.Equal(3, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));

        categoryA = 0;
        categoryB = 1;
        Assert.Equal(3, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));

        categoryA = 0;
        categoryB = 0;
        Assert.Equal(5, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));

        categoryA = 1;
        categoryB = 1;
        Assert.Equal(4, CategorisedOverlapArray.GetElementIndex(categoryA, categoryB, array.CategoriesTriangularSum));
    }

    [Fact]
    public void Test()
    {
        int maxOverlaps = 25;
        CategorisedOverlapArray<int> array = new(3, maxOverlaps);
        array.CategoryLengths[0] = 2;
        array.CategoryLengths[1] = 3;
        array.CategoryLengths[2] = 1;
        int[] expectedData = [2,4,1,3,6,8,5,7,9,5,7,9,1,3,5,7,9,1,3,5,7,1,3,5,7];
        int[] expectedSubCounts = [2,3,1,6,9,4];

        CategorisedOverlapArray.BuildChunks(array);

        Assert.True(CategorisedOverlapArray.Append(data: 2, array, categoryA: 2, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 4, array, categoryA: 0, categoryB: 2));
        Assert.False(CategorisedOverlapArray.Append(data: 2, array, categoryA: 2, categoryB: 0));
        Assert.False(CategorisedOverlapArray.Append(data: 4, array, categoryA: 0, categoryB: 2));

        Assert.True(CategorisedOverlapArray.Append(data: 1, array, categoryA: 2, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 3, array, categoryA: 1, categoryB: 2));
        Assert.True(CategorisedOverlapArray.Append(data: 6, array, categoryA: 1, categoryB: 2));
        Assert.False(CategorisedOverlapArray.Append(data: 1, array, categoryA: 2, categoryB: 1));
        Assert.False(CategorisedOverlapArray.Append(data: 3, array, categoryA: 1, categoryB: 2));

        Assert.True(CategorisedOverlapArray.Append(data: 8, array, categoryA: 2, categoryB: 2)); 
        Assert.False(CategorisedOverlapArray.Append(data: 8, array, categoryA: 2, categoryB: 2));

        Assert.True(CategorisedOverlapArray.Append(data: 5, array, categoryA: 1, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 9, array, categoryA: 0, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 5, array, categoryA: 1, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 9, array, categoryA: 0, categoryB: 1));
        Assert.False(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 0));
        Assert.False(CategorisedOverlapArray.Append(data: 9, array, categoryA: 0, categoryB: 1));

        Assert.True(CategorisedOverlapArray.Append(data: 1, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 3, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 5, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 9, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 1, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 3, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 5, array, categoryA: 1, categoryB: 1));
        Assert.True(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 1));
        Assert.False(CategorisedOverlapArray.Append(data: 7, array, categoryA: 1, categoryB: 1));
    
        Assert.True(CategorisedOverlapArray.Append(data: 1, array, categoryA: 0, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 3, array, categoryA: 0, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 5, array, categoryA: 0, categoryB: 0));
        Assert.True(CategorisedOverlapArray.Append(data: 7, array, categoryA: 0, categoryB: 0));
        Assert.False(CategorisedOverlapArray.Append(data: 7, array, categoryA: 0, categoryB: 0));
    
        Assert.Equal(expectedData, array.Data);
        Assert.Equal(expectedSubCounts, array.SubCategoryCounts);

        // get overlaps test.

        Span<int> overlaps;
        int[] expectedOverlaps;

        expectedOverlaps = [2,4];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 2, categoryB: 0);
        Assert.Equal(expectedOverlaps, overlaps);
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 0, categoryB: 2);
        Assert.Equal(expectedOverlaps, overlaps);

        expectedOverlaps = [1,3,6];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 2, categoryB: 1);
        Assert.Equal(expectedOverlaps, overlaps);
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 1, categoryB: 2);
        Assert.Equal(expectedOverlaps, overlaps);

        expectedOverlaps = [8];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 2, categoryB: 2);
        Assert.Equal(expectedOverlaps, overlaps);

        expectedOverlaps = [5,7,9,5,7,9];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 0, categoryB: 1);
        Assert.Equal(expectedOverlaps, overlaps);
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 1, categoryB: 0);
        Assert.Equal(expectedOverlaps, overlaps);

        expectedOverlaps = [1,3,5,7,9,1,3,5,7];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 1, categoryB: 1);
        Assert.Equal(expectedOverlaps, overlaps);

        expectedOverlaps = [1,3,5,7];
        overlaps = CategorisedOverlapArray.GetOverlaps(array, categoryA: 0, categoryB: 0);
        Assert.Equal(expectedOverlaps, overlaps);
    
        // == clear count test ==.

        CategorisedOverlapArray.ClearCounts(array);

        Assert.Equal([0,0,0,0,0,0], array.SubCategoryCounts);
    }
}