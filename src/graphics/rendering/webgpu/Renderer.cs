using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Howl;
using Howl.Text;
using N_Howl.N_Collections;
using N_Howl.N_Font;
using N_Howl.N_Graphics;
using N_Howl.N_Math;
using N_Howl.N_Windowing;
using Silk.NET.Core.Native;
using Debug = Howl.Debug;
using WebGPU = Silk.NET.WebGPU;
using WebGPUStatic = Silk.NET.WebGPU.WebGPU;
using WGpu = Silk.NET.WebGPU.Extensions.WGPU;

namespace N_Howl.N_Rendering.N_WebGpu;
public unsafe static class Renderer{

/******************

    utility shaders.

*******************/

public const string blitShaderCode = """ 
struct VertexOutput {
    @builtin(position) position : vec4<f32>,
    @location(0) uv : vec2<f32>,
}

@vertex
fn vs_main(@builtin(vertex_index) vertexIndex : u32) -> VertexOutput {
    // Traditional full-screen quad using 4 vertices
    var pos = array<vec2<f32>, 4>(
        vec2<f32>(-1.0,  1.0), // Top-Left
        vec2<f32>(-1.0, -1.0), // Bottom-Left
        vec2<f32>( 1.0,  1.0), // Top-Right
        vec2<f32>( 1.0, -1.0)  // Bottom-Right
    );
    var uv = array<vec2<f32>, 4>(
        vec2<f32>(0.0, 0.0),
        vec2<f32>(0.0, 1.0),
        vec2<f32>(1.0, 0.0),
        vec2<f32>(1.0, 1.0)
    );

    var output : VertexOutput;
    output.position = vec4<f32>(pos[vertexIndex], 0.0, 1.0);
    output.uv = uv[vertexIndex];
    return output;
}

@group(0) @binding(0) var textureSampler : sampler;
@group(0) @binding(1) var sourceTexture : texture_2d<f32>;

@fragment
fn fs_main(@location(0) uv : vec2<f32>) -> @location(0) vec4<f32> {
    return textureSample(sourceTexture, textureSampler, uv);
}
""";

/******************

    statics.

*******************/

public static WebGPUStatic WebGPUApi = WebGPUStatic.GetApi();
public static WGpu.Wgpu WGpuApi;
// public static unsafe byte* VertShaderEntryPoint = (byte*)"vs_main\0";
public static System.ReadOnlySpan<byte> VertShaderEntryPoint => "vs_main"u8;
public static System.ReadOnlySpan<byte> FragShaderEntryPoint => "fs_main"u8;

/******************

    functions.

*******************/

static Renderer(){
    if(WebGPUApi.TryGetDeviceExtension(null, out WGpuApi) != true){
        Debug.Panic("Failed to load WebGpu Extension: WGPU");
    }

}

public static void InitRenderer(
    ref RendererCtx ctx, ref Memory.Arena arena, RendererCtxInitInfo info, 
    WindowManagerInfo windowInfo, uint windowWidth, uint windowHeight
){
    // validation steps.
    Debug.Assert(ctx.IsInitialised == false, 
        $"Web GPU cannot initialise an already intialised renderer."
    );
    Debug.Assert(info.ImageTextureInitInfos.IsInitialised == true, 
        $"Web GPU cannot initialise a renderer with no texture create infos."
    );
    Debug.Assert(info.SpriteLayerCreateInfos.IsInitialised == true, 
        $"Web GPU cannot initialise a renderer with no sprite layer create infos."
    );

    WebGPU.InstanceDescriptor desc = default; 
    desc.NextInChain = null;
    ctx.Instance = WebGPUApi.CreateInstance(&desc);
    if(ctx.Instance == null){
        Debug.Panic("Failed to create web gpu instance.");
    }
    Memory.Arena.Initialise(ref ctx.TransientArena, ref arena, info.TransientArenaSizeInBytes);
    // order matters here, requesting adapters depends upon the window surface.
    WGpu.InstanceEnumerateAdapterOptions options = default;
    RequestAdapters(ref ctx, ref arena, &options);
    RequestDevices(ref ctx, ref arena);
    // TODO: CHOOSE THE MOST APPROPRIATE DEVICE HERE.
    ref Device device = ref GetChosenDevice(ref ctx);
    ref Adapter adapter = ref GetChosenAdapter(ref ctx);
    // surface configuration must be done at the end of the program.
    InitVirtualTextureManager(
        ref ctx.VirtualTextureManager, device, ref arena, info.ImageTextureInitInfos, info.FontTexturesInitInfo, 
        info.MaxVirtualTextures, info.MaxFilePathLength
    );
    InitSpriteManager(ref ctx.SpriteManager, device, ref arena, info.SpriteLayerCreateInfos);
    InitVertexBuffer(ref ctx);
    InitIndexBuffer(ref ctx, ctx.SpriteManager.Sprites.Length);
    InitUserUniformBuffer(ref ctx, info.MaxUserUniformBufferSizeInBytes);
    InitUserStorageBuffer(ref ctx, info.MaxUserStorageBufferSizeInBytes);
    LinkToWindow(ref ctx, windowInfo, windowWidth, windowHeight);
    InitFinalRenderTarget(ref ctx, info.FinalRenderTextureWidth, info.FinalRenderTextureHeight);
    InitBlitPipeline(ref ctx);
    InitGraphicsPipeline(
        ref ctx, info.GraphicsPipelineShaderFilePath, 
        info.MaxUserUniformBufferSizeInBytes, info.MaxUserStorageBufferSizeInBytes
    );
    UpdateDestinationRectangle(ref ctx);
    ctx.IsInitialised = true;
}

public static void DrawRenderer(
    ref RendererCtx ctx
){

    /**========================================
        VALIDATION. 
    ========================================**/
    {
        // crash.
        Debug.Assert(ctx.IsInitialised, 
            "Web GPU cannot draw an unintialised renderer instance."
        );
        AssertInitialisedWindowSurface(ctx.WindowSurface);
    }

    ref Device device = ref GetChosenDevice(ref ctx);
    ref Adapter adapter = ref GetChosenAdapter(ref ctx);
    SurfaceTexture swapChainTexture = GetNextSwapChainImageView(ref ctx); 

    /**========================================
        UNIFORM PREPERATION.
    ========================================**/    
    WriteToBuffer(device, ctx.VirtualTextureManager.VirtualTextures, ref ctx.VirtualTextureManager.VirtualTextureBuffer);
    /**
        TODO:
        This may have to be optimised out later for a compute buffer operation to sort sprites; so that it is faster.
        but that depends entirely upon how many sprites the game is actually going to have.
    **/
    // prepare for sorting.
    System.Span<Sprite> span = new(ctx.SpriteManager.SortedSprites.Pointer, ctx.SpriteManager.SortedSprites.Length);
    Collections.ClearZeroed(ctx.SpriteManager.SortedSprites);
    Memory.Copy<Sprite>((byte*)ctx.SpriteManager.Sprites.Pointer, (byte*)ctx.SpriteManager.SortedSprites.Pointer, ctx.SpriteManager.Sprites.Length);
    /**
        sort sprites by their z position within their local layer groups.

        Example Output:
        ----------------------------------
        | Sprite Id | Z Position | Layer |
        ----------------------------------
        | 0         | 0          | 0     |
        | 1         | 1          | 0     |
        | 2         | 2          | 0     |
        | 3         | -1         | 1     |
        | 4         | 23         | 1     |
        | 5         | 43         | 1     |
        | 6         | -99        | 2     |
        ----------------------------------
    **/
    int ptrOffset = 0;
    for(int i = 0; i < ctx.SpriteManager.SpriteLayers.Length; i++){
        ref SpriteLayer layer = ref ctx.SpriteManager.SpriteLayers[i];
        void* ptr = ctx.SpriteManager.SortedSprites.Pointer + ptrOffset;
        System.Span<Sprite> spriteSpan = new(ptr, layer.MaxSprites);
        System.MemoryExtensions.Sort(spriteSpan);
        // note; add just the stride as C# automatically incrrements by sizeof(T) for typed pointers (Sprite*).
        ptrOffset += layer.MaxSprites;
    }
    WriteToBuffer(device, ctx.SpriteManager.SortedSprites, ref ctx.SpriteManager.SpriteBuffer);

    /**========================================
        COMMAND ENCODER CREATION.
    ========================================**/    
    WebGPU.CommandEncoderDescriptor cmdEncoderDesc = default;
    WebGPU.CommandEncoder* cmdEncoder = WebGPUApi.DeviceCreateCommandEncoder(device.Pointer, &cmdEncoderDesc);
    
    /**========================================
        RENDER PASS 1: FINAL RENDER TEXTURE.
    ========================================**/
    { 
        
        /**========================================
            COLOUR ATTACHMENT
        ========================================**/
        WebGPU.RenderPassColorAttachment colourAtt = default;
        // render to the final render target image view.
        // crash:
        Debug.Assert(ctx.FinalRenderTexture.IsIntialised, "Attempted render pass initialiasation with an uninitialised final render texture.");
        colourAtt.View = ctx.FinalRenderTexture.View;
        colourAtt.LoadOp = WebGPU.LoadOp.Clear;
        colourAtt.StoreOp = WebGPU.StoreOp.Store;
        colourAtt.ClearValue = new(){ R = 0.01, G = 0.01, B = 0.01, A = 1 };
        
        /**========================================
            DEPTH ATTACHMENT
        ========================================**/
        WebGPU.RenderPassDepthStencilAttachment depthAtt = default;
        // crash.
        Debug.Assert(ctx.DepthTexture.IsIntialised, "Attempted render pass initialisation with an uninitialised depth texture.");
        depthAtt.View = ctx.DepthTexture.View;
        // the initial value of the depth buffer, 1 = "far".
        depthAtt.DepthClearValue = 1.0f;
        depthAtt.DepthLoadOp = WebGPU.LoadOp.Clear;
        depthAtt.DepthStoreOp = WebGPU.StoreOp.Store;
        depthAtt.DepthReadOnly = false;
        // stencil setup is mandatory but unused.
        depthAtt.StencilClearValue = 0;
        depthAtt.StencilLoadOp = WebGPU.LoadOp.Clear;
        depthAtt.StencilStoreOp = WebGPU.StoreOp.Store;
        depthAtt.StencilReadOnly = false;

        /**========================================
            RENDER PASS CREATION.
        ========================================**/        
        WebGPU.RenderPassDescriptor renderPassDesc = default;
        renderPassDesc.ColorAttachmentCount = 1;
        renderPassDesc.ColorAttachments = &colourAtt;
        renderPassDesc.DepthStencilAttachment = &depthAtt;
        renderPassDesc.TimestampWrites = null;
        WebGPU.RenderPassEncoder* renderPass = WebGPUApi.CommandEncoderBeginRenderPass(cmdEncoder, &renderPassDesc);

        /**========================================
            RENDER PASS ENCODING.
        ========================================**/
        WebGPUApi.RenderPassEncoderSetPipeline(renderPass, ctx.GraphicsPipeline.RenderPipeline);
        WebGPUApi.RenderPassEncoderSetVertexBuffer(renderPass, 0, ctx.VertexBuffer.Device, 0, ctx.VertexBuffer.CountInBytes);
        WebGPUApi.RenderPassEncoderSetIndexBuffer(renderPass, ctx.IndexBuffer.Device, WebGPU.IndexFormat.Uint32, 0, ctx.IndexBuffer.CountInBytes);
        WebGPUApi.RenderPassEncoderSetBindGroup(renderPass, 0, ctx.GraphicsPipeline.BindGroup0, 0, null);
        WebGPUApi.RenderPassEncoderSetBindGroup(renderPass, 1, ctx.GraphicsPipeline.BindGroup1, 0, null);
        WebGPUApi.RenderPassEncoderSetBindGroup(renderPass, 2, ctx.GraphicsPipeline.BindGroup2, 0, null);
        WebGPUApi.RenderPassEncoderDrawIndexed(
            renderPass, indexCount: 6, instanceCount: (uint)ctx.SpriteManager.Sprites.Length, firstIndex: 0, baseVertex: 0, firstInstance: 0
        );
        WebGPUApi.RenderPassEncoderEnd(renderPass);
        WebGPUApi.RenderPassEncoderRelease(renderPass);
    }

    /**========================================
        RENDER PASS 2: BLIT TO BACK BUFFER.
    ========================================**/
    { 

        /**========================================
            BIND GROUP CREATION.
        ========================================**/
        /**
            Dynamically create per-frame bind groups, linking the offscreen texture to the current frame's swap chain texture.
        **/
            /**========================================
                ENTRIES
            ========================================**/
            WebGPU.BindGroupEntry* entries = stackalloc WebGPU.BindGroupEntry[2];
            ref WebGPU.BindGroupEntry samplerEntry = ref entries[BlitPipeline.SamplerBinding];
            samplerEntry.Binding = BlitPipeline.SamplerBinding;
            samplerEntry.Sampler = ctx.BlitPipeline.Sampler;
            ref WebGPU.BindGroupEntry textureEnty = ref entries[BlitPipeline.TextureBinding];
            textureEnty.Binding = BlitPipeline.TextureBinding;
            // source texture.
            textureEnty.TextureView = ctx.FinalRenderTexture.View;
            /**========================================
                GROUP
            ========================================**/
            WebGPU.BindGroupDescriptor bindGroupDesc = default;
            bindGroupDesc.Layout = ctx.BlitPipeline.BindGroupLayout;
            bindGroupDesc.EntryCount = BlitPipeline.BindGroupEntryCount;
            bindGroupDesc.Entries = entries;
            WebGPU.BindGroup* bindGroup = WebGPUApi.DeviceCreateBindGroup(device.Pointer, ref bindGroupDesc);

        /**========================================
            COLOUR ATTACHMENT.
        ========================================**/
        WebGPU.RenderPassColorAttachment colourAtt = default;
        // destination texture.
        colourAtt.View = swapChainTexture.View;
        colourAtt.LoadOp = WebGPU.LoadOp.Clear;
        colourAtt.StoreOp = WebGPU.StoreOp.Store;

        /**========================================
            RENDER PASS CREATION.
        ========================================**/        
        WebGPU.RenderPassDescriptor renderPassDesc = default;
        renderPassDesc.ColorAttachmentCount = 1;
        renderPassDesc.ColorAttachments = &colourAtt;
        renderPassDesc.DepthStencilAttachment = null;
        renderPassDesc.TimestampWrites = null;
        WebGPU.RenderPassEncoder* renderPass = WebGPUApi.CommandEncoderBeginRenderPass(cmdEncoder, &renderPassDesc);
        
        /**========================================
            RENDER PASS ENCODING.
        ========================================**/                
        WebGPUApi.RenderPassEncoderSetPipeline(renderPass, ctx.BlitPipeline.RenderPipeline);
        WebGPUApi.RenderPassEncoderSetBindGroup(renderPass, 0, bindGroup, 0, null);
        // WebGPUApi.RenderPassEncoderSetViewport(
        //     renderPass, 0, 0, ctx.FinalRenderTexture.Extents.Width, ctx.FinalRenderTexture.Extents.Height, 0.0f, 1.0f
        // );
        WebGPUApi.RenderPassEncoderSetViewport(
            renderPass, ctx.DestinationRectangle.X, ctx.DestinationRectangle.Y, 
            ctx.DestinationRectangle.Width, ctx.DestinationRectangle.Height, 0.0f, 1.0f
        );
        // draw the quad (sampling into the src texture) onto the destination texture.
        WebGPUApi.RenderPassEncoderDraw(renderPass, 4, 1, 0, 0);
        WebGPUApi.RenderPassEncoderEnd(renderPass);

        /**========================================
            CLEAN-UP
        ========================================**/
        WebGPUApi.BindGroupRelease(bindGroup);
    }

    /**========================================
        SUBMIT COMMAND BUFFER
    ========================================**/
    WebGPU.CommandBufferDescriptor cmdBufferDesc = default;
    WebGPU.CommandBuffer* cmdBuffer = WebGPUApi.CommandEncoderFinish(cmdEncoder, &cmdBufferDesc);
    WebGPUApi.CommandEncoderRelease(cmdEncoder);
    WebGPUApi.QueueSubmit(device.Queue, 1, &cmdBuffer);
    WebGPUApi.CommandBufferRelease(cmdBuffer);

    /**========================================
        PRESENT
    ========================================**/    
    WebGPUApi.SurfacePresent(ctx.WindowSurface.Surface);
    
    /**========================================
        CLEAN-UP
    ========================================**/
    FreeSurfaceTexture(ref swapChainTexture);
    if(ctx.SpriteManager.OneFrameSpritesIndices.Count > 0){
        for(int i = 0; i < ctx.SpriteManager.OneFrameSpritesIndices.Count; i++){
            DeallocateSprite(ref ctx.SpriteManager, ctx.SpriteManager.OneFrameSpritesIndices[i]);
        }
        Collections.Clear(ref ctx.SpriteManager.OneFrameSpritesIndices);
    }
}

public static void FreeAllResources(
    ref RendererCtx ctx
){
    FreeTexture(ref ctx.DepthTexture);
    FreeBufferResources(ref ctx.IndexBuffer);
    FreeBufferResources(ref ctx.VertexBuffer);
    FreeBufferResources(ref ctx.UserUniformBuffer);
    FreeBufferResources(ref ctx.UserStorageBuffer);
    FreeGraphicsPipeline(ref ctx.GraphicsPipeline);
    FreeSpriteManagerResources(ref ctx.SpriteManager);
    FreeVirtualTextureManagerResources(ref ctx.VirtualTextureManager);
    WebGPUApi.SurfaceUnconfigure(ctx.WindowSurface.Surface);
    FreeWindowSurface(ref ctx.WindowSurface);
    for(int i = 0; i < ctx.Devices.Length; i++){
        FreeDeviceResources(ref ctx.Devices[i]);
    }
    for(int i = 0; i < ctx.Adapters.Length; i++){
        FreeAdapterResources(ref ctx.Adapters[i]);
    }
    WebGPUApi.InstanceRelease(ctx.Instance);
}

public static void RequestAdapters(
    ref RendererCtx ctx, ref Memory.Arena arena, WGpu.InstanceEnumerateAdapterOptions* options
){
    Debug.Assert(ctx.Adapters.IsInitialised==false, "Cannot retrieve wep gpu adapters as the renderer context has already done so.");    
    
    nuint totalAdapterCount = WGpuApi.InstanceEnumerateAdapters(ctx.Instance, null, null);
    WebGPU.Adapter** foundAdapters = stackalloc WebGPU.Adapter*[(int)totalAdapterCount];
    WGpuApi.InstanceEnumerateAdapters(ctx.Instance, options, foundAdapters);

    /******************
    
        Find Adapters.
    
    *******************/
    /**
        Force either a vulkan or metal backend, ensuring that a single GPU doesnt have multiple devices as there can be multiple Adapters 
        for the same GPU that differ in backend. There should never be more than one device per hardware; especially if they differ in backend. 
    **/
    WebGPU.BackendType backendType = WebGPU.BackendType.Undefined;
    if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
        backendType = WebGPU.BackendType.Vulkan;
    }
    else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
        backendType = WebGPU.BackendType.Metal;
    }
    int adapterCount = 0;
    for(int i = 0; i < (int)totalAdapterCount; i++){
        WebGPU.AdapterProperties properties = default;
        WebGPUApi.AdapterGetProperties(foundAdapters[i], &properties);
        if(properties.BackendType == backendType){
            adapterCount++;
        }
    }

    /******************
    
        Store Adapters.
    
    *******************/
    Collections.Init(ref ctx.Adapters, ref arena, adapterCount);
    int writeIndex = 0;
    for(int i = 0; i < (int)adapterCount; i++){
    fixed(Adapter* adapter = &ctx.Adapters[writeIndex]){
        // retrieve properties.
        WebGPU.AdapterProperties properties = default;
        WebGPUApi.AdapterGetProperties(foundAdapters[i], &properties);
        if(properties.BackendType != backendType){
            continue;
        }
        // retrieve adapter limits.
        if(WebGPUApi.AdapterGetLimits(foundAdapters[i], &adapter->SupportedLimits) == false){
            Debug.Panic("Failed to retrieve web gpu adapter limits.");
            goto Next;
        }

        // copy pointer.
        adapter->Pointer = foundAdapters[i];
        //retrieve adapter features.
        nuint featureCount = WebGPUApi.AdapterEnumerateFeatures(adapter->Pointer, null);
        Collections.Init(ref adapter->Features, ref arena, (int)featureCount);
        WebGPUApi.AdapterEnumerateFeatures(adapter->Pointer, adapter->Features.Pointer);
        adapter->IsInitialised = true;

        // conclude adapter write.
        Next:
            writeIndex++;
    }}
}

