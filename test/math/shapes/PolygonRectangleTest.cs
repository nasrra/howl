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

    [Fact]
    public void Centroid_Test()
    {
        PolygonRectangle rectangle;
        Vector2 centroid;

        rectangle = new PolygonRectangle(-0.5f, 0.5f, 1f, 1f);
        centroid = new Vector2(0, 0);
        Assert.Equal(centroid, Centroid(rectangle));
        
        rectangle = new PolygonRectangle(
            [
                new Vector2(-10, 5),
                new Vector2(0, 15),
                new Vector2(10, 5),
                new Vector2(0,-5),
            ]
        );
        centroid = new Vector2(0, 5);
        Assert.Equal(centroid, Centroid(rectangle));
    
        rectangle = new PolygonRectangle(
            [
                new Vector2(-20, 10),
                new Vector2(10, 10),
                new Vector2(10, -5),
                new Vector2(-20,-5),
            ]
        );
        centroid = new Vector2(-5, 2.5f);
        Assert.Equal(centroid, Centroid(rectangle));    
    }

    [Fact]
    public void GetAABB_Test()
    {
        PolygonRectangle shape = new PolygonRectangle(-0.5f, 0.5f, 20, 10);
        Aabb aabb = GetAABB(shape);
        Assert.Equal(-0.5f, aabb.MinX, precision: 1);
        Assert.Equal(-9.5f, aabb.MinY, precision: 1);
        Assert.Equal(19.5f, aabb.MaxX, precision: 1);
        Assert.Equal(0.5f, aabb.MaxY, precision: 1);
    }
}