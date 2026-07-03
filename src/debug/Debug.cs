using System.Diagnostics;
using System.Runtime.CompilerServices;
using N_Howl.N_CameraSystem;
using N_Howl.N_Graphics;
using N_Howl.N_Math;
using N_Howl.N_Rendering;

namespace N_Howl.N_Debug;
#if DEBUG || DEV
public static unsafe class Debug{

    public const int DefaultWireFrameThickness = 1;
    public const int DefaultCircleVerticeAmount = 24;

    /// <summary>
    ///     draws a wire rect relative to a camera's position.
    /// </summary>
    public static void DrawWireRect(
        Rectangle shape, float zPosition, int layer, int materialIndex, int cameraIndex, float thickness = DefaultWireFrameThickness
    ){
        thickness = CalculateThicknessRelativeToCamera(thickness, zPosition, cameraIndex);
        DrawWireRect(shape, zPosition, layer, materialIndex, thickness);
    }

    public static void DrawWireRect(
        Rectangle shape, float zPosition, int layer, int materialIndex, float thickness = DefaultWireFrameThickness
    ){
        AssertState();


        float topY = shape.Y;
        float leftX = shape.X;
        float rightX = shape.X + shape.Width;
        float bottomY = shape.Y - shape.Height;

        Vector3 topLeft = new(){X = leftX, Y = topY, Z = zPosition};
        Vector3 topRight = new(){X = rightX, Y = topY, Z = zPosition};
        Vector3 bottomRight = new(){X = rightX, Y = bottomY, Z = zPosition};
        Vector3 bottomLeft = new(){X = leftX, Y = bottomY, Z = zPosition};

        DrawLine(topLeft, topRight, layer, materialIndex, thickness);
        DrawLine(topRight, bottomRight, layer, materialIndex, thickness);
        DrawLine(bottomRight, bottomLeft, layer, materialIndex, thickness);
        DrawLine(bottomLeft, topLeft, layer, materialIndex, thickness);
    }

    /// <summary>
    ///     draws a circle relative to a camera's position.
    /// </summary>
    public static void DrawWireCircle(
        Circle shape, float zPosition, int layer, int materialIndex, int cameraIndex, 
        float thickness = DefaultWireFrameThickness, int verticeCount = DefaultCircleVerticeAmount
    ){
        thickness = CalculateThicknessRelativeToCamera(thickness, zPosition, cameraIndex);
        DrawWireCircle(shape, zPosition, layer, materialIndex, thickness, verticeCount);
    }

    public static void DrawWireCircle(
        Circle shape, float zPosition, int layer, int materialIndex, 
        float thickness = DefaultWireFrameThickness, int verticeCount = DefaultCircleVerticeAmount
    ){
        verticeCount = Math.Clamp(verticeCount, 3, int.MaxValue);
        
        float rotation = Math.Tau / verticeCount;
        float sin = Math.Sin(rotation);
        float cos = Math.Cos(rotation);

        float startX = shape.X;
        float startY = shape.Y + shape.Radius;

        for(int i = 0; i < verticeCount; i++){
        
            float relX = startX - shape.X; // remove the circle position as rotation must be around the origin.
            float relY = startY - shape.Y; // remove the circle position as rotation must be around the origin.

            float endX = cos * relX - sin * relY + shape.X; // add back the circle position at the end.
            float endY = sin * relX + cos * relY + shape.Y; // add back the circle position at the end.

            DrawLine(
                new(){X = startX, Y = startY, Z = zPosition}, 
                new(){X = endX, Y = endY, Z = zPosition}, 
                layer, materialIndex, thickness
            );

            startX = endX;
            startY = endY;
        }
    }

    /// <summary>
    ///     Draws a wire poly relative to a camera.
    /// </summary>
    public static void DrawWirePoly(
        System.Span<float> verticesX, System.Span<float> verticesY, float zPosition, int layer, int materialIndex, int cameraIndex, float thickness = DefaultWireFrameThickness
    ){
        thickness = CalculateThicknessRelativeToCamera(thickness, zPosition, cameraIndex);
        DrawWirePoly(verticesX, verticesY, zPosition, layer, materialIndex, thickness);
    }

    public static void DrawWirePoly(
        System.Span<float> verticesX, System.Span<float> verticesY, float zPosition, int layer, int materialIndex, float thickness = DefaultWireFrameThickness
    ){
        int nextIndex;
        int count = verticesX.Length;
        for(int startIndex = 0; startIndex < count; startIndex++){
            nextIndex = (startIndex + 1) % count;
            Vector3 start   = new(){ X = verticesX[startIndex], Y = verticesY[startIndex], Z = zPosition};
            Vector3 end     = new(){ X = verticesX[nextIndex], Y = verticesY[nextIndex], Z = zPosition};
            DrawLine(start, end, layer, materialIndex, thickness);
        }
    }

    public static void DrawLine(
        Vector3 start, Vector3 end, int layer, int materialIndex, float thickness = DefaultWireFrameThickness
    ){
        AssertState();

        Transform transform = default;
        transform.Rotation = Math.GetRotationBetweenPoints(start, end);
        transform.Position = (end + start)*0.5f; 
        transform.Scale.Y = Math.Length(end - start);
        transform.Scale.X = thickness; 

        bool isValidOutput = false;
        SpriteId sprite = Renderer.AllocateOneFrameSprite(layer, ref isValidOutput);
        Renderer.InitSprite(sprite, transform, Colour.White, default, default, 1, materialIndex, true);
    }

    public static float CalculateThicknessRelativeToCamera(
        float baseThickness, float objectZPosition, int cameraIndex
    ){
        ref Camera camera = ref CameraSystem.GetCamera(cameraIndex);
        // calculate the absolute distance from the camera lens plane to the object plane.
        float distance = Math.Abs(objectZPosition - camera.Position.Z);
        float scaling = 1;
        switch(camera.ProjectionType){
            case ProjectionType.Orthographic:
                scaling = distance / camera.OrthographicSize;
            break;
            case ProjectionType.Perspective:
                /**
                    Calculate the actual physical height of the viewport at this depth.
                    using Height = 2 * distance * tan(fov / 2)
                **/
                scaling = distance * Math.Tan(camera.PerspectiveFov * 0.5f);
            break;
        }
        return baseThickness * scaling;
    }

    public static void AssertState(){
        // crash.
        Howl.Debug.Assert(CameraSystem.GlobalState.IsIntialised, "Attempted operation with an uninitialised camera system.");
        // crash.
        Howl.Debug.Assert(Renderer.GlobalState.IsInitialised, "Attempted operation with an uninitialised renderer.");
    }
}
#endif