public static void RequestDevices(
    ref RendererCtx ctx, ref Memory.Arena arena
){
    // validation layers.
    Debug.Assert(ctx.Adapters.IsInitialised, 
        "Web Gpu renderer cannot request devices as it stores no adapters."
    );
    Debug.Assert(ctx.Devices.IsInitialised==false,
        "Renderer context cannot initialise devices as it already contains intialised devices."
    );

    // initialisation.
    Collections.Init(ref ctx.Devices, ref arena, ctx.Adapters.Length);
    var callback = new WebGPU.PfnRequestDeviceCallback(OnDeviceRequestEnded);
    var onLostCallback = new WebGPU.PfnDeviceLostCallback(OnDeviceLostCallback);

    // retrieval.
    for(int i = 0; i < ctx.Adapters.Length; i++){
        
        ref Adapter adapter = ref ctx.Adapters[i];
        if(adapter.IsInitialised==false){
            continue;
        }
        
        ref Device device = ref ctx.Devices[i];
        DeviceInitCtx initCtx = default;
        WebGPU.RequiredLimits requiredLimits = GetRequiredLimits(adapter);
        WebGPU.DeviceDescriptor descriptor = default;
        descriptor.Label = adapter.Properties.Name;
        descriptor.RequiredLimits = &requiredLimits;

        WebGPUApi.AdapterRequestDevice(
            ctx.Adapters[i].Pointer, &descriptor, callback, &initCtx
        );
        while(initCtx.RequestEnded!=true){
            System.Threading.Thread.Yield();
        }
        if(initCtx.RequestEnded==false){
            Debug.Panic("GPU Device request timed out.");
            continue;
        }
        if(initCtx.IsValid == false){
            Debug.Panic("GPU device request failed to retrieve a valid device.");
            continue;
        }

        device.Pointer = *initCtx.Device;
        device.Queue = WebGPUApi.DeviceGetQueue(device.Pointer);
        device.IsInitialised = true;
    }
}

public static void OnDeviceRequestEnded(
    WebGPU.RequestDeviceStatus status, WebGPU.Device* device, byte* msg, void* userData
){
    if(userData==null){
        return;
    }

    DeviceInitCtx* ctx = (DeviceInitCtx*)userData;
    if(status == WebGPU.RequestDeviceStatus.Success){
        ctx->Device = &device;
        ctx->IsValid = true;
    }
    else{
        string errorMsg = Marshal.PtrToStringAnsi((nint)msg) ?? "Unknown Error";
        Debug.Panic($"Failed to request Web GPU device with error msg: {errorMsg}");
    }
    ctx->RequestEnded = true;
}

public static void OnDeviceLostCallback(
    WebGPU.DeviceLostReason reason, byte* msg, void* userData
){
    string errorMsg = Marshal.PtrToStringAnsi((nint)msg) ?? "Unknown Error";
    Debug.Panic($"Device Lost: reason: '{reason}', error message: '{errorMsg}'");
}

public static ref Device GetChosenDevice(
    ref RendererCtx ctx
){
    return ref ctx.Devices[ctx.ChosenDevice];
}

public static ref Adapter GetChosenAdapter(
    ref RendererCtx ctx
){
    return ref ctx.Adapters[ctx.ChosenDevice];
}

public static void OnQueueWorkDone(
    WebGPU.QueueWorkDoneStatus status, void* userData
){
    switch(status){
        // case WebGPU.QueueWorkDoneStatus.Success:
        //     Debug.LogInfo("queue work finished successfully.");
        // break;
        case WebGPU.QueueWorkDoneStatus.Error:
            Debug.LogError("queue work finished with an error status.");
        break;
        case WebGPU.QueueWorkDoneStatus.Unknown:
            Debug.LogWarning("queue work finished with an unknown status.");
        break;
        case WebGPU.QueueWorkDoneStatus.DeviceLost:
            Debug.LogWarning("queue work finished with the device being lost.");
        break;
        case WebGPU.QueueWorkDoneStatus.Force32:
            Debug.LogInfo("queue work finished with status 'force 32'.");
        break;
    }
}

/**##########################################################################################################################################
    div: WINDOWING.
##########################################################################################################################################**/

public static void LinkToWindow(
    ref RendererCtx ctx, WindowManagerInfo windowInfo, uint windowWidth, uint windowHeight
){
    // validation steps.
    Debug.Assert(ctx.IsInitialised == false,
        "Web GPU cannot link an initialised renderer to a window."
    );
    Debug.Assert(ctx.Instance!=null, 
        "Web GPU cannot intiialise Web Gpu window as the renderer context doesnt contain a web gpu instance."
    );

    // free the previous surface if there was any.
    if(ctx.WindowSurface.IsInitialised){
        FreeWindowSurface(ref ctx.WindowSurface);
    }

    WebGPU.SurfaceDescriptor surfaceDescriptor = default;
    WebGPU.SurfaceDescriptorFromWindowsHWND win32Desc = default;
    WebGPU.SurfaceDescriptorFromXlibWindow x11Desc = default;
    WebGPU.SurfaceDescriptorFromWaylandSurface waylandDesc = default;
    WebGPU.SurfaceDescriptorFromMetalLayer cocoaDesc = default;
    
    // win32 (windows).
    if(windowInfo.Win32Info.IsInitialised == true){
        win32Desc.Chain = new(){SType = WebGPU.SType.SurfaceDescriptorFromWindowsHwnd };
        win32Desc.Hinstance = windowInfo.Win32Info.HInstance;
        win32Desc.Hwnd = windowInfo.Win32Info.Hwnd;
        surfaceDescriptor.NextInChain = (WebGPU.ChainedStruct*)&win32Desc;
    }
    // x11 (linux).
    else if(windowInfo.X11Info.IsInitialised == true){
        x11Desc.Chain = new(){ SType = WebGPU.SType.SurfaceDescriptorFromXlibWindow };
        x11Desc.Display = windowInfo.X11Info.Display;
        x11Desc.Window = (ulong)windowInfo.X11Info.Window;
        surfaceDescriptor.NextInChain = (WebGPU.ChainedStruct*)&x11Desc;
    }
    // wayland (linux).
    else if(windowInfo.WaylandInfo.IsInitialised == true){
        waylandDesc.Chain = new(){ SType = WebGPU.SType.SurfaceDescriptorFromWaylandSurface };
        waylandDesc.Display = windowInfo.WaylandInfo.Display;
        waylandDesc.Surface = windowInfo.WaylandInfo.Surface;
        surfaceDescriptor.NextInChain = (WebGPU.ChainedStruct*)&waylandDesc;
    }
    // cocoa (macos)
    else if(windowInfo.CocoaInfo.IsInitialised == true){
        cocoaDesc.Chain = new(){ SType = WebGPU.SType.SurfaceDescriptorFromMetalLayer };
        cocoaDesc.Layer = windowInfo.CocoaInfo.Window;
        surfaceDescriptor.NextInChain = (WebGPU.ChainedStruct*)&cocoaDesc;
    }

    // initialise render specifics.
    InitWindowSurface(ref ctx.WindowSurface, ref ctx.Instance, ref surfaceDescriptor, windowWidth, windowHeight);
    ConfigureWindowSurface(ref ctx.WindowSurface, GetChosenDevice(ref ctx), GetChosenAdapter(ref ctx), windowWidth, windowHeight);    
}

public static void InitWindowSurface(
    ref WindowSurface surface, ref WebGPU.Instance* instance, ref WebGPU.SurfaceDescriptor desc, uint windowWidth, uint windowHeight
){
    Debug.Assert(surface.IsInitialised==false, "Attempted to initialise an already initialised window surface.");
    surface.IsInitialised = true;
    surface.Surface = WebGPUApi.InstanceCreateSurface(instance, ref desc);
    surface.WindowExtents = new(){Width = windowWidth, Height = windowHeight, DepthOrArrayLayers = 1};
}

public static void ConfigureWindowSurface(
    ref WindowSurface windowSurface, Device device, Adapter deviceAdapter, uint windowWidth, uint windowHeight
){
    { // validation.
        AssertInitialisedWindowSurface(windowSurface);
    }

    WebGPUApi.SurfaceUnconfigure(windowSurface.Surface);
    WebGPU.SurfaceConfiguration config = default;
    //configure the textures created for the underlying swap chain.
    config.Width = windowWidth;
    config.Height = windowHeight;
    config.Format = WebGPUApi.SurfaceGetPreferredFormat(windowSurface.Surface, deviceAdapter.Pointer);
    config.Usage = WebGPU.TextureUsage.RenderAttachment;
    config.Device = device.Pointer;
    /**
        Specify the swap chain behaviour:

        Immediate:
            No off-screen texture is used, the render process directly draws on the surface, which might lead
            to artifacts (e.g. tearing) but has zero latency. 
        Mailbox:
            There is only one slot in the queue, and when a new frame is rendered, it replacees the one currently
            waiting (whch is discarded without ever being presented).
        Fifo:
            Stands for "first-in, fisrt-out", meaning that the presented texture is always the oldest one like a
            regular queue. No rendered texture is wasted. 
    **/
    config.PresentMode = WebGPU.PresentMode.Immediate;
    // spcifies how the texture is composited onto the OS window.
    config.AlphaMode = WebGPU.CompositeAlphaMode.Auto;
    windowSurface.WindowExtents.Width = windowWidth;
    windowSurface.WindowExtents.Height = windowHeight;
    WebGPUApi.SurfaceConfigure(windowSurface.Surface, &config);
}

public static SurfaceTexture GetNextSwapChainImageView(
    ref RendererCtx ctx
){

    /**========================================
        Validation.
    ========================================**/
    {
        AssertInitialisedRenderCtx(ctx);
        AssertInitialisedWindowSurface(ctx.WindowSurface);
    }

    SurfaceTexture surfaceTexture = default;
    ref WindowSurface windowSurface = ref ctx.WindowSurface; 

    /**
        Get the texture to draw onto; Note that the 'surface texture' is not really an object, rather a container
        for the multiple things that this function returns.
    **/
    WebGPUApi.SurfaceGetCurrentTexture(windowSurface.Surface, &surfaceTexture.WTexture);
    if(surfaceTexture.WTexture.Status != WebGPU.SurfaceGetCurrentTextureStatus.Success){
        Debug.Panic("Failed to get next swapchain surface texture");
        return default;
    }
    /**
        Wrap up the raw surface texture data into a texture view.
    **/
    WebGPU.TextureViewDescriptor desc = default;
    desc.NextInChain = null;
    desc.Format = WebGPUApi.TextureGetFormat(surfaceTexture.WTexture.Texture);
    desc.Dimension = WebGPU.TextureViewDimension.Dimension2D;
    // swap chain images dont use mipmaps.
    desc.BaseMipLevel = 0;
    desc.MipLevelCount = 1;
    desc.BaseArrayLayer = 0;
    desc.ArrayLayerCount = 1;
    // equivalent to vulkan image aspect.
    desc.Aspect = WebGPU.TextureAspect.All; 
    surfaceTexture.View = WebGPUApi.TextureCreateView(surfaceTexture.WTexture.Texture, &desc);
    surfaceTexture.Extents = windowSurface.WindowExtents;

    return surfaceTexture;
}

