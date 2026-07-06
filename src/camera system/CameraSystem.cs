using System.Runtime.CompilerServices;
using Howl;
using N_Howl.N_Collections;
using N_Howl.N_Input;
using N_Howl.N_Math;
using N_Howl.N_Memory;
using N_Howl.N_Windowing;

namespace N_Howl.N_CameraSystem;
public static class CameraSystem{

public static class GlobalState{
    public static Array<Camera> Cameras;
    public static bool IsIntialised;
}

public static void Init(
    ref MemoryArena arena, int cameraCount
){
    GlobalState.IsIntialised = true;
    Collections.Init(ref GlobalState.Cameras, ref arena, cameraCount);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static ref Camera GetCamera(
    int cameraIndex
){
    return ref GlobalState.Cameras[cameraIndex];
}

public static void InitPerspectiveCamera(
    ref Camera camera, Vector3 position, float nearZ, float farZ, float fovInRadians
){
    camera.Position = position;
    Debug.Assert(nearZ >= float.Epsilon, $"Camera should not be initialised with a near-z plane value '{nearZ}'less than float.Epsilon");
    camera.NearZ = Math.Clamp(nearZ, float.Epsilon, float.MaxValue);
    camera.FarZ = farZ;
    camera.PerspectiveFov = fovInRadians;
    camera.ProjectionType = ProjectionType.Perspective;
    camera.IsInitialised = true;
}

public static void InitOrthographicCamera(
    ref Camera camera, Vector3 position, float nearZ, float farZ, float orthogrpahicSize
){
    camera.Position = position;
    Debug.Assert(nearZ >= float.Epsilon, $"Camera should not be initialised with a near-z plane value '{nearZ}'less than float.Epsilon");
    camera.NearZ = Math.Clamp(nearZ, float.Epsilon, float.MaxValue);
    camera.FarZ = farZ;
    camera.OrthographicSize = orthogrpahicSize;
    camera.ProjectionType = ProjectionType.Orthographic;
    camera.IsInitialised = true;
}

public static void UpdateProjectionMatrices(
    ref Camera camera
){
    Debug.Assert(Windowing.GlobalState.IsInitialised, "Attempted to update projection matrices with Windowing uninitialsied.");

    Vector3 cameraPos = new(){X = camera.Position.X, Y = camera.Position.Y, Z = camera.Position.Z}; 
    Vector3 lookAtPos = new(){X = camera.Position.X, Y = camera.Position.Y, Z = camera.Position.Z+0.001f}; 
    Vector3 worldUpDir = new(){Y = 1};
    camera.View = Math.CreateLookAt(cameraPos, lookAtPos, worldUpDir);
    camera.Model = Math.IdentityMatrix();
    float aspectRatio = Windowing.CalculateAspectRatio();

    switch(camera.ProjectionType){
        case ProjectionType.Perspective:
            camera.Projection = Math.CreatePerspective(camera.PerspectiveFov, aspectRatio, camera.NearZ, camera.FarZ);
        break;
        case ProjectionType.Orthographic:
            // Compute half-width and half-height in world units based on virtual resolution
            float halfHeight = camera.OrthographicSize * 0.5f;
            float halfWidth = halfHeight * aspectRatio; // keep aspect ratio correct
            camera.Projection = Math.CreateOrthographic(-halfWidth, halfWidth, -halfHeight, halfHeight, camera.NearZ, camera.FarZ);

        break;
    }
}

public static Vector2 GetMouseOrthographicPosition(
    int cameraIndex
){
    ref Camera camera = ref GlobalState.Cameras[cameraIndex];
    { // validation.
        Howl.Debug.Assert(camera.IsInitialised, "Attempted to get mouse screen position with an uninitialised camera.");
        Howl.Debug.Assert(Windowing.GlobalState.IsInitialised, "Attempted to get mouse screen position with Windowing uninitialised.");
        Howl.Debug.Assert(N_Rendering.Renderer.GlobalState.IsInitialised, "Attempted to get mouse screen position with Renderer uninitialised.");
        Howl.Debug.Assert(camera.ProjectionType == ProjectionType.Orthographic, "Attempted to get the screen position of a non-orthographic camera.");
    }

    Vector2UI windowResolution = Windowing.GetWindowResolution();
    Rectangle dstRect = N_Rendering.Renderer.GetDestinationRectangle();
        
    // get the distance from the destination rect to the mouse position. 
    float x = Input.GlobalState.MouseBackBufferX - dstRect.X;
    float y = Input.GlobalState.MouseBackBufferY - dstRect.Y;

    // NAN guard.
    if(dstRect.Width == 0 || dstRect.Height == 0){
        return default;
    }

    // normalise the value between zero and one, to find
    // how far into the destination rect the mouse is. 
    // - x/y is less than zero, the mouse is outside the destinationRectangle to the left.
    // - x/y is greater than zero, the mouse is outside the destinationRectangle to the right.
    x /= dstRect.Width;
    y /= dstRect.Height;

    // NOTE: you may need to clamp between 0 and 1 here...

    // bring into the destination resolution coordinate space.
    x *= windowResolution.X;
    y *= windowResolution.Y;

    // factor in the difference of output resolution to camera resolution.
    float factor = windowResolution.Y / camera.OrthographicSize;
    x = (x + camera.Position.X) / factor; 
    // - y to convert into rasterised space.
    y = (-y + camera.Position.Y) / factor;

    // translate from rasterised-space into device-coordinate space; because WebGPU is in NDC.
    float halfHeight = camera.OrthographicSize * 0.5f;
    float halfWidth = halfHeight * Windowing.CalculateAspectRatio(); // keep aspect ratio correct
    x -= halfWidth;
    y += halfHeight;

    return new(){ 
        X = x,
        Y = y // negative here as screen space uses rasterised coordinates.
    };
}


}