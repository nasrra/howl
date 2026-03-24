using Xunit;
using Howl.DataStructures.Bvh;
using Howl.Test.Math.Shapes;

namespace Howl.Test.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchyTest
{
    [Fact]
    public void Constructor_Test()
    {
        int capacity = 9;
        Soa_BoundingVolumeHierarchy bvh = new(capacity);
    }
}