public static void FreeSurfaceTexture(
    ref SurfaceTexture surfaceTexture
){
    WebGPUApi.TextureRelease(surfaceTexture.WTexture.Texture);
    WebGPUApi.TextureViewRelease(surfaceTexture.View);
}

public static void FreeWindowSurface(
    ref WindowSurface windowSurface
){
    WebGPUApi.SurfaceRelease(windowSurface.Surface);
}

public static void HandleWindowResize(
    ref RendererCtx ctx, Vector2UI windowResolution
){
    
    ConfigureWindowSurface(
        ref ctx.WindowSurface, GetChosenDevice(ref ctx), GetChosenAdapter(ref ctx), windowResolution.X, windowResolution.Y
    );

    UpdateDestinationRectangle(ref ctx);
}

public static void UpdateDestinationRectangle(
    ref RendererCtx ctx
){
    { // validation
        AssertInitialisedTexture(ctx.FinalRenderTexture);
        AssertInitialisedWindowSurface(ctx.WindowSurface);
    }

    ctx.DestinationRectangle = N_Rendering.Renderer.CalculateDestinationRectangle(
        ctx.FinalRenderTexture.Extents.Width, ctx.FinalRenderTexture.Extents.Height, 
        ctx.WindowSurface.WindowExtents.Width, ctx.WindowSurface.WindowExtents.Height
    );
}

/**##########################################################################################################################################
    div: PIPELINES
##########################################################################################################################################**/

public static void InitGraphicsPipeline(
    ref RendererCtx ctx, String shaderFilePath, uint userUniformBufferSizeInBytes, uint userStorageBufferSizeInBytes
){
fixed(byte* vertShaderEntryPoint = VertShaderEntryPoint){
fixed(byte* fragShaderEntryPoint = FragShaderEntryPoint){

    WebGPU.RenderPipelineDescriptor pipelineDesc = default;
    ref Device device = ref GetChosenDevice(ref ctx);
    ref Adapter adapter = ref GetChosenAdapter(ref ctx);

    /**========================================
        Validation.
    ========================================**/
    {
        // crash.
        Debug.Assert(ctx.GraphicsPipeline.IsInitialised==false, 
            "Attempted initialisation of an already initialised graphics pipeline."
        );
        AssertInitialisedDevice(device);
        AssertInitialisedAdapter(adapter);
        AssertInitialisedTexture(ctx.FinalRenderTexture);
    }

    InitSampler(ref ctx.GraphicsPipeline.NonFilterSampler, device);

    /**========================================
        LOAD SHADER.
    ========================================**/
    WebGPU.ShaderModule* shaderModule = LoadShaderModule(ref ctx, shaderFilePath, ref ctx.TransientArena);

    /**========================================
        VERTEX STATE.
    ========================================**/    
    pipelineDesc.Vertex.Module = shaderModule;
    pipelineDesc.Vertex.BufferCount = 0;
    pipelineDesc.Vertex.EntryPoint = vertShaderEntryPoint;
    pipelineDesc.Vertex.ConstantCount = 0;
    pipelineDesc.Primitive.Topology = WebGPU.PrimitiveTopology.TriangleList;
    // Specify the order of vertices that should be connected; when not specified like so: vertices are considered sequentially.
    pipelineDesc.Primitive.StripIndexFormat = WebGPU.IndexFormat.Undefined;
    // clock wise.
    pipelineDesc.Primitive.FrontFace = WebGPU.FrontFace.CW;
    pipelineDesc.Primitive.CullMode = WebGPU.CullMode.None;
    
    /**========================================
        FRAMGENT STATE.
    ========================================**/    
    WebGPU.FragmentState fragState = default;
    fragState.Module = shaderModule;
    fragState.EntryPoint = fragShaderEntryPoint;
    fragState.ConstantCount = 0;
    WebGPU.BlendState blendState = default;
    /** 
        The blending equation can be set independently for the rgb channels and the alpha channel, in general, it takes the following form:

            rgb = Color.SrcFactor * srcRgb [Color.Operation] Color.DstFactor * dstRgb;

        the usual blending equation is configures as:

            rgb = srcAlpha * srcRgb + (1 - srcAlpha) * dstRgb;

        corresponding to the intuition of "layering" rendered fragments over the existing pixel's value.
    **/
    blendState.Color.SrcFactor = WebGPU.BlendFactor.SrcAlpha;
    blendState.Color.Operation = WebGPU.BlendOperation.Add;
    blendState.Color.DstFactor = WebGPU.BlendFactor.OneMinusSrcAlpha;
    /**
        There is a similar blending equation for the alpha channel:

            alpha = Alpha.SrcFactor * srcAlpha [Alpha.Operation] Alpha.DstFactor * dstAlpha

        the target alpha should stay untouched:

            alpha = dstAlpha = 0 * srcAlpha + 1 * dstAlpha;
    **/
    blendState.Alpha.SrcFactor = WebGPU.BlendFactor.SrcAlpha;
    blendState.Alpha.Operation = WebGPU.BlendOperation.Add;
    blendState.Alpha.DstFactor = WebGPU.BlendFactor.OneMinusSrcAlpha;
    WebGPU.ColorTargetState colourTarget = default;
    colourTarget.Format = WebGPUApi.SurfaceGetPreferredFormat(ctx.WindowSurface.Surface, adapter.Pointer);
    colourTarget.Blend = &blendState;
    colourTarget.WriteMask = WebGPU.ColorWriteMask.All;
    // we only have one target because out render pass has only one output colour attachment.
    fragState.TargetCount = 1;
    fragState.Targets = &colourTarget;
    pipelineDesc.Fragment = &fragState;

    /**========================================
        DEPTH STATE.
    ========================================**/
    WebGPU.StencilFaceState stencilFaceState = default;
    stencilFaceState.Compare = WebGPU.CompareFunction.Less;
    stencilFaceState.DepthFailOp = WebGPU.StencilOperation.Keep;
    stencilFaceState.FailOp = WebGPU.StencilOperation.Keep;
    stencilFaceState.PassOp = WebGPU.StencilOperation.Replace;
    WebGPU.DepthStencilState depthStencilState = default;
    depthStencilState.DepthCompare = WebGPU.CompareFunction.Less;
    depthStencilState.DepthWriteEnabled = true;
    depthStencilState.Format = WebGPU.TextureFormat.Depth24Plus;
    // we are not using the stencil at all.
    depthStencilState.StencilFront = stencilFaceState;
    depthStencilState.StencilBack = stencilFaceState;
    depthStencilState.StencilReadMask = 0;
    depthStencilState.StencilWriteMask = 0;
    depthStencilState.DepthWriteEnabled = false;
    pipelineDesc.DepthStencil = &depthStencilState;

    /**========================================
        MULTISAMPLE STATE
    ========================================**/
    // Multi-sampling/Anti-aliasing is off for now with the set values.
    pipelineDesc.Multisample.Count = 1u;
    pipelineDesc.Multisample.Mask = ~0u;
    pipelineDesc.Multisample.AlphaToCoverageEnabled = false;

    /**========================================
        LAYOUTS
    ========================================**/
    /**
        Layouts define the way a resource (buffer/texture) is accessed by the driver, where as a descriptor set (a descriptor in WebGPU)
        defines the actual data that is accessed; this enables the driver to perform optimisations and validation checks ahead of time.
    **/
    // create the pipeline layout.
    WebGPU.PipelineLayoutDescriptor layoutDesc = default;

        /**========================================
            VERTEX BUFFER LAYOUT
        ========================================**/
        /**
            For the 'vertex fetch' stage to transform this raw data from the vertex buffer 
            into what the vertex shader expects, we need to specify a layout.
        **/
        int vertexAttCount = 2;
        WebGPU.VertexAttribute* vertexAtts = stackalloc WebGPU.VertexAttribute[vertexAttCount];        
            
            /**========================================
                POSITION ATT
            ========================================**/
            ref WebGPU.VertexAttribute positionAtt = ref vertexAtts[0];
            positionAtt.ShaderLocation = ShaderVertexLocation.Position;
            // WebGPU.VertexFormat.Float32x3 means Vector3.
            positionAtt.Format = WebGPU.VertexFormat.Float32x3;
            positionAtt.Offset = Vertex.OffsetOfPosition;

            /**========================================
                UV ATT
            ========================================**/
            ref WebGPU.VertexAttribute uvAtt = ref vertexAtts[1];
            uvAtt.ShaderLocation = ShaderVertexLocation.UV;
            // WebGPU.VertexFormat.Float32x2 means Vector2.
            uvAtt.Format = WebGPU.VertexFormat.Float32x2;
            uvAtt.Offset = Vertex.OffsetOfUV;

        WebGPU.VertexBufferLayout vertexBufferLayout = default;
        /**
            The stride designates the number of bytes between two consecutive elements that form a vertex; in out case, the positions are
            contiguous so the stride is equal to the size of a vector2. This should only change when adding more interleaved attributes.
        **/
        vertexBufferLayout.ArrayStride = (uint)sizeof(Vertex);
        /**
            StepMode = Vertex:
                each entry in the buffer corresponds to a different vertex.

            StepMode = Instance:
                each entry is shared by all vertices of the same instance (i.e, copy) of the shape.
        **/
        vertexBufferLayout.StepMode = WebGPU.VertexStepMode.Vertex;
        vertexBufferLayout.AttributeCount = (uint)vertexAttCount;
        vertexBufferLayout.Attributes = vertexAtts;
        /**
            note that a given render pipeline may use more than one vertex buffer. On the other hand, the same
            vertex buffer can contain multiple vertex attributes.
        **/
        pipelineDesc.Vertex.BufferCount = 1;    
        pipelineDesc.Vertex.Buffers = &vertexBufferLayout;

        /**========================================
            BINDINGS
        ========================================**/
            /**========================================
                LAYOUTS.
            ========================================**/            
                /**========================================
                    GROUP 0 (BUFFERS)
                ========================================**/                                
                WebGPU.BindGroupLayoutEntry* group0LayoutEntries = stackalloc WebGPU.BindGroupLayoutEntry[(int)ShaderBindingCount.Buffers]; 

                // user uniform.
                ref WebGPU.BindGroupLayoutEntry userUniformLayoutEntry = ref group0LayoutEntries[(int)ShaderBinding.UserUniform];
                userUniformLayoutEntry.Buffer.Type = WebGPU.BufferBindingType.Uniform;
                userUniformLayoutEntry.Buffer.MinBindingSize = 0;
                userUniformLayoutEntry.Binding = (uint)ShaderBinding.UserUniform;
                userUniformLayoutEntry.Visibility = WebGPU.ShaderStage.Vertex | WebGPU.ShaderStage.Fragment;
                
                // virtual textures uniform.
                ref WebGPU.BindGroupLayoutEntry virtualTexturesUniformLayoutEntry = ref group0LayoutEntries[(int)ShaderBinding.VirtualTexturesUniform];
                virtualTexturesUniformLayoutEntry.Buffer.Type = WebGPU.BufferBindingType.Uniform;
                virtualTexturesUniformLayoutEntry.Buffer.MinBindingSize = 0;
                virtualTexturesUniformLayoutEntry.Binding = (uint)ShaderBinding.VirtualTexturesUniform;
                virtualTexturesUniformLayoutEntry.Visibility = WebGPU.ShaderStage.Fragment;

                // sprite ssbo.
                ref WebGPU.BindGroupLayoutEntry spriteSSBOLayoutEntry = ref group0LayoutEntries[(int)ShaderBinding.SpritesStorage];
                spriteSSBOLayoutEntry.Buffer.Type = WebGPU.BufferBindingType.ReadOnlyStorage;
                spriteSSBOLayoutEntry.Buffer.MinBindingSize = 0;
                spriteSSBOLayoutEntry.Binding = (uint)ShaderBinding.SpritesStorage;
                spriteSSBOLayoutEntry.Visibility = WebGPU.ShaderStage.Vertex | WebGPU.ShaderStage.Fragment;

                // user ssbo.
                ref WebGPU.BindGroupLayoutEntry userSSBOLayoutEntry = ref group0LayoutEntries[(int)ShaderBinding.UserStorage];
                userSSBOLayoutEntry.Buffer.Type = WebGPU.BufferBindingType.ReadOnlyStorage;
                userSSBOLayoutEntry.Buffer.MinBindingSize = 0;
                userSSBOLayoutEntry.Binding = (int)ShaderBinding.UserStorage;
                userSSBOLayoutEntry.Visibility = WebGPU.ShaderStage.Vertex | WebGPU.ShaderStage.Fragment;

                // create bind group layout. 
                WebGPU.BindGroupLayoutDescriptor group0LayoutDesc = default;
                group0LayoutDesc.EntryCount = (uint)ShaderBindingCount.Buffers;
                group0LayoutDesc.Entries = group0LayoutEntries;
                ctx.GraphicsPipeline.BindGroup0Layout = 
                    WebGPUApi.DeviceCreateBindGroupLayout(device.Pointer, &group0LayoutDesc);

                /**========================================
                    GROUP 1: (TEXTURE ARRAYS)
                ========================================**/                
                int textureArrayCount = ctx.VirtualTextureManager.TextureArrays.Length;
                WebGPU.BindGroupLayoutEntry* group1LayoutEntries = stackalloc WebGPU.BindGroupLayoutEntry[textureArrayCount];
                
                // define the texture array entries.
                for(uint i = 0; i < textureArrayCount; i++){
                    ref WebGPU.BindGroupLayoutEntry entry = ref group1LayoutEntries[i];
                    entry.Binding = i;
                    entry.Visibility = WebGPU.ShaderStage.Fragment;
                    entry.Texture.SampleType = WebGPU.TextureSampleType.Float;
                    entry.Texture.ViewDimension = WebGPU.TextureViewDimension.Dimension2DArray;
                }
                
                // crete the bind group layout.
                WebGPU.BindGroupLayoutDescriptor group1LayoutDesc = default;
                group1LayoutDesc.EntryCount = (uint)textureArrayCount;
                group1LayoutDesc.Entries = group1LayoutEntries;
                ctx.GraphicsPipeline.BindGroup1Layout = 
                    WebGPUApi.DeviceCreateBindGroupLayout(device.Pointer, &group1LayoutDesc);

                /**========================================
                    GROUP 2: (UTILITIES)
                ========================================**/
                uint bindGroup2EntryCount = (uint)ShaderBindingCount.Utilities;
                WebGPU.BindGroupLayoutEntry* group2LayoutEntries = stackalloc WebGPU.BindGroupLayoutEntry[(int)bindGroup2EntryCount];
                
                // define the entries.
                ref WebGPU.BindGroupLayoutEntry nonFilterSamplerLayoutEntry = ref group2LayoutEntries[(uint)ShaderBinding.NonFilterSampler];
                nonFilterSamplerLayoutEntry.Binding = (uint)ShaderBinding.NonFilterSampler;
                nonFilterSamplerLayoutEntry.Visibility = WebGPU.ShaderStage.Fragment;
                // non filtering as we dont use bilinear interpolation or whatever; nearest because pixel art.
                nonFilterSamplerLayoutEntry.Sampler.Type = WebGPU.SamplerBindingType.NonFiltering;
                
                // create the group layout.
                WebGPU.BindGroupLayoutDescriptor group2LayoutDesc = default;
                group2LayoutDesc.EntryCount = bindGroup2EntryCount;
                group2LayoutDesc.Entries = group2LayoutEntries;
                ctx.GraphicsPipeline.BindGroup2Layout =
                    WebGPUApi.DeviceCreateBindGroupLayout(device.Pointer, &group2LayoutDesc);

            /**
                set the layouts; note that ordering matters here, descriptor sets cannot be made without their 
                respective layouts not being binded to the descriptor before hand. 
            **/
            layoutDesc.BindGroupLayoutCount = 3;
            WebGPU.BindGroupLayout** bindGroupLayouts = stackalloc WebGPU.BindGroupLayout*[]{
                ctx.GraphicsPipeline.BindGroup0Layout, ctx.GraphicsPipeline.BindGroup1Layout, ctx.GraphicsPipeline.BindGroup2Layout
            };
            layoutDesc.BindGroupLayouts = bindGroupLayouts;


            /**========================================
                DESCRIPTOR SETS
            ========================================**/
                /**========================================
                    GROUP 0
                ========================================**/                            
                WebGPU.BindGroupEntry* group0Entries = stackalloc WebGPU.BindGroupEntry[(int)ShaderBindingCount.Buffers]; 
                
                // user uniform.
                ref WebGPU.BindGroupEntry userUniformEntry = ref group0Entries[(int)ShaderBinding.UserUniform];
                userUniformEntry.Binding = (uint)ShaderBinding.UserUniform;
                userUniformEntry.Buffer = ctx.UserUniformBuffer.Device;
                userUniformEntry.Offset = 0;
                // this should crash the program.
                Debug.Assert(userUniformBufferSizeInBytes > 0, "Web GPU cannot intialise a user uniform buffer with a size less than zero.");
                userUniformEntry.Size = userUniformBufferSizeInBytes;
                
                // user ssbo.
                ref WebGPU.BindGroupEntry userSSBOEntry = ref group0Entries[(int)ShaderBinding.UserStorage];
                userSSBOEntry.Binding = (uint)ShaderBinding.UserStorage;
                userSSBOEntry.Buffer = ctx.UserStorageBuffer.Device;
                userSSBOEntry.Offset = 0;
                // this should crash the program.
                Debug.Assert(userStorageBufferSizeInBytes > 0, "Web GPU cannot intialise a user storage buffer with a size less than zero.");
                userSSBOEntry.Size = userStorageBufferSizeInBytes;
                
                // virtual textures uniform.
                ref WebGPU.BindGroupEntry virtualTexturesEntry = ref group0Entries[(int)ShaderBinding.VirtualTexturesUniform];
                virtualTexturesEntry.Binding = (uint)ShaderBinding.VirtualTexturesUniform;
                virtualTexturesEntry.Buffer = ctx.VirtualTextureManager.VirtualTextureBuffer.Device;
                virtualTexturesEntry.Offset = 0;
                virtualTexturesEntry.Size = (uint)(sizeof(VirtualTexture) * ctx.VirtualTextureManager.VirtualTextures.Length);

                // sprite ssbo.
                ref WebGPU.BindGroupEntry spriteSSBOEntry = ref group0Entries[(int)ShaderBinding.SpritesStorage];
                spriteSSBOEntry.Binding = (uint)ShaderBinding.SpritesStorage;
                spriteSSBOEntry.Buffer = ctx.SpriteManager.SpriteBuffer.Device;
                spriteSSBOEntry.Offset = 0;
                spriteSSBOEntry.Size = (uint)(sizeof(Sprite) * ctx.SpriteManager.Sprites.Length);

                // create the bind group.
                WebGPU.BindGroupDescriptor bindGroup0Desc = default;
                bindGroup0Desc.Layout = ctx.GraphicsPipeline.BindGroup0Layout;
                bindGroup0Desc.EntryCount = (uint)ShaderBindingCount.Buffers;
                bindGroup0Desc.Entries = group0Entries;
                ctx.GraphicsPipeline.BindGroup0 = WebGPUApi.DeviceCreateBindGroup(device.Pointer, &bindGroup0Desc);

                /**========================================
                    GROUP 1.
                ========================================**/
                // texture array entries.
                WebGPU.BindGroupEntry* group1Entries = stackalloc WebGPU.BindGroupEntry[textureArrayCount];
                for(uint i = 0; i < textureArrayCount; i++){
                    WebGPU.BindGroupEntry* textureArrayEntry = &group1Entries[i];
                    textureArrayEntry->Binding = i;
                    textureArrayEntry->TextureView = ctx.VirtualTextureManager.TextureArrays[(int)i].View;
                }

                // create the binding group.
                WebGPU.BindGroupDescriptor bindGroup1Desc = default;
                bindGroup1Desc.Layout = ctx.GraphicsPipeline.BindGroup1Layout;
                bindGroup1Desc.EntryCount = (uint)textureArrayCount;
                bindGroup1Desc.Entries = group1Entries;
                ctx.GraphicsPipeline.BindGroup1 = WebGPUApi.DeviceCreateBindGroup(device.Pointer, &bindGroup1Desc);

                /**========================================
                    GROUP 2.
                ========================================**/
                // sampler entry.
                WebGPU.BindGroupEntry* group2Entries = stackalloc WebGPU.BindGroupEntry[(int)ShaderBindingCount.Utilities];
                ref WebGPU.BindGroupEntry nonFilterSamplerEntry = ref group2Entries[(int)ShaderBinding.NonFilterSampler];
                nonFilterSamplerEntry.Binding = (uint)ShaderBinding.NonFilterSampler;
                nonFilterSamplerEntry.Sampler = ctx.GraphicsPipeline.NonFilterSampler;
                
                // create the binding group.
                WebGPU.BindGroupDescriptor bindGroup2Desc = default;
                bindGroup2Desc.Layout = ctx.GraphicsPipeline.BindGroup2Layout;
                bindGroup2Desc.EntryCount = (uint)ShaderBindingCount.Utilities;
                bindGroup2Desc.Entries = group2Entries;
                ctx.GraphicsPipeline.BindGroup2 = WebGPUApi.DeviceCreateBindGroup(device.Pointer, &bindGroup2Desc);

    // create the pipeline layout.    
    ctx.GraphicsPipeline.Layout = WebGPUApi.DeviceCreatePipelineLayout(device.Pointer, &layoutDesc);
    pipelineDesc.Layout = ctx.GraphicsPipeline.Layout;

    /**========================================
        CREATION
    ========================================**/
    ctx.GraphicsPipeline.RenderPipeline = WebGPUApi.DeviceCreateRenderPipeline(GetChosenDevice(ref ctx).Pointer, &pipelineDesc);
    ctx.GraphicsPipeline.IsInitialised = true;

    /**========================================
        CLEANUP
    ========================================**/
    // this is okay as the shader has already been loaded into the pipeline.
    WebGPUApi.ShaderModuleRelease(shaderModule);
}}}

