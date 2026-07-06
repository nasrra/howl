using Howl.Text;
using N_Howl.N_Graphics;
using N_Howl.N_Math;
using N_Howl.N_Memory;
using N_Howl.N_Windowing;
using WebGPU = N_Howl.N_Rendering.N_WebGpu;

namespace N_Howl.N_Rendering;
public unsafe static class Renderer{

public static class GlobalState{
    public static WebGPU.RendererCtx WebGpuCtx;
    public static bool IsInitialised;
}

static Renderer(){
    Windowing.GlobalState.OnWindowResize = &OnWindowResize; 
}

public static void Init(
    ref MemoryArena arena, RendererCtxInitInfo initInfo
){
    // crash.
    Howl.Debug.Assert(GlobalState.IsInitialised==false, "Attempted to initialise an already initialised renderer.");
    // crash.
    Howl.Debug.Assert(Windowing.GlobalState.IsInitialised, 
        "Attempted to initialise renderer before windowing; windowing should be initialised first."
    );

    WebGPU.Renderer.InitRenderer(
        ref GlobalState.WebGpuCtx, ref arena, initInfo, 
        Windowing.GlobalState.WindowManagerInfo, 
        Windowing.GlobalState.WindowResolution.X, 
        Windowing.GlobalState.WindowResolution.Y
    );
    GlobalState.IsInitialised = true;
}

public static void Draw(

){
    WebGPU.Renderer.DrawRenderer(ref GlobalState.WebGpuCtx);
}

/**##########################################################################################################################################
    div: Textures.
##########################################################################################################################################**/

public static bool LoadImageTexture(
    int virtualTextureId
){
    return WebGPU.Renderer.LoadImageTexture(ref GlobalState.WebGpuCtx, virtualTextureId);
}

public static bool UnloadImageTexture(
    int virtualTextureId
){
    return WebGPU.Renderer.UnloadImageTexture(ref GlobalState.WebGpuCtx, virtualTextureId);
}

public static bool LoadFontTexture(
    int virtualTextureId, uint glyphHeightInPixels
){
    return WebGPU.Renderer.LoadFontTexture(ref GlobalState.WebGpuCtx, virtualTextureId, glyphHeightInPixels);
}

public static void SetVirtualTextureFilePath(
    String filePath, int virtualTextureId
){
    WebGPU.Renderer.SetVirtualTextureFilePath(ref GlobalState.WebGpuCtx, filePath, virtualTextureId);
}

/**##########################################################################################################################################
    div: Buffers.
##########################################################################################################################################**/

public static void WriteToUserUniformBuffer(
    void* uboData, uint sizeOfBufferInBytes
){
    WebGPU.Renderer.WriteToUserUniformBuffer(ref GlobalState.WebGpuCtx, uboData, sizeOfBufferInBytes);
}

public static void WriteToUserStorageBuffer(
    void* uboData, uint sizeOfBufferInBytes
){
    WebGPU.Renderer.WriteToUserStorageBuffer(ref GlobalState.WebGpuCtx, uboData, sizeOfBufferInBytes);
}

/**##########################################################################################################################################
    div: Sprites.
##########################################################################################################################################**/

public static SpriteId AllocSprite(
    int layer, ref bool isValidOutput
){
    return WebGPU.Renderer.AllocSprite(ref GlobalState.WebGpuCtx, layer, ref isValidOutput);
}

public static bool DeallocSprite(
    SpriteId spriteId
){
    return WebGPU.Renderer.DeallocSprite(ref GlobalState.WebGpuCtx, spriteId);
}

public static bool InitSprite(
    SpriteId spriteId, Transform transform, N_Graphics.Colour colour, Region region, 
    ColourState colourState, int virtualTextureIndex, int materialIndex, bool isActive
){
    return WebGPU.Renderer.InitSprite(
        ref GlobalState.WebGpuCtx, spriteId, transform, colour, region, 
        colourState, virtualTextureIndex, materialIndex, isActive
    );
}

public static SpriteId AllocSpriteChain(
    int chainLength, int layer, ref bool isValidOutput
){
    return WebGPU.Renderer.AllocSpriteChain(ref GlobalState.WebGpuCtx, chainLength, layer, ref isValidOutput);
}

public static bool DeallocSpriteChain(
    SpriteId spriteId
){
    return WebGPU.Renderer.DeallocSpriteChain(ref GlobalState.WebGpuCtx, spriteId);
}

public static bool InitSpriteString(
    SpriteId spriteId, String text, Transform transform, int virtualTextureId, int materialId, bool isActive
){
    return WebGPU.Renderer.InitSpriteString(ref GlobalState.WebGpuCtx, spriteId, text, transform, virtualTextureId, materialId, isActive);
}

public static SpriteId AllocateOneFrameSprite(
    int layer, ref bool isValidOutput
){
    return WebGPU.Renderer.AllocOneFrameSprite(ref GlobalState.WebGpuCtx, layer, ref isValidOutput);
}

public static bool SetSpriteTransform(
    SpriteId spriteId, Transform transform
){
    return WebGPU.Renderer.SetSpriteTransform(ref GlobalState.WebGpuCtx, spriteId, transform);
}

public static bool SetSpriteVirtualTexture(
    SpriteId spriteId, int virtualTextureId
){
    return WebGPU.Renderer.SetSpriteVirtualTexture(ref GlobalState.WebGpuCtx, spriteId, virtualTextureId);
}

public static bool SetSpriteMaterial(
    SpriteId spriteId, int materialId
){
    return WebGPU.Renderer.SetSpriteMaterial(ref GlobalState.WebGpuCtx, spriteId, materialId);
}

public static bool SetSpriteRegion(
    SpriteId spriteId, Region region
){
    return WebGPU.Renderer.SetSpriteRegion(ref GlobalState.WebGpuCtx, spriteId, region);
}

public static bool SetSpriteColour(
    SpriteId spriteId, Colour colour
){
    return WebGPU.Renderer.SetSpriteColour(ref GlobalState.WebGpuCtx, spriteId, colour);
}

public static bool SetSpriteStringTransform(
    SpriteId spriteId, Transform transform
){
    return WebGPU.Renderer.SetSpriteStringTransform(ref GlobalState.WebGpuCtx, spriteId, transform);
}

public static SpriteType GetSpriteType(
    SpriteId spriteId, ref bool isValidOutput 
){
    return WebGPU.Renderer.GetSpriteType(ref GlobalState.WebGpuCtx, spriteId, ref isValidOutput);
}

/**##########################################################################################################################################
    div: Final Render Texture. 
##########################################################################################################################################**/

public static Rectangle GetDestinationRectangle(

){
    return GlobalState.WebGpuCtx.DestinationRectangle;
}

public static Rectangle CalculateDestinationRectangle(
    uint srcWidth, uint srcHeight, uint dstWidth, uint dstHeight
)
{
    // Rectangle backbufferBounds = MonoGameAppState.GraphicsDevice.PresentationParameters.Bounds;
    float backbufferAspectRatio = (float)dstWidth / dstHeight;
    float renderTargetAspectRatio = (float)srcWidth / srcHeight;

    // scale the image to fit into the window's back buffer.
    Rectangle rect = default;
    rect.X = 0;
    rect.Y = 0f;
    rect.Width = dstWidth;
    rect.Height = dstHeight;

    // stretch image (render target) width to fit on the window's back buffer.
    if(backbufferAspectRatio > renderTargetAspectRatio)
    {
        rect.Width = rect.Height * renderTargetAspectRatio;
        rect.X = ((float)dstWidth - rect.Width) * 0.5f;
    }

    // shrink image (render target) height to fit on the window's back buffer.
    else if (backbufferAspectRatio < renderTargetAspectRatio)
    {
        rect.Height = rect.Width / renderTargetAspectRatio;
        rect.Y = ((float)dstHeight - rect.Height) * 0.5f;
    }

    return rect;
}

public static float GetFinalTextureAspectRatio(

){
    return WebGPU.Renderer.GetFinalRenderTextureAspectRatio(GlobalState.WebGpuCtx);
}

/**##########################################################################################################################################
    div: Windowing.
##########################################################################################################################################**/

public static void OnWindowResize(){
    if(GlobalState.IsInitialised==false){
        return;
    }
    WebGPU.Renderer.HandleWindowResize(ref GlobalState.WebGpuCtx, Windowing.GetWindowResolution());
}

}