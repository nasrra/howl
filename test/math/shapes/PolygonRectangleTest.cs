using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.PolygonRectangle;

namespace Howl.Test.Math.Shapes;

public class PolygonRectangleTest
{
    [Fact]
    public void Constructor_Test()
    {
        PolygonRectangle rectangle;

        Assert.Throws<ArgumentException>(
            () =>
            {
                new PolygonRectangle(
                    [
                        new Vector2(0,0)
                    ]
                );
            }
        );
        Assert.Throws<ArgumentException>(
            () =>
            {
                new PolygonRectangle(
                    [
                        new Vector2(0,0),
                        new Vector2(0,0),
                        new Vector2(0,0),
                        new Vector2(0,0),
                        new Vector2(0,0)
                    ]
                );
            }
        );

        rectangle = new PolygonRectangle(-0.5f,0.5f,1,1);
        
        Span<float> x1 = VerticesXAsSpan(in rectangle);
        Assert.Equal(-0.5f, x1[0]);
        Assert.Equal(0.5f,  x1[1]);
        Assert.Equal(0.5f,  x1[2]);
        Assert.Equal(-0.5f, x1[3]);

        Span<float> y1 = VerticesYAsSpan(in rectangle);
        Assert.Equal(0.5f,  y1[0]);
        Assert.Equal(0.5f,  y1[1]);
        Assert.Equal(-0.5f, y1[2]);
        Assert.Equal(-0.5f, y1[3]);
    
        rectangle = new PolygonRectangle([new Vector2(0,0), new Vector2(1,0), new Vector2(1,-1), new Vector2(0,-1)]);

        Span<float> x2 = VerticesXAsSpan(in rectangle);
        Assert.Equal(0, x2[0]);
        Assert.Equal(1, x2[1]);
        Assert.Equal(1, x2[2]);
        Assert.Equal(0, x2[3]);

        Span<float> y2 = VerticesYAsSpan(in rectangle);
        Assert.Equal(0,     y2[0]);
        Assert.Equal(0,     y2[1]);
        Assert.Equal(-1,    y2[2]);
        Assert.Equal(-1,    y2[3]);

    }
}