public static void FreeGraphicsPipeline(
    ref GraphicsPipeline pipeline
){
    WebGPUApi.RenderPipelineRelease(pipeline.RenderPipeline);
    WebGPUApi.PipelineLayoutRelease(pipeline.Layout);
    WebGPUApi.SamplerRelease(pipeline.NonFilterSampler);
    WebGPUApi.BindGroupLayoutRelease(pipeline.BindGroup0Layout);
    WebGPUApi.BindGroupLayoutRelease(pipeline.BindGroup1Layout);
    WebGPUApi.BindGroupLayoutRelease(pipeline.BindGroup2Layout);
    WebGPUApi.BindGroupRelease(pipeline.BindGroup0);
    WebGPUApi.BindGroupRelease(pipeline.BindGroup1);
    WebGPUApi.BindGroupRelease(pipeline.BindGroup2);
}

public static void InitBlitPipeline(
    ref RendererCtx ctx
){
fixed(byte* fragEntryPoint = FragShaderEntryPoint){
fixed(byte* vertEntryPoint = VertShaderEntryPoint){

    ref Device device = ref GetChosenDevice(ref ctx);
    ref Adapter adapter = ref GetChosenAdapter(ref ctx);

    /**========================================
        Validation.
    ========================================**/
    {
        // crash.
        Debug.Assert(ctx.BlitPipeline.IsInitialised==false, 
            "Attempted initialisation of an already initialised graphics pipeline."
        );
        AssertInitialisedDevice(device);
        AssertInitialisedAdapter(adapter);
        AssertInitialisedTexture(ctx.FinalRenderTexture);
    }

    InitSampler(ref ctx.BlitPipeline.Sampler, device);

    /**========================================
        SHADER CREATION.
    ========================================**/
    WebGPU.ShaderModule* shaderModule = CreateShaderModule(ref ctx, blitShaderCode);

    /**========================================
        PRIMITIVE STATE
    ========================================**/
    WebGPU.PrimitiveState primitiveState = default;
    // Must be WebGPU.PrimitiveTopology.TriangleStrip for the 4-vertex quad setup to map correctly.
    primitiveState.Topology = WebGPU.PrimitiveTopology.TriangleStrip;
    primitiveState.CullMode = WebGPU.CullMode.None;

    /**========================================
        MULTISAMPLE STATE
    ========================================**/
    WebGPU.MultisampleState multisampleState = default;
    multisampleState.Count = 1;
    // max value turns this off.
    multisampleState.Mask = uint.MaxValue;
    multisampleState.AlphaToCoverageEnabled = false;

    /**========================================
        VERTEX STATE
    ========================================**/
    WebGPU.VertexState vertState = default;
    vertState.Module = shaderModule;
    vertState.EntryPoint = vertEntryPoint;
    vertState.BufferCount = 0;
    vertState.Buffers = null;

    /**========================================
        FRAGMENT STATE
    ========================================**/
    WebGPU.BlendState blendState = default;
    // turn off any form of blending as the blit should not carry over any of the previous frames colours.
    blendState.Color.SrcFactor = WebGPU.BlendFactor.One;
    blendState.Color.Operation = WebGPU.BlendOperation.Add;
    blendState.Color.DstFactor = WebGPU.BlendFactor.Zero;
    blendState.Alpha.SrcFactor = WebGPU.BlendFactor.One;
    blendState.Alpha.Operation = WebGPU.BlendOperation.Add;
    blendState.Alpha.DstFactor = WebGPU.BlendFactor.Zero;
    WebGPU.ColorTargetState colourTarget = default;
    colourTarget.Format = WebGPUApi.SurfaceGetPreferredFormat(ctx.WindowSurface.Surface, adapter.Pointer);
    colourTarget.Blend = &blendState;
    colourTarget.WriteMask = WebGPU.ColorWriteMask.All;
    WebGPU.FragmentState fragState = default;
    fragState.Module = shaderModule;
    fragState.EntryPoint = fragEntryPoint;
    fragState.TargetCount = 1;
    fragState.Targets = & colourTarget;

    /**========================================
        LAYOUT
    ========================================**/
        /**========================================
            BIND GROUP
        ========================================**/
        WebGPU.BindGroupLayoutDescriptor groupLayoutDesc = default;
        groupLayoutDesc.EntryCount = BlitPipeline.BindGroupEntryCount;
        WebGPU.BindGroupLayoutEntry* groupLayoutEntries = stackalloc WebGPU.BindGroupLayoutEntry[(int)BlitPipeline.BindGroupEntryCount];
        { // initialisation.

            ref WebGPU.BindGroupLayoutEntry samplerLayoutEntry = ref groupLayoutEntries[BlitPipeline.SamplerBinding];
            samplerLayoutEntry.Binding = BlitPipeline.SamplerBinding;
            samplerLayoutEntry.Visibility = WebGPU.ShaderStage.Fragment;
            samplerLayoutEntry.Sampler.Type = WebGPU.SamplerBindingType.NonFiltering;
            ref WebGPU.BindGroupLayoutEntry textureLayoutEntry = ref groupLayoutEntries[BlitPipeline.TextureBinding];
            textureLayoutEntry.Binding = BlitPipeline.TextureBinding;
            textureLayoutEntry.Visibility = WebGPU.ShaderStage.Fragment;
            textureLayoutEntry.Texture.SampleType = WebGPU.TextureSampleType.Float;
            textureLayoutEntry.Texture.ViewDimension = WebGPU.TextureViewDimension.Dimension2D;
        }
        groupLayoutDesc.Entries = groupLayoutEntries;
        ctx.BlitPipeline.BindGroupLayout = WebGPUApi.DeviceCreateBindGroupLayout(GetChosenDevice(ref ctx).Pointer, ref groupLayoutDesc);

    WebGPU.BindGroupLayout** layouts = stackalloc WebGPU.BindGroupLayout*[1]{ctx.BlitPipeline.BindGroupLayout};
    WebGPU.PipelineLayoutDescriptor pipelineLayoutDesc = default;
    pipelineLayoutDesc.BindGroupLayoutCount = 1;
    pipelineLayoutDesc.BindGroupLayouts = layouts;
    WebGPU.PipelineLayout* pipelineLayout = WebGPUApi.DeviceCreatePipelineLayout(device.Pointer, ref pipelineLayoutDesc);


    /**========================================
        DESCRIPTOR SETS
    ========================================**/
        /**========================================
            BIND GROUP
        ========================================**/
        /**
            note that the binding groups are not setup here at all as they should be dynamically created during the render pass,
            This is because the swap chain texture which is being blitted constantly changes (as per the nature of swap chain structures).
        **/

    /**========================================
        PIPELINE CREATION.
    ========================================**/    
    WebGPU.RenderPipelineDescriptor pipelineDesc = default;
    pipelineDesc.Layout = pipelineLayout;
    pipelineDesc.Primitive = primitiveState;
    pipelineDesc.Multisample = multisampleState;
    pipelineDesc.Fragment = &fragState;
    pipelineDesc.Vertex = vertState;
    pipelineDesc.DepthStencil = null;
    ctx.BlitPipeline.RenderPipeline = WebGPUApi.DeviceCreateRenderPipeline(device.Pointer, ref pipelineDesc);
    ctx.BlitPipeline.IsInitialised = true;

    /**========================================
        CLEAN-UP.
    ========================================**/
    WebGPUApi.PipelineLayoutRelease(pipelineLayout);
    WebGPUApi.ShaderModuleRelease(shaderModule);
}}}

public static void FreeBlitPipeline(
    ref BlitPipeline pipeline
){
    WebGPUApi.RenderPipelineRelease(pipeline.RenderPipeline);
}

/**##########################################################################################################################################
    div: SHADERS.
##########################################################################################################################################**/

public static WebGPU.ShaderModule* CreateShaderModule(
    ref RendererCtx ctx, string shaderCode
){
    WebGPU.ShaderModuleWGSLDescriptor wgslDesc = default;
    wgslDesc.Chain.SType = WebGPU.SType.ShaderModuleWgslDescriptor;
    wgslDesc.Code = (byte*)SilkMarshal.StringToPtr(shaderCode);
    WebGPU.ShaderModuleDescriptor shaderDesc = default;
    shaderDesc.NextInChain = (WebGPU.ChainedStruct*)&wgslDesc;
    WebGPU.ShaderModule* module = WebGPUApi.DeviceCreateShaderModule(GetChosenDevice(ref ctx).Pointer, ref shaderDesc);
    SilkMarshal.Free((nint)wgslDesc.Code);
    return module;
}

public static WebGPU.ShaderModule* LoadShaderModule(
    ref RendererCtx ctx, String filePath, ref Memory.Arena arena
){
    Memory.Arena.ClearZeroed(ref arena);
    long totalBytesRead = 0;
    Howl.IO.File.Read(filePath, ref arena, ref totalBytesRead);
    WebGPU.ShaderModuleDescriptor desc = default;
    WebGPU.ShaderModuleWGSLDescriptor wgslDesc = default;
    wgslDesc.Chain.SType = WebGPU.SType.ShaderModuleWgslDescriptor;
    wgslDesc.Code = arena.StartPtr;
    desc.NextInChain = &wgslDesc.Chain;
    return WebGPUApi.DeviceCreateShaderModule(GetChosenDevice(ref ctx).Pointer, &desc);
}

