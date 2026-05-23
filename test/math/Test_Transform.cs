using Howl.Math;

namespace Howl.Test.Math;

public class Test_Transform
{
    [Fact]
    public void ConstructorTest_Test()
    {
        Vector2 position = new Vector2(1,2);
        Vector2 scale = new Vector2(1,1);
        float rotation = 45;
        
        Transform transform = new Transform(position, scale, rotation);

        Assert.Equal(position, transform.Position);
        Assert.Equal(scale, transform.Scale);
        Assert.Equal(rotation, transform.RotationRadians);
        Assert.Equal(0.85f, transform.Sine, precision: 2);
        Assert.Equal(MathF.Sin(rotation), transform.Sine, precision: 2);
        Assert.Equal(0.525f, transform.Cosine, precision: 3);
        Assert.Equal(MathF.Cos(rotation), transform.Cosine, precision: 2);
    }

    [Fact]
    public void Translate_Test()
    {
        Vector2 position = new Vector2(10,10);
        Vector2 scale = new Vector2(1,1);
        float rotation = 0;

        Transform transform = new Transform(position, scale, rotation);

        Vector2 translation = new Vector2(13, -2);
        Vector2 expected = position + translation;

        Transform.Translate(ref transform, translation);

        Assert.Equal(expected, transform.Position);
    }

    [Fact]
    public void TranslateTo_Test()
    {
        Vector2 position = new Vector2(20, 20);
        Vector2 scale = new Vector2(1,1);
        float rotation = 0;

        Transform transform = new(position, scale, rotation);

        Vector2 newPosition = new Vector2(-12,12);

        Transform.Warp(ref transform, newPosition);

        Assert.Equal(newPosition, transform.Position);
    }

    [Fact]
    public void SetScale_Test()
    {
        Vector2 position = new Vector2(0,0);
        Vector2 scale = new Vector2(1,1);
        float rotation = 0;

        Transform transform = new Transform(position, scale, rotation);

        Vector2 newScale = new Vector2(-2,12);
        Transform.SetScale(ref transform, newScale);
    
        Assert.Equal(newScale, transform.Scale);
    }
}