using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.Math.Shapes;

public class PolygonRectangleTest
{
    [Fact]
    public void Constructor_Test()
    {
        PolygonRectangle rectangle;
        Span<float> xVerts;
        Span<float> yVerts;

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
        
        xVerts = rectangle.GetVerticesXAsSpan();
        Assert.Equal(-0.5f, xVerts[0]);
        Assert.Equal(0.5f,  xVerts[1]);
        Assert.Equal(0.5f,  xVerts[2]);
        Assert.Equal(-0.5f, xVerts[3]);

        yVerts = rectangle.GetVerticesYAsSpan();
        Assert.Equal(0.5f,  yVerts[0]);
        Assert.Equal(0.5f,  yVerts[1]);
        Assert.Equal(-0.5f, yVerts[2]);
        Assert.Equal(-0.5f, yVerts[3]);
    
        rectangle = new PolygonRectangle([new Vector2(0,0), new Vector2(1,0), new Vector2(1,-1), new Vector2(0,-1)]);

        xVerts = rectangle.GetVerticesXAsSpan();
        Assert.Equal(0, xVerts[0]);
        Assert.Equal(1, xVerts[1]);
        Assert.Equal(1, xVerts[2]);
        Assert.Equal(0, xVerts[3]);

        yVerts = rectangle.GetVerticesYAsSpan();
        Assert.Equal(0,     yVerts[0]);
        Assert.Equal(0,     yVerts[1]);
        Assert.Equal(-1,    yVerts[2]);
        Assert.Equal(-1,    yVerts[3]);

    }
}