public static WebGPU.Limits CreateDefaultLimits(

){
    /**
        undefined means there is no limit.
        this function is used for initialising limits as WebGPU follows RAII instead of ZII :((((

        the default capabilities for a device can be found here:
            https://www.w3.org/TR/webgpu/#limit-default
        note that every adapter is guaranteed to support the default or better:
            https://www.w3.org/TR/webgpu/#limit-default
    **/
    WebGPU.Limits limits = default;
    limits.MaxBindGroups = WebGPUStatic.LimitU32Undefined;
    limits.MaxBindGroupsPlusVertexBuffers = WebGPUStatic.LimitU32Undefined;
    limits.MaxBindingsPerBindGroup = WebGPUStatic.LimitU32Undefined;
    limits.MaxBufferSize = WebGPUStatic.LimitU64Undefined;
    limits.MaxColorAttachmentBytesPerSample = WebGPUStatic.LimitU32Undefined;
    limits.MaxColorAttachments = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeInvocationsPerWorkgroup = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeWorkgroupSizeX = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeWorkgroupSizeY = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeWorkgroupSizeZ = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeWorkgroupsPerDimension = WebGPUStatic.LimitU32Undefined;
    limits.MaxComputeWorkgroupStorageSize = WebGPUStatic.LimitU32Undefined;
    limits.MaxDynamicStorageBuffersPerPipelineLayout = WebGPUStatic.LimitU32Undefined;
    limits.MaxDynamicUniformBuffersPerPipelineLayout = WebGPUStatic.LimitU32Undefined;
    limits.MaxInterStageShaderComponents = WebGPUStatic.LimitU32Undefined;
    limits.MaxInterStageShaderVariables = WebGPUStatic.LimitU32Undefined;
    limits.MaxSampledTexturesPerShaderStage = WebGPUStatic.LimitU32Undefined;
    limits.MaxSamplersPerShaderStage = WebGPUStatic.LimitU32Undefined;
    limits.MaxStorageBufferBindingSize = WebGPUStatic.LimitU64Undefined;
    limits.MaxStorageBuffersPerShaderStage = WebGPUStatic.LimitU32Undefined;
    limits.MaxStorageTexturesPerShaderStage = WebGPUStatic.LimitU32Undefined;
    limits.MaxTextureArrayLayers = WebGPUStatic.LimitU32Undefined;
    limits.MaxTextureDimension1D = WebGPUStatic.LimitU32Undefined;
    limits.MaxTextureDimension2D = WebGPUStatic.LimitU32Undefined;
    limits.MaxTextureDimension3D = WebGPUStatic.LimitU32Undefined;
    limits.MaxUniformBufferBindingSize = WebGPUStatic.LimitU64Undefined;
    limits.MaxUniformBuffersPerShaderStage = WebGPUStatic.LimitU32Undefined;
    limits.MaxVertexAttributes = WebGPUStatic.LimitU32Undefined;
    limits.MaxVertexBufferArrayStride = WebGPUStatic.LimitU32Undefined;
    limits.MaxVertexBuffers = WebGPUStatic.LimitU32Undefined;
    limits.MinStorageBufferOffsetAlignment = WebGPUStatic.LimitU32Undefined;
    limits.MinUniformBufferOffsetAlignment = WebGPUStatic.LimitU32Undefined;
    return limits;
}

public static WebGPU.RequiredLimits GetRequiredLimits(
    Adapter adapter
){
    // https://www.w3.org/TR/webgpu/#limits

    WebGPU.SupportedLimits supported = adapter.SupportedLimits;
    WebGPU.RequiredLimits required = default;
    required.Limits = CreateDefaultLimits();
    
    required.Limits.MaxVertexAttributes = 3;
    required.Limits.MaxVertexBuffers = 1;
    // 256 is the default web gpu size.
    required.Limits.MaxBufferSize = Memory.Megabytes(256);
    // maximum stride is between two vonsecutivve vertice (to make triangles) in the vertex buffer.
    required.Limits.MaxVertexBufferArrayStride = (uint)sizeof(Vertex);
    /**
        These two limits are different because they are 'minimum' limits, they are the only ones we may forward from the adapter's 
        supported limits as it may cause issuesit they remain undefined (not supported by the adapter).
    **/
    required.Limits.MinUniformBufferOffsetAlignment = supported.Limits.MinUniformBufferOffsetAlignment;
    required.Limits.MinStorageBufferOffsetAlignment = supported.Limits.MinStorageBufferOffsetAlignment;
    // note that 4 is the guaranteed standard for WebGPU.
    required.Limits.MaxBindGroups = 4;
    required.Limits.MaxUniformBuffersPerShaderStage = 2;
    // 64Kib as defined as the default uniform buffer size by WebGPU. 
    required.Limits.MaxUniformBufferBindingSize = Memory.Kilobytes(64);
    // 128Mb is defined as the default storage buffer size by WebGPU.
    required.Limits.MaxStorageBufferBindingSize = Memory.Megabytes(128);
    // set the max required height of a texture; in pixels.
    required.Limits.MaxTextureDimension1D = 2160;
    // set the max required width of a texture; in pixels.
    required.Limits.MaxTextureDimension2D = 4096;
    required.Limits.MaxTextureArrayLayers = 256;
    return required;
}

public static void InitVertexBuffer(
    ref RendererCtx ctx 
){
    Debug.Assert(ctx.VertexBuffer.Host == null && ctx.VertexBuffer.Device == null,
        "Web GPU cannot init rendering context's vertex buffer as it has already done so."
    );
    WebGPU.BufferUsage hostUsage = WebGPU.BufferUsage.MapWrite | WebGPU.BufferUsage.CopySrc;
    WebGPU.BufferUsage deviceUsage = WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Vertex;
    ctx.VertexBuffer = CreateBuffer<Vertex>(GetChosenDevice(ref ctx), hostUsage, deviceUsage, 4);

    Vertex* pVertices = stackalloc Vertex[4]; 
    Buffer<Vertex> vertices = default;
    Collections.Init(ref vertices, pVertices, 4);
    // top left.
    Collections.Append(
        ref vertices, new(){UV = new(){X = 0, Y = 0}, Position = new(){X = -0.5f, Y = 0.5f}}
    );
    // top right.
    Collections.Append(
        ref vertices, new(){UV = new(){X = 1, Y = 0}, Position = new(){X = 0.5f, Y = 0.5f}}
    );
    // bottom right.
    Collections.Append(
        ref vertices, new(){UV = new(){X = 1, Y = 1}, Position = new(){X = 0.5f, Y = -0.5f}}
    );
    // bottom left.
    Collections.Append(
        ref vertices, new(){UV = new (){X = 0, Y = 1}, Position = new(){X = -0.5f, Y = -0.5f}}
    );
    WriteToBuffer(GetChosenDevice(ref ctx), vertices, ref ctx.VertexBuffer);
}

public static void InitIndexBuffer(
    ref RendererCtx ctx, int maxSprites
){
    Debug.Assert(ctx.IndexBuffer.Host == null && ctx.IndexBuffer.Device == null,
        "Web GPU cannot init rendering context's index buffer as it has already done so."
    );


    // round up to the next multiple of four: (verticesCount + 3) & ~3.
    int totalVertices = ((maxSprites * 4)+3)&~3;
    // verticesCount / 4 * 6 = total required indices.
    int totalIndices = (int)totalVertices / 4 * 6;

    WebGPU.BufferUsage hostUsage = WebGPU.BufferUsage.CopySrc | WebGPU.BufferUsage.MapWrite;
    WebGPU.BufferUsage deviceUsage = WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Index;
    ctx.IndexBuffer = CreateBuffer<uint>(GetChosenDevice(ref ctx), hostUsage, deviceUsage, (uint)totalIndices);


    uint* pIndices = stackalloc uint[totalIndices];
    Buffer<uint> indices = default;
    Collections.Init(ref indices, pIndices, totalIndices);
    for(uint i = 0; i < totalVertices; i+=4){
        Collections.Append(ref indices, i+0u);
        Collections.Append(ref indices, i+1u);
        Collections.Append(ref indices, i+2u);
        Collections.Append(ref indices, i+2u);
        Collections.Append(ref indices, i+3u);
        Collections.Append(ref indices, i+0u);
    }
    WriteToBuffer(GetChosenDevice(ref ctx), indices, ref ctx.IndexBuffer);

}

public static Buffer CreateBuffer<T>(
    Device device, WebGPU.BufferUsage hostUsage, WebGPU.BufferUsage deviceUsage, uint length
) where T : unmanaged{
    return CreateBuffer(device, hostUsage, deviceUsage, length, (uint)sizeof(T));
}

