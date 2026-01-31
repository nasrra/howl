using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class Polygon16Test
{
    [Fact]
    public unsafe void Contructor_Test()
    {
        Vector2 vertex1 = new(0,0);
        Vector2 vertex2 = new(1,1);
        Vector2 vertex3 = new(3,-3);

        Polygon16 polygon = new Polygon16([vertex1, vertex2, vertex3]);
        Assert.Equal(3, polygon.VerticesCount);
        Assert.Equal(vertex1, new Vector2(polygon.XVertices[0], polygon.YVertices[0]));
        Assert.Equal(vertex2, new Vector2(polygon.XVertices[1], polygon.YVertices[1]));
        Assert.Equal(vertex3, new Vector2(polygon.XVertices[2], polygon.YVertices[2]));
    }
}