[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
public static Buffer CreateBuffer(
    Device device, WebGPU.BufferUsage hostUsage, WebGPU.BufferUsage deviceUsage, uint length, uint elementSize
){
    Buffer buffer = default;
    buffer.LengthInBytes = elementSize * length;
    // create the host staging buffer.
    WebGPU.BufferDescriptor hostDesc = default;
    hostDesc.Size = buffer.LengthInBytes;
    hostDesc.Usage = hostUsage;
    hostDesc.MappedAtCreation = false;
    buffer.Host = WebGPUApi.DeviceCreateBuffer(device.Pointer, &hostDesc);
    // create the device local buffer.
    WebGPU.BufferDescriptor deviceDesc = default;
    deviceDesc.Size = buffer.LengthInBytes;
    deviceDesc.Usage = deviceUsage;
    buffer.Device = WebGPUApi.DeviceCreateBuffer(device.Pointer, & deviceDesc);    
    return buffer;
} 

public static bool WriteToBuffer<T>(
    Device device, Buffer<T> src, ref Buffer dst
) where T : unmanaged{
    int srcByteCount = Memory.ArraySizeInBytes<T>(src.Count);
    return WriteToBuffer(device, ref dst, src.Pointer, (uint)srcByteCount);
}

public static bool WriteToBuffer<T>(
    Device device, Array<T> src, ref Buffer dst
) where T : unmanaged{
    int srcByteCount = Memory.ArraySizeInBytes<T>(src.Length);
    return WriteToBuffer(device, ref dst, src.Pointer, (uint)srcByteCount);
}

public static bool WriteToBuffer(
    Device device, ref Buffer dst, void* src, uint sizeOfSrcInBytes 
){
    
    // handle count & lengths.
    
    if(sizeOfSrcInBytes > dst.LengthInBytes){
        Debug.Panic("Web GPU buffer is of insufficient size to store user buffer.");
        return false;
    }
    dst.CountInBytes = sizeOfSrcInBytes;
    // asynchronously map the staging buffer for writing.
    MapAsyncCtx mapCtx = default;
    using(var callback = new WebGPU.PfnBufferMapCallback(OnMapAsyncEnd)){
        WebGPUApi.BufferMapAsync(dst.Host, WebGPU.MapMode.Write, 0, dst.LengthInBytes, callback, &mapCtx);
        while(mapCtx.RequestEnded == false){
            // make sure the thread doesnt block other operations while waiting on async.
            WebGPUApi.QueueSubmit(device.Queue, 0, null);
            System.Threading.Thread.Yield();
        }
        if(mapCtx.IsValid == false){
            return false;
        }
    }
    // write to the mapped data.
    void* mapped = WebGPUApi.BufferGetMappedRange(dst.Host, 0, dst.LengthInBytes);
    Unsafe.CopyBlock(mapped, src, sizeOfSrcInBytes);
    // hand ownership back to the gpu.
    WebGPUApi.BufferUnmap(dst.Host);
    // copy from host staging to device local buffer.
    WebGPU.CommandEncoderDescriptor cmdEncoderDesc = default;
    WebGPU.CommandEncoder* cmdEncoder = WebGPUApi.DeviceCreateCommandEncoder(device.Pointer, &cmdEncoderDesc);
    WebGPUApi.CommandEncoderCopyBufferToBuffer(cmdEncoder, dst.Host, 0, dst.Device, 0, dst.CountInBytes);
    WebGPU.CommandBufferDescriptor cmdBufferDesc = default;
    WebGPU.CommandBuffer* cmdBuffer = WebGPUApi.CommandEncoderFinish(cmdEncoder, &cmdBufferDesc);
    WebGPUApi.QueueSubmit(device.Queue, 1, &cmdBuffer);
    return true;
}

public static void OnMapAsyncEnd(WebGPU.BufferMapAsyncStatus status, void* userData){
    MapAsyncCtx* data = (MapAsyncCtx*)userData;
    if(status != WebGPU.BufferMapAsyncStatus.Success){
        Debug.Panic($"WebGpu failed to map buffer; error status '{status}'");
    }
    else{
        data->IsValid = true;
    }
    data->RequestEnded = true;
}

public static void FreeBufferResources(ref Buffer buffer){
    // note: yout may need to also release the buffer, but that crashes for some reason???
    WebGPUApi.BufferDestroy(buffer.Host);
    WebGPUApi.BufferDestroy(buffer.Device);
    buffer.Host = null;
    buffer.Device = null;
}

public static void InitUserUniformBuffer(
    ref RendererCtx ctx, uint userUniformBufferSizeInBytes
){
    Debug.Assert(ctx.UserUniformBuffer.Host == null && ctx.UserUniformBuffer.Device == null,
        "Web Gpu cannot init uniform buffer as it already has."
    );

    // ensure that the size is a multiple of 16; accounting for the 16 byte padding of WG.
    // uint adjustedSize = (sizeOfUbo + 15) & ~15u;
    WebGPU.BufferUsage hostUsage = WebGPU.BufferUsage.MapWrite | WebGPU.BufferUsage.CopySrc;
    WebGPU.BufferUsage deviceUsage = WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Uniform;
    ctx.UserUniformBuffer = CreateBuffer(GetChosenDevice(ref ctx), hostUsage, deviceUsage, 1, userUniformBufferSizeInBytes);
}

public static void InitUserStorageBuffer(
    ref RendererCtx ctx, uint userStorageBufferSizeInBytes
){
    Debug.Assert(ctx.UserStorageBuffer.Host == null && ctx.UserStorageBuffer.Device == null,
        "Web Gpu cannot init uniform buffer as it already has."
    );

    WebGPU.BufferUsage hostUsage = WebGPU.BufferUsage.MapWrite | WebGPU.BufferUsage.CopySrc;
    WebGPU.BufferUsage deviceUsage = WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Storage;
    ctx.UserStorageBuffer = CreateBuffer(GetChosenDevice(ref ctx), hostUsage, deviceUsage, 1, userStorageBufferSizeInBytes);
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para><paramref name="sizeOfBufferInBytes"/> should be no greater that the renderer intialisation value; otherwise memory corruption will occur.</para>
/// </remarks>
public static bool WriteToUserUniformBuffer(ref RendererCtx ctx, void* ubo, uint sizeOfBufferInBytes){
    Debug.Assert(ctx.IsInitialised, "Web GPU cannot write a user uniform buffer to an unintialised render context.");
    return WriteToBuffer(GetChosenDevice(ref ctx), ref ctx.UserUniformBuffer, ubo, sizeOfBufferInBytes);
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para><paramref name="sizeOfBufferInBytes"/> should be no greater that the renderer intialisation value; otherwise memory corruption will occur.</para>
/// </remarks>
public static bool WriteToUserStorageBuffer(ref RendererCtx ctx, void* ubo, uint sizeOfBufferInBytes){
    Debug.Assert(ctx.IsInitialised, "Web GPU cannot write a user storage buffer to an unintialised render context.");
    return WriteToBuffer(GetChosenDevice(ref ctx), ref ctx.UserStorageBuffer, ubo, sizeOfBufferInBytes);
}

/**##########################################################################################################################################
    div: TEXTURES.
##########################################################################################################################################**/

public static Texture CreateTexture(
    Device device, WebGPU.TextureFormat format, WebGPU.TextureUsage usage, WebGPU.TextureAspect aspect, uint width, uint height 
){
    AssertInitialisedDevice(device);

    Texture texture = default;
    // create the texture.
    WebGPU.TextureDescriptor textureDesc = default;
    textureDesc.Dimension = WebGPU.TextureDimension.Dimension2D;
    textureDesc.Format = format;
    textureDesc.MipLevelCount = 1;
    textureDesc.SampleCount = 1;
    textureDesc.Size = new(){Width = width, Height = height, DepthOrArrayLayers = 1};
    textureDesc.Usage = usage;
    textureDesc.ViewFormatCount = 1;
    textureDesc.ViewFormats = &format;
    texture.Pointer = WebGPUApi.DeviceCreateTexture(device.Pointer, &textureDesc);
    // create the view.
    WebGPU.TextureViewDescriptor viewDesc = default;
    viewDesc.Aspect = aspect;
    viewDesc.BaseArrayLayer = 0;
    viewDesc.ArrayLayerCount = 1;
    viewDesc.BaseMipLevel = 0;
    viewDesc.MipLevelCount = 1;
    viewDesc.Dimension = WebGPU.TextureViewDimension.Dimension2D;
    viewDesc.Format = format;
    texture.View = WebGPUApi.TextureCreateView(texture.Pointer, &viewDesc);
    texture.Extents = textureDesc.Size;
    texture.IsIntialised = true;
    return texture;
}

public static void FreeTexture(ref Texture texture){
    WebGPUApi.TextureViewRelease(texture.View);
    WebGPUApi.TextureDestroy(texture.Pointer);
    WebGPUApi.TextureRelease(texture.Pointer);
    texture.IsIntialised = false;
}

public static void InitDepthTexture(
    ref RendererCtx ctx, uint width, uint height
){
    
    Debug.Assert(ctx.DepthTexture.IsIntialised == false,
        "Web GPU has already initialised a depth texture."
    );

    ctx.DepthTexture = CreateTexture(
        GetChosenDevice(ref ctx), WebGPU.TextureFormat.Depth24Plus, WebGPU.TextureUsage.RenderAttachment, 
        WebGPU.TextureAspect.DepthOnly, width, height
    );
}

public static void InitVirtualTextureManager(
    ref VirtualTextureManager manager, Device device, ref Memory.Arena arena, Array<ImageTexturesInitInfo> imageInfos, 
    FontTexturesInitInfo fontInfo, int maxVirtualTextures, int filePathLength
){
    // validation steps.
    Debug.Assert(manager.IsInitialised==false,
        "Web GPU should not initialise a virtual texture manager that has already been initialised."
    );
    Debug.Assert(device.IsInitialised==true,
        "Web GPU cannot initialise a virtual texture manager with an uninitialised device."
    );
    Debug.Assert(maxVirtualTextures >= 2, 
        "Web GPU virtual texture manager cannot be initialised with less than two virtual textures"
    );
    Debug.Assert(maxVirtualTextures <= VirtualTexture.MaxAmount, 
        $"Web GPU renderer cannot store more than '{VirtualTexture.MaxAmount}' unqiue textures, requrested unique textures '{maxVirtualTextures}' is too large."
    );
    maxVirtualTextures = Math.Clamp(maxVirtualTextures, 2, VirtualTexture.MaxAmount);

    // initialise virtual textures.
    Collections.Init(ref manager.VirtualTextures, ref arena, maxVirtualTextures);
    Collections.Init(ref manager.VirtualTextureFilePaths, ref arena, maxVirtualTextures);
    Collections.Init(ref manager.VirtualTextureFontData, ref arena, maxVirtualTextures);
    Collections.Init(ref manager.VirtualTextureTypes, ref arena, maxVirtualTextures);
    manager.VirtualTextureBuffer = CreateBuffer<VirtualTexture>(
        device, WebGPU.BufferUsage.CopySrc | WebGPU.BufferUsage.MapWrite, WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Uniform, (uint)maxVirtualTextures
    );
    for(int i = 0; i < maxVirtualTextures; i++){
        String.Initialise(ref manager.VirtualTextureFilePaths[i], ref arena, filePathLength);
    }    

    // initialise font virtual textures.
    for(int i = 0; i < fontInfo.VirtualTextures.Length; i++){
        int virtualTextureIndex = fontInfo.VirtualTextures[i];
        Debug.Assert(virtualTextureIndex>=0, $"Cannot initialise virtual texture '{virtualTextureIndex} to be a font virtual texture.'");
        ref FontData fontData = ref manager.VirtualTextureFontData[virtualTextureIndex];
        Font.InitFontData(ref fontData, ref arena, fontInfo.GlyphCount, fontInfo.BaseGlyphIndex);
        manager.VirtualTextureTypes[virtualTextureIndex] = VirtualTextureType.Font;
    }

    // initialise texture arrays.
    // +2 for the nil entry and the font texture array.
    int textureArrayCount = imageInfos.Length + VirtualTextureManager.FontTextureArrayIndex + 1;
    int writeIndex = 0;
    Collections.Init(ref manager.TextureArrays, ref arena, textureArrayCount);
    
    // init the nil.
    // the format should be the least taxing on VRAM storage.
    WebGPU.TextureFormat nilTextureFormat = WebGPU.TextureFormat.R8Unorm;
    InitTextureArray(ref manager.TextureArrays[writeIndex], device, ref arena, nilTextureFormat, 1, 1, 1);
    
    // init the font texture
    writeIndex = VirtualTextureManager.FontTextureArrayIndex;
    /**
        Font textures are an array of bytes ranging from 0-256 for one channel ('red' - relative to the gpu - as it is a one channel value).
        These values should be normalised in the shader; converting 0-256 to 0-1.
    **/
    WebGPU.TextureFormat fontTextureFormat = WebGPU.TextureFormat.R8Unorm;
    InitTextureArray(
        ref manager.TextureArrays[writeIndex], device, ref arena, fontTextureFormat, fontInfo.TextureWidth, fontInfo.TextureHeight, fontInfo.MaxFonts
    );
    
    // image textures.
    /**
        Image textures are an array of four bytes for each channel (red, green, blue, alpha) ranging from 0-256.
        These values should be normalised in the shader; converting 0-256 to 0-1.
    **/
    writeIndex = VirtualTextureManager.ImageTextureArrayStartIndex;
    WebGPU.TextureFormat imageTextureFormat = WebGPU.TextureFormat.Rgba8Unorm;
    for(int i = 0; i < imageInfos.Length; i++){
        ref ImageTexturesInitInfo createInfo = ref imageInfos[i];
        InitTextureArray(
            ref manager.TextureArrays[writeIndex], device, ref arena, imageTextureFormat, createInfo.Width, createInfo.Height, createInfo.MaxTextures
        );
        writeIndex++;
    }
    
    manager.IsInitialised = true; 
}

/// <summary>
///     Creates a texture array inside a virtual texture array manager to map physical textures to virtual textures when loaded.
/// </summary>
public static void InitTextureArray(
    ref TextureArray array, Device device, ref Memory.Arena arena, WebGPU.TextureFormat format, uint width, uint height, uint layerCount
){

    // validation steps.
    Debug.Assert(device.IsInitialised, 
        "Web GPU cannot initialise a texture array for a virtual texture manager with an unintialised device."
    );
    Debug.Assert(array.IsInitialised==false,
        "Web GPU cannot initialise a texture array that is already initialised."
    );

    // create the texture.
    WebGPU.TextureDescriptor textDesc = default;
    textDesc.Size = new(){Width = width, Height = height, DepthOrArrayLayers = layerCount};
    textDesc.MipLevelCount = 1;
    textDesc.SampleCount = 1;
    textDesc.Dimension = WebGPU.TextureDimension.Dimension2D;
    textDesc.Format = format;
    textDesc.Usage = WebGPU.TextureUsage.CopyDst | WebGPU.TextureUsage.TextureBinding;
    textDesc.ViewFormatCount = 0;
    array.Pointer = WebGPUApi.DeviceCreateTexture(device.Pointer, &textDesc);

    // create the view.
    WebGPU.TextureViewDescriptor viewDesc = default;
    viewDesc.Format = format;
    viewDesc.Dimension = WebGPU.TextureViewDimension.Dimension2DArray;
    viewDesc.BaseMipLevel = 0;
    viewDesc.MipLevelCount = 1;
    viewDesc.BaseArrayLayer = 0;
    viewDesc.ArrayLayerCount = layerCount;
    viewDesc.Aspect = WebGPU.TextureAspect.All;
    array.View = WebGPUApi.TextureCreateView(array.Pointer, &viewDesc);

    // create the free texture indices for all textures.
    Collections.Init(ref array.FreeLayerIndices, ref arena, (int)layerCount);
    for(int i = 0; i < layerCount; i++){
        Collections.Push(ref array.FreeLayerIndices, i);
    }

    array.Extents = textDesc.Size;
    array.IsInitialised = true;
}

public static void WriteToTextureArray(
    Device device, TextureArray array, WebGPU.TextureFormat format, uint layerIndex, Array<byte> hostBufferToCopy
){
    
    // validation steps.
    Debug.Assert(device.IsInitialised,
        "Web GPU cannot write to a texture array with an uninitialised device."
    );
    Debug.Assert(array.IsInitialised,
        "Web GPU cannot write to an unintialised texture array."
    );

    // define where in the texture to write to.
    WebGPU.ImageCopyTexture dst = default;
    dst.Texture = array.Pointer;
    dst.MipLevel = 0;
    dst.Origin = new(){X = 0, Y = 0, Z = layerIndex};
    dst.Aspect = WebGPU.TextureAspect.All;

    // describe the layout of the host pixel buffer.
    uint bytesPerPixel = 4;
    switch(format){
        case WebGPU.TextureFormat.Rgba8Unorm:
            bytesPerPixel = 4;
        break;
        case WebGPU.TextureFormat.R8Unorm:
            bytesPerPixel = 1;
        break;
        default:
            // this should crash immediately.
            Debug.Panic($"Writing a '{format}' host buffer to a gpu device buffer has not been implemented.");
        break;
    } 
    WebGPU.TextureDataLayout layout = default;
    layout.Offset = 0;
    layout.BytesPerRow = array.Extents.Width * bytesPerPixel;
    layout.RowsPerImage = array.Extents.Height;

    // define te region size we are replacing (1 layer at a time)
    WebGPU.Extent3D writeSize = array.Extents;
    writeSize.DepthOrArrayLayers = 1;

    // note that this command is an immediate schedule and dispatch, there is no need to call queue submit.
    WebGPUApi.QueueWriteTexture(device.Queue, &dst, hostBufferToCopy.Pointer, (uint)hostBufferToCopy.Length, &layout, &writeSize);
    WebGPUApi.QueueSubmit(device.Queue, 0, null);
}

public static void SetVirtualTextureFilePath(
    ref RendererCtx ctx, String filePath, int virtualTextureId
){
    SetVirtualTextureFilePath(ref ctx.VirtualTextureManager, filePath, virtualTextureId);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetVirtualTextureFilePath(
    ref VirtualTextureManager manager, String filePath, int virtualTextureId
){
    if(virtualTextureId == 0){
        Debug.Panic("Web GPU renderer attempted to set the texture path of the Nil virtual texture.");
        return;
    }
    ref String dst = ref manager.VirtualTextureFilePaths[virtualTextureId];
    String.Clear(ref dst);
    String.Append(ref dst, filePath);
}

public static bool LoadImageTexture(
    ref RendererCtx ctx, int virtualTextureId
){
    return LoadImageTexture(ref ctx.VirtualTextureManager, GetChosenDevice(ref ctx), virtualTextureId);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool LoadImageTexture(
    ref VirtualTextureManager manager, Device device, int virtualTextureIndex
){
    
    // validation steps.
    Debug.Assert(device.IsInitialised,
        $"Web GPU cannot load virtual texture '{virtualTextureIndex}' with an unintialised device"
    );
    Debug.Assert(manager.IsInitialised,
        $"Web GPU cannot load virtual texture '{virtualTextureIndex}' to an unintialised virtual texture manager"
    );
    if(virtualTextureIndex == 0){
        Debug.Panic("Web GPU attempted to load the Nil virtual texture.");
        return false;
    }
    ref VirtualTexture vT = ref manager.VirtualTextures[virtualTextureIndex];
    if(vT.IsLoaded == 1){
        Debug.Panic("Web GPU attempted to load a texture that is already loaded.");
        return false;
    }
    if(IsImageVirtualTexture(ref manager, virtualTextureIndex)==false){
        Debug.Panic($"Virtual texture '{virtualTextureIndex}' is not an image texture and cannot be loaded as one.");
        return false;
    }

    // load the image file.
    bool isValid = false;
    String filePath = manager.VirtualTextureFilePaths[virtualTextureIndex];
    N_IO.Image image = N_IO.IO.LoadImage(filePath, ref isValid);
    if(isValid != true){
        Debug.Panic($"Failed to read image file: '{String.ToSystemString(filePath)}'");
        return false;
    }
    
    // validate that the image can be stored.
    uint width = (uint)image.Width;
    uint height = (uint)image.Height;
    int textureArrayBinding = -1;
    for(int i = VirtualTextureManager.ImageTextureArrayStartIndex; i < manager.TextureArrays.Length; i++){
        ref TextureArray array = ref manager.TextureArrays[i];
        if(array.Extents.Width == width && array.Extents.Height == height){
            textureArrayBinding = i;
            break;
        }
    }
    if(textureArrayBinding == -1){
        Debug.Panic(
            $"Failed to load image texture; resolution [width = '{width}', height = '{height}'] has not been registered. file path: '{String.ToSystemString(filePath)}'"
        );
        N_IO.IO.FreeImage(ref image);
        return false;
    }

    ref TextureArray textureArray = ref manager.TextureArrays[textureArrayBinding]; 

    // get the next available slot to load into.
    if(textureArray.FreeLayerIndices.Count == 0){
        Debug.Panic($"Web Gpu memory limit hit: cannot load any more images of resolution [width = '{width}', height = '{height}']");
        N_IO.IO.FreeImage(ref image);
        return false;
    }
    vT.ShaderTextureArrayBinding = textureArrayBinding;
    vT.TextureArrayLayerIndex = Collections.Pop(ref textureArray.FreeLayerIndices);
    vT.IsLoaded = 1;

    // write the pixel data to the texture array.
    WriteToTextureArray(device, textureArray, WebGPU.TextureFormat.Rgba8Unorm, (uint)vT.TextureArrayLayerIndex, image.Pixels);
    N_IO.IO.FreeImage(ref image);
    return true;
}

public static bool UnloadImageTexture(
    ref RendererCtx ctx, int virtualTextureId
){
    return UnloadImageTexture(ref ctx.VirtualTextureManager, virtualTextureId);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool UnloadImageTexture(
    ref VirtualTextureManager manager, int virtualTextureIndex
){  
    // validation steps.
    Debug.Assert(manager.IsInitialised,
        $"Web GPU cannot unload virtual texture '{virtualTextureIndex}' from an unintialised virtual texture manager."
    ); 
    ref VirtualTexture vT = ref manager.VirtualTextures[virtualTextureIndex];
    if(vT.IsLoaded == 0){
        Debug.Panic($"Web GPU attempted to unload virtual texture '{virtualTextureIndex}', which is already unloaded.");
        return false;
    }
    if(IsImageVirtualTexture(ref manager, virtualTextureIndex)){
        Debug.Panic($"Virtual texture '{virtualTextureIndex}' is not an image texture and cannot be unloaded like one.");
        return false;
    }

    // push the freed layer index back into the texture array for reuse.
    Collections.Push(ref manager.TextureArrays[vT.ShaderTextureArrayBinding].FreeLayerIndices, vT.TextureArrayLayerIndex);
    vT.IsLoaded = 0;
    return true;
}

public static bool LoadFontTexture(
    ref RendererCtx ctx, int virtualTextureIndex, uint glyphHeightInPixels
){
    return LoadFontTexture(ref ctx.VirtualTextureManager, ref ctx.TransientArena, GetChosenDevice(ref ctx), virtualTextureIndex, glyphHeightInPixels);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool LoadFontTexture(
    ref VirtualTextureManager manager, ref Memory.Arena transient,  Device device, int virtualTextureIndex, uint glyphHeightInPixels
){
    
    // validation steps.
    Debug.Assert(device.IsInitialised,
        $"Web GPU cannot load virtual texture '{virtualTextureIndex}' with an unintialised device"
    );
    Debug.Assert(manager.IsInitialised,
        $"Web GPU cannot load virtual texture '{virtualTextureIndex}' to an unintialised virtual texture manager"
    );
    if(virtualTextureIndex == 0){
        Debug.Panic("Web GPU attempted to load the Nil virtual texture.");
        return false;
    }
    ref VirtualTexture vT = ref manager.VirtualTextures[virtualTextureIndex];
    if(vT.IsLoaded == 1){
        Debug.Panic("Web GPU attempted to load a texture that is already loaded.");
        return false;
    }

    Array<byte> textureData = default;
    ref TextureArray fontTextureArray = ref manager.TextureArrays[VirtualTextureManager.FontTextureArrayIndex];
#pragma warning disable
    Memory.Arena.ClearZeroed(ref transient);
    uint textureWidth = fontTextureArray.Extents.Width;
    uint textureHeight = fontTextureArray.Extents.Height;
    Collections.Init(ref textureData, ref transient, (int)textureWidth * (int)textureHeight); 
#pragma warning enable
    ref FontData fontData = ref manager.VirtualTextureFontData[virtualTextureIndex];

    // load the font file bitmap into a texture.
    if(Font.LoadFont(
            manager.VirtualTextureFilePaths[virtualTextureIndex], ref fontData, ref textureData, textureWidth, glyphHeightInPixels
        ) != true
    ){
        return false;
    } 

    ref TextureArray textureArray = ref manager.TextureArrays[VirtualTextureManager.FontTextureArrayIndex];

    // get the next available slot to load into.
    if(textureArray.FreeLayerIndices.Count == 0){
        Debug.Panic($"Web Gpu memory limit hit: cannot load any more font textures.");
        return false;
    }
    vT.ShaderTextureArrayBinding = VirtualTextureManager.FontTextureArrayIndex;
    vT.TextureArrayLayerIndex = Collections.Pop(ref textureArray.FreeLayerIndices);

    // write the data into the texture array.
    WriteToTextureArray(device, textureArray, WebGPU.TextureFormat.R8Unorm, (uint)vT.TextureArrayLayerIndex, textureData);

    vT.IsLoaded = 1;
    return true;
}

public static bool IsImageVirtualTexture(
    ref VirtualTextureManager manager, int virtualTextureIndex
){
    return manager.VirtualTextureTypes[virtualTextureIndex] == VirtualTextureType.Image;
}

public static bool IsFontVirtualTexture(
    ref VirtualTextureManager manager, int virtualTextureIndex
){
    return manager.VirtualTextureTypes[virtualTextureIndex] == VirtualTextureType.Font;
}

public static void InitFinalRenderTarget(
    ref RendererCtx ctx, uint width, uint height
){
    if(ctx.FinalRenderTexture.IsIntialised){
        FreeTexture(ref ctx.FinalRenderTexture);
    }

    ctx.FinalRenderTexture = CreateTexture(
        GetChosenDevice(ref ctx), WebGPU.TextureFormat.Bgra8UnormSrgb, WebGPU.TextureUsage.RenderAttachment | WebGPU.TextureUsage.TextureBinding,
        WebGPU.TextureAspect.All, width, height
    );

    InitDepthTexture(ref ctx, width, height);
}

public static float GetFinalRenderTextureAspectRatio(
    RendererCtx ctx
){
    { // validation.
        Debug.Assert(ctx.FinalRenderTexture.IsIntialised, 
            "Attempted to calculate the aspect ratio of an uninitialised final render target."
        );
    }
    return (float)ctx.FinalRenderTexture.Extents.Width / ctx.FinalRenderTexture.Extents.Height;
}

/**##########################################################################################################################################
    div: SAMPLERS.
##########################################################################################################################################**/

public static void InitSampler(
    ref WebGPU.Sampler* sampler, Device device  
){
    // validation steps.
    Debug.Assert(device.IsInitialised==true,
        "Web GPU cannot initialise a sampler with a unintialised device."
    );
    Debug.Assert(sampler == null, 
        "Web Gpu should not initialise a sampler that has already been initialised."
    );
    
    WebGPU.SamplerDescriptor desc = default;
    /**
        the addressing modde can be specified per axis; note that the axes are called 
        {U,V,W} instead of {X,Y,Z}.
    **/
    desc.AddressModeU = WebGPU.AddressMode.Repeat;
    desc.AddressModeV = WebGPU.AddressMode.Repeat;
    /**
        The mag and min filter specify how to interpolate texels that are magnified or minified.
        The magnigation concerns oversampling problems while minification concerns undersampling.
    **/
    desc.AddressModeW = WebGPU.AddressMode.Repeat;
    desc.MagFilter = WebGPU.FilterMode.Nearest;
    desc.MinFilter = WebGPU.FilterMode.Nearest;
    desc.LodMinClamp = 1.0f;
    desc.LodMaxClamp = 1.0f;
    desc.Compare = WebGPU.CompareFunction.Undefined;
    desc.MaxAnisotropy = 1;
    
    sampler = WebGPUApi.DeviceCreateSampler(device.Pointer, &desc);
}

/**##########################################################################################################################################
    div: SPRITES
##########################################################################################################################################**/

public static void InitSpriteManager(
    ref SpriteManager manager, Device device, ref Memory.Arena arena, Array<SpriteLayerCreateInfo> layerInfos
){
    
    // validation step.
    Debug.Assert(device.IsInitialised==true, "Web GPU cannot initialise a sprite manager with an unintialised device");
    Debug.Assert(manager.IsInitialised==false, "Web GPU should not intialise a sprite manager that has already been initialised.");
    Debug.Assert(manager.IsInitialised==false, "Web GPU sprite manager attempted to initialise an already initialised sprite manager.");
    Debug.Assert(layerInfos.IsInitialised==true, "Web GPU cannot create a sprite with an unintialised create info array.");

    int maxSprites = 0;
    for(int i = 0; i < layerInfos.Length; i++){
        // this should crash the program.
        Debug.Assert(layerInfos[i].MaxSprites > 0, "Web GPU cannot intialise a sprite layer with less than 1 sprite.");
        maxSprites += layerInfos[i].MaxSprites;
    }

    // this should be an immediate crash.
    Debug.Assert(maxSprites<=Sprite.MaxAmount, $"Web GPU sprite manager cannot be initialised with more than '{Sprite.MaxAmount}'; user requested '{maxSprites}'.");
    // this should pass.
    Debug.Assert(maxSprites>=2, "Web GPU sprite manager must be initialised with two or more sprites.");
    maxSprites = Math.Clamp(maxSprites, 2, Sprite.MaxAmount);

    manager.SpriteBuffer = CreateBuffer<Sprite>(
        device, WebGPU.BufferUsage.CopySrc | WebGPU.BufferUsage.MapWrite, 
        WebGPU.BufferUsage.CopyDst | WebGPU.BufferUsage.Storage, (uint)maxSprites
    );
    Collections.Init(ref manager.Sprites, ref arena, maxSprites);
    Collections.Init(ref manager.SpriteGenerations, ref arena, maxSprites);
    Collections.Init(ref manager.ChainSprites, ref arena, maxSprites);
    Collections.Init(ref manager.SortedSprites, ref arena, maxSprites);
    Collections.Init(ref manager.SpriteLayers, ref arena, layerInfos.Length);
    Collections.Init(ref manager.OneFrameSpritesIndices, ref arena, maxSprites);
    // exclude the Nil sprite.
    int freeIndex = 1;
    for(int i = 0; i < manager.SpriteLayers.Length; i++){
        ref SpriteLayer layer = ref manager.SpriteLayers[i];
        ref SpriteLayerCreateInfo createInfo = ref layerInfos[i];

        layer.MaxSprites = createInfo.MaxSprites;
        Collections.Init(ref layer.FreeSpritesIndices, ref arena, layer.MaxSprites);
        // push the free indices.
        if(i == 0){
            // exclude the Nil sprite.
            for(int j = 1; j < layer.MaxSprites; j++){
                Collections.Push(ref layer.FreeSpritesIndices, freeIndex);
                freeIndex++;
            }
        }
        else{
            for(int j = 0; j < layer.MaxSprites; j++){
                Collections.Push(ref layer.FreeSpritesIndices, freeIndex);
                freeIndex++;
            }
        }
    }
    manager.IsInitialised = true;
}

public static SpriteId AllocateSprite(
    ref RendererCtx ctx, int layer, ref bool isValidOutput
){
    return AllocateSprite(ref ctx.SpriteManager, layer, ref isValidOutput);
}

[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
public static SpriteId AllocateSprite(
    ref SpriteManager manager, int layer, ref bool isValidOutput
){
    
    { // validation
        AssertInitialisedSpriteManager(manager);
    }

    // get an index to assign to the 
    ref StackArray<int> freeIndices = ref manager.SpriteLayers[layer].FreeSpritesIndices; 
    if(freeIndices.Count == 0){
        Debug.Panic("Web Gpu renderer sprite manager memory limit hit; cannot allocate more sprites.");
        isValidOutput = false;
        return default;
    }

    int spriteIndex = Collections.Pop(ref freeIndices);
    ref Sprite sprite = ref manager.Sprites[spriteIndex]; 
    sprite.State = SpriteState.Inactive;
    sprite.Layer = layer;
    isValidOutput = true;
    // identity matrix.
    return new(){GenId = new(spriteIndex, manager.SpriteGenerations[spriteIndex]), Layer = layer};
}

public static SpriteId AllocateOneFrameSprite(
    ref RendererCtx ctx, int layer, ref bool isValidOutput
){
    return AllocateOneFrameSprite(ref ctx.SpriteManager, layer, ref isValidOutput);
}

public static SpriteId AllocateOneFrameSprite(
    ref SpriteManager manager, int layer, ref bool isValidOutput
){
    SpriteId spriteId = AllocateSprite(ref manager, layer, ref isValidOutput);
    if(isValidOutput){
        Collections.Push(ref manager.OneFrameSpritesIndices, spriteId);
    } 
    return spriteId;
}

public static SpriteId AllocateSpriteChain(
    ref RendererCtx ctx, int chainLength, int layer, ref bool isValidOutput
){
    return AllocateSpriteChain(ref ctx.SpriteManager, chainLength, layer, ref isValidOutput);
}

public static SpriteId AllocateSpriteChain(
    ref SpriteManager manager, int chainLength, int layer, ref bool isValidOutput
){
    SpriteId first = default;
    int previousIndex = 0;
    int firstIndex = 0;
    ref StackArray<int> freeIndices = ref manager.SpriteLayers[layer].FreeSpritesIndices;
    for(int i = 0; i < chainLength; i++){
        SpriteId spriteId = AllocateSprite(ref manager, layer, ref isValidOutput);
        int index = GenId.GetIndex(spriteId.GenId);
        ref ChainSprite previousSprite = ref manager.ChainSprites[previousIndex];
        ref ChainSprite chainSprite = ref manager.ChainSprites[index];
        if(i==0){
            first = spriteId;
            firstIndex = index;
            chainSprite.IsFirst = true;
        }
        else if(i==chainLength){
            // maintain the circular linked list.
            previousSprite.NextSprite = firstIndex;
            chainSprite.IsFirst = false;
        }
        else{
            // maintain the circular linked list.
            previousSprite.NextSprite = index;
            chainSprite.IsFirst = false;
        }
        if(isValidOutput == false){
            Debug.Assert(false, $"Insufficient free sprites on sprite layer '{layer}' to accomodate a the full sprite chain of length '{chainLength}'."); 
            // maintain the circular linked list even on a fail case.
            previousSprite.NextSprite = firstIndex;
            return first;
        }
        previousIndex = index;
    }
    return first;
}

public static bool InitSprite(
    ref RendererCtx ctx, SpriteId spriteId, Transform transform, Colour colour, Region region, 
    ColourState colourState, int virtualTextureIndex, int materialIndex, bool isActive 
){
    return InitSprite(
        ref ctx.SpriteManager, spriteId, transform, colour, region, 
        colourState, virtualTextureIndex, materialIndex, isActive
    );
}

public static bool InitSprite(
    ref SpriteManager manager, SpriteId spriteId, Transform transform, Colour colour, Region region, 
    ColourState colourState, int virtualTextureIndex, int materialIndex, bool isActive
){

    int index = GenId.GetIndex(spriteId.GenId);
    int gen = GenId.GetGeneration(spriteId.GenId);

    { // validation
        AssertInitialisedSpriteManager(manager);
        AssertValidSpriteId(spriteId);
        AssertValidVirtualTextureIndex(virtualTextureIndex);
        AssertValidMaterialIndex(materialIndex);
        if(gen != manager.SpriteGenerations[index]){
            return false;
        }
    }

    SetSpriteTransformUnsafe(ref manager, index, transform);
    SetSpriteMaterialUnsafe(ref manager, index, materialIndex);
    SetSpriteRegionUnsafe(ref manager, index, region);
    SetSpriteVirtualTextureUnsafe(ref manager, index, virtualTextureIndex);
    SetSpriteActiveUnsafe(ref manager, index, isActive);
    SetSpriteColourUnsafe(ref manager, index, colour);
    SetSpriteColourStateUnsafe(ref manager, index, colourState);
    return true;
}

public static bool InitSpriteString(
    ref RendererCtx ctx, SpriteId spriteId, String text, Transform transform, int virtualTextureIndex, int materialIndex, bool isActive
){
    return InitSpriteString(
        ref ctx.SpriteManager, ref ctx.VirtualTextureManager, spriteId, text, transform, virtualTextureIndex, materialIndex, isActive
    );
}

public static bool InitSpriteString(
    ref SpriteManager sprites, ref VirtualTextureManager textures, SpriteId spriteId, String text, Transform transform, int virtualTextureIndex, int materialIndex, bool isActive
){
    int firstIndex = GenId.GetIndex(spriteId.GenId);
    int generation = GenId.GetGeneration(spriteId.GenId);

    { // validation
        AssertInitialisedSpriteManager(sprites);
        AssertValidChainSpriteId(spriteId, sprites);
        AssertValidFontVirtualTextureIndex(virtualTextureIndex, textures);
        AssertValidMaterialIndex(materialIndex);
        if(sprites.SpriteGenerations[firstIndex] != generation){
            return false;
        }
    }

    int index = firstIndex;
    Vector2 advance = default;
    ref FontData fontData = ref textures.VirtualTextureFontData[virtualTextureIndex];
    Vector2I maxGlyphSize = new(){X = (int)fontData.MaxGlyphHeightInPixels, Y = (int)fontData.MaxGlyphHeightInPixels};
    for(int i = 0; i < text.Count; i++){    

        char c = text[i];
        if(c=='\n'){
            advance.X = 0;
            advance.Y -= fontData.MaxGlyphHeightInPixels;
            continue;
        }
        int glyphDataIndex = (int)c - (int)fontData.BaseGlyphIndex + 1;
        ref Glyph glyphData = ref fontData.Glyphs[glyphDataIndex];
        
        // pixel coords.
        SetSpriteVirtualTextureUnsafe(ref sprites, index, virtualTextureIndex);
        SetSpriteRegionUnsafe(
            ref sprites, index, 
            new(){
                TopLeft = new(){
                    X = glyphData.TextureCoords.X, 
                    Y = glyphData.TextureCoords.Y
                }, 
                BotRight = new(){
                    X = glyphData.TextureCoords.X + glyphData.Size.X, 
                    Y = glyphData.TextureCoords.Y + glyphData.Size.Y
                }
            }
        );

        /**
            Note that the transform code below expects to be handed glyph data that is within rasterised space:
            (X+ = right, Y+ = down). where the origin of the glyph on the font texture is the top left of its quad region.

            As this renderer is in cartesian space (X+ = right, Y+ = up) and the origin of the sprite is at its center, there
            are conversions that must be accounted for.
        **/
        Vector3 scaling = new(){X = (float)glyphData.Size.X/maxGlyphSize.X, Y = (float)glyphData.Size.Y/maxGlyphSize.Y};
        // Calculate the local top-left of the glyph using the glyph data metrics.
        // In Y+ up space, adding Offset.Y pushes the top edge correctly upwards from the baseline.
        float localLeft = advance.X + glyphData.Offset.X;
        float localTop  = advance.Y + glyphData.Offset.Y; 

        // Since sprites have an origin at their center:
        // X moves right (+), but Y must move DOWN (-) to reach the center from the top edge.
        Vector3 positionalOffset = new(){
            X = localLeft + (glyphData.Size.X * 0.5f),
            Y = localTop  - (glyphData.Size.Y * 0.5f), 
            Z = 0.0f
        };

        Transform glyphTransform = default;
        // Apply the base transform's scale to the glyph's dimensions
        glyphTransform.Scale = transform.Scale * new Vector3(){X = glyphData.Size.X, Y = glyphData.Size.Y, Z = 1.0f};

        // Position needs to inherit the base transform's position plus our offset scaled by the base scale
        glyphTransform.Position = transform.Position + (positionalOffset * transform.Scale);
        
        SetSpriteTransformUnsafe(ref sprites, index, glyphTransform);

        // Advance the cursor for the next character, scaled by your base transform scale
        advance += glyphData.Advance * new Vector2(){X = transform.Scale.X * (1f/transform.Scale.X), Y = transform.Scale.Y* (1f/transform.Scale.Y)};

        // other.
        SetSpriteActiveUnsafe(ref sprites, index, isActive);
        SetSpriteMaterialUnsafe(ref sprites, index, materialIndex);

        // next loop iteration preperation.
        ref ChainSprite chainSprite = ref sprites.ChainSprites[index];
        index = chainSprite.NextSprite;
        if(index==firstIndex){
            // Debug.Assert(false, $"Sprite String '{firstIndex}' is too small to fully contain string: '{String.ToSystemString(text)}'");
            return false;
        }
    }

    return true;
}

public static bool DeallocateSprite(
    ref RendererCtx ctx, SpriteId spriteId
){
    return DeallocateSprite(ref ctx.SpriteManager, spriteId);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool DeallocateSprite(
    ref SpriteManager manager, SpriteId spriteId
){
    // validation steps.
    Debug.Assert(manager.IsInitialised,
        "Web GPU cannot deallocate a sprite from an uninitialised sprite manager."
    );
    int generation = GenId.GetGeneration(spriteId.GenId);
    int index = GenId.GetIndex(spriteId.GenId);
    if(index <= 0){
        Debug.Panic($"Attempted to deallocate sprite at index '{index}'; which is not allowed as it is the Nil sprite or invalid.");
        return false;
    }
    if(generation != manager.SpriteGenerations[index]){
        Debug.Panic("Web GPU attempted to deallocate a sprite with a stale index.");
        return false;
    }
    ref Sprite sprite = ref manager.Sprites[index];
    if(sprite.State == SpriteState.Deallocated){
        Debug.Panic("Web GPU attempted to deallocate a sprite that has already been deallocated.");
        return false;
    }
    sprite.State = SpriteState.Deallocated;

    Collections.Push(ref manager.SpriteLayers[spriteId.Layer].FreeSpritesIndices, index);

    return true;
}

public static bool SetSpriteActive(
    ref RendererCtx ctx, SpriteId spriteId, bool isActive
){
    return SetSpriteActive(ref ctx.SpriteManager, spriteId, isActive);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteActive(
    ref SpriteManager manager, SpriteId spriteId, bool isActive
){
    
    int index = GenId.GetIndex(spriteId.GenId);
    int generation = GenId.GetGeneration(spriteId.GenId);

    { // validation
        AssertInitialisedSpriteManager(manager);
        AssertValidSpriteId(spriteId);
        if(generation != manager.SpriteGenerations[index]){
            return false;
        }
    }

    SetSpriteActiveUnsafe(ref manager, index, isActive);
    return true;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypasses all validation checks.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteActiveUnsafe(
    ref SpriteManager manager, SpriteId spriteId, bool isActive
){
    SetSpriteActiveUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), isActive);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteActiveUnsafe(
    ref SpriteManager manager, int spriteIndex, bool isActive
){
    manager.Sprites[spriteIndex].State = isActive? SpriteState.Active : SpriteState.Inactive;
}

public static bool SetSpriteRegion(
    ref RendererCtx ctx, SpriteId spriteId, Region region
){
    return SetSpriteRegion(ref ctx.SpriteManager, spriteId, region);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteRegion(
    ref SpriteManager manager, SpriteId spriteId, Region uv
){
    // validation steps.
    Debug.Assert(manager.IsInitialised,
        "Web GPU cannot set an uninitialised sprite manager's sprite's UV."
    );
    int generation = GenId.GetGeneration(spriteId.GenId);
    int index = GenId.GetIndex(spriteId.GenId);
    if(generation != manager.SpriteGenerations[index]){
        Debug.Panic("Web GPU attempted to set the UV of a sprite with a stale id.");
        return false;
    }

    SetSpriteRegionUnsafe(ref manager, index, uv);
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteRegionUnsafe(
    ref SpriteManager manager, SpriteId spriteId, Region region
){
    SetSpriteRegionUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), region);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteRegionUnsafe(
    ref SpriteManager manager, int spriteIndex, Region region
){
    manager.Sprites[spriteIndex].Region = region;
}

public static bool SetSpriteTransform(
    ref RendererCtx ctx, SpriteId spriteId, Transform transform
){
    return SetSpriteTransform(ref ctx.SpriteManager, spriteId, transform);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteTransform(
    ref SpriteManager manager, SpriteId spriteId, Transform transform
){
    // validation.
    int index = GenId.GetIndex(spriteId.GenId);
    int generation = GenId.GetGeneration(spriteId.GenId);
    Debug.Assert(index != 0, "Web GPU renderer should not set the position of the Nil sprite.");
    if(manager.SpriteGenerations[index] != generation){
        return false;
    }

    SetSpriteTransformUnsafe(ref manager, index, transform);    
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
public static void SetSpriteTransformUnsafe(
    ref SpriteManager manager, SpriteId spriteId, Transform transform
){
    SetSpriteTransformUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), transform);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteTransformUnsafe(
    ref SpriteManager manager, int spriteIndex, Transform transform
){
    manager.Sprites[spriteIndex].Transform = Math.CreateMatrix(transform);
}

public static bool SetSpriteMaterial(
    ref RendererCtx ctx, SpriteId spriteId, int materialIndex
){
    return SetSpriteMaterial(ref ctx.SpriteManager, spriteId, materialIndex);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteMaterial(
    ref SpriteManager manager, SpriteId spriteId, int materialIndex
){

    // validation.
    int index = GenId.GetIndex(spriteId.GenId);
    int generation = GenId.GetGeneration(spriteId.GenId);
    Debug.Assert(index != 0, "Web GPU renderer should not set the material index of the Nil sprite.");
    Debug.Assert(materialIndex != 0, $"Web GPU renderer should not set the Nil material index to sprite sprite '{index}'.");
    if(manager.SpriteGenerations[index] != generation){
        return false;
    }

    SetSpriteMaterialUnsafe(ref manager, index, materialIndex);

    return true;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypasses all validation checks.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteMaterialUnsafe(
    ref SpriteManager manager, SpriteId spriteId, int materialIndex
){
    SetSpriteMaterialUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), materialIndex);
}

public static void SetSpriteMaterialUnsafe(
    ref SpriteManager manager, int spriteIndex, int materialIndex
){
    manager.Sprites[spriteIndex].MaterialIndex = materialIndex;
}

public static bool SetSpriteVirtualTexture(
    ref RendererCtx ctx, SpriteId spriteId, int virtualTextureIndex
){
    return SetSpriteVirtualTexture(ref ctx.SpriteManager, spriteId, virtualTextureIndex);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteVirtualTexture(
    ref SpriteManager manager, SpriteId spriteId, int virtualTextureIndex
){

    // validation.
    int index = GenId.GetIndex(spriteId.GenId);
    int generation = GenId.GetGeneration(spriteId.GenId);
    Debug.Assert(index != 0, "Web GPU should not set the virtual texture of the Nil sprite.");
    Debug.Assert(virtualTextureIndex != 0, $"Web GPU should not set the Nil virtual texture to sprite '{index}'.");
    if(manager.SpriteGenerations[index] != generation){
        return false;
    }

    SetSpriteVirtualTextureUnsafe(ref manager, index, virtualTextureIndex);
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteVirtualTextureUnsafe(
    ref SpriteManager manager, SpriteId spriteId, int virtualTextureIndex
){
    SetSpriteVirtualTextureUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), virtualTextureIndex);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteVirtualTextureUnsafe(
    ref SpriteManager manager, int spriteIndex, int virtualTextureIndex
){
    manager.Sprites[spriteIndex].VirtualTextureIndex = virtualTextureIndex;
}

public static bool SetSpriteColour(
    ref RendererCtx ctx, SpriteId spriteId, Colour colour
){
    return SetSpriteColour(ref ctx.SpriteManager, spriteId, colour);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool SetSpriteColour(
    ref SpriteManager manager, SpriteId spriteId, Colour colour
){

    int index = GenId.GetIndex(spriteId.GenId);
    int gen = GenId.GetGeneration(spriteId.GenId);

    { // validation.
        AssertValidSpriteId(spriteId);
        AssertInitialisedSpriteManager(manager);
        if(manager.SpriteGenerations[index] != gen){
            return false;
        } 
    }

    SetSpriteColourUnsafe(ref manager, index, colour);
    return true;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypasses all validation checks.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteColourUnsafe(
    ref SpriteManager manager, SpriteId spriteId, Colour colour
){
    SetSpriteColourUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), colour);
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypasses all validation checks.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void SetSpriteColourUnsafe(
    ref SpriteManager manager, int spriteIndex, Colour colour
){
    manager.Sprites[spriteIndex].Colour = colour;
}

public static bool SetSpriteColourState(
    ref SpriteManager manager, SpriteId spriteId, ColourState colourState
){
    int index = GenId.GetIndex(spriteId.GenId);
    int gen = GenId.GetGeneration(spriteId.GenId);
    
    { // validation.
        AssertValidSpriteId(spriteId);
        AssertInitialisedSpriteManager(manager);
        if(manager.SpriteGenerations[index] != gen){
            return false;
        }
    }

    return true;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypassses all validation checks.</para>
/// </remarks>
public static void SetSpriteColourStateUnsafe(
    ref SpriteManager manager, SpriteId spriteId, ColourState colourState
){
    SetSpriteColourStateUnsafe(ref manager, GenId.GetIndex(spriteId.GenId), colourState);
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Bypassses all validation checks.</para>
/// </remarks>
public static void SetSpriteColourStateUnsafe(
    ref SpriteManager manager, int spriteIndex, ColourState colourState
){
    manager.Sprites[spriteIndex].ColourState = (int)colourState;
}

/**##########################################################################################################################################
    div: RESOURCE HANDLING
##########################################################################################################################################**/

public static void FreeDeviceResources(
    ref Device device
){
    WebGPUApi.QueueRelease(device.Queue);
    WebGPUApi.DeviceRelease(device.Pointer);
}

public static void FreeAdapterResources(
    ref Adapter adapter
){
    WebGPUApi.AdapterRelease(adapter.Pointer);
}

public static void FreeSpriteManagerResources(
    ref SpriteManager manager
){
    FreeBufferResources(ref manager.SpriteBuffer);
}

public static void FreeVirtualTextureManagerResources(
    ref VirtualTextureManager manager
){
    for(int i = 0; i < manager.TextureArrays.Length; i++){
        FreeTextureArrayResources(ref manager.TextureArrays[i]);
    }
    FreeBufferResources(ref manager.VirtualTextureBuffer);
}

public static void FreeTextureArrayResources(
    ref TextureArray array
){
    WebGPUApi.TextureRelease(array.Pointer);
    WebGPUApi.TextureViewRelease(array.View);
}

/**##########################################################################################################################################
    ASSERTIONS
##########################################################################################################################################**/

[Conditional("DEBUG")]
public static void AssertValidVirtualTextureIndex(
    int virtualTextureIndex
){
    // pass.
    Debug.Assert(virtualTextureIndex != 0, "Attempted usage of the Nil virtual texture index");
    // crash.
    Debug.Assert(virtualTextureIndex > -1, "Attempted usage of an invalid virtual texture index");
}

public static void AssertValidFontVirtualTextureIndex(
    int virtualTextureIndex, VirtualTextureManager manager
){
    AssertValidVirtualTextureIndex(virtualTextureIndex);
    Debug.Assert(IsFontVirtualTexture(ref manager, virtualTextureIndex), "Attempted usage of a image virtual texture for a font virtual texture operation.");
}

[Conditional("DEBUG")]
public static void AssertValidImageVirtualTextureIndex(
    int virtualTextureIndex, VirtualTextureManager manager
){
    AssertValidVirtualTextureIndex(virtualTextureIndex);
    Debug.Assert(IsImageVirtualTexture(ref manager, virtualTextureIndex), "Attempted usage of a font virtual texture for a image virtual texture operation.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedSpriteManager(
    SpriteManager manager
){
    // crash.
    Debug.Assert(manager.IsInitialised, "Attempted usage of an uninitialised sprite manager");
}

[Conditional("DEBUG")]
public static void AssertInitialisedVirtualTextureManager(
    ref VirtualTextureManager manager
){
    // crash.
    Debug.Assert(manager.IsInitialised, "Attempted usage of an unintialised virtual texture manager");
}

[Conditional("DEBUG")]
public static void AssertValidSpriteId(
    SpriteId id
){
    int index = GenId.GetIndex(id.GenId);
    // pass.
    Debug.Assert(index > 0, "Attempted usage of the Nil sprite.");
}

[Conditional("DEBUG")]
public static void AssertValidChainSpriteId(
    SpriteId id, SpriteManager manager
){
    int index = GenId.GetIndex(id.GenId);
    // pass.
    Debug.Assert(index > 0, "Attempted usage of the Nil sprite.");
    // pass
    Debug.Assert(manager.ChainSprites[index].NextSprite > 0, "Attempted usage of a non-chain sprite during a sprite string operation.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedDevice(
    Device device
){
    // crash.
    Debug.Assert(device.IsInitialised, "Attempted usage of an unintialised device");
}

[Conditional("DEBUG")]
public static void AssertValidMaterialIndex(
    int materialIndex
){
    // pass.
    Debug.Assert(materialIndex != 0, "Attempted usage of the Nil material index");
    // crash.
    Debug.Assert(materialIndex > -1, "Attempted usage of an invalid material index");
}

[Conditional("DEBUG")]
public static void AssertInitialisedWindowSurface(
    WindowSurface surface
){
    // crash.
    Debug.Assert(surface.IsInitialised, "Attempted usage of an uninitialised window surface.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedGraphicsPipeline(
    GraphicsPipeline pipeline
){
    // crash.
    Debug.Assert(pipeline.IsInitialised, "Attempted usage of an uninitialised graphics pipeline.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedBlitPipeline(
    BlitPipeline pipeline
){
    // crash.
    Debug.Assert(pipeline.IsInitialised, "Attempted usage of an unintialised blit pipeline.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedRenderCtx(
    RendererCtx ctx
){
    // crash.
    Debug.Assert(ctx.IsInitialised, "Attempted usage of an uninitialised render context.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedAdapter(
    Adapter adapter
){
    // crash.
    Debug.Assert(adapter.IsInitialised, "Attempted usage of an uninitialised GPU adapter.");
}

[Conditional("DEBUG")]
public static void AssertInitialisedTexture(
    Texture texture
){
    // crash
    Debug.Assert(texture.IsIntialised, "Attempted usage of an uninitialised texture.");
}

}