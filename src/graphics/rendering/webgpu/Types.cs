using System.Runtime.InteropServices;
using WebGPU = Silk.NET.WebGPU;
using SDL = Silk.NET.SDL;
using Howl;
using N_Howl.N_Math;
using Howl.Text;
using N_Howl.N_Font;
using N_Howl.N_Graphics;
using N_Howl.N_Collections;

namespace N_Howl.N_Rendering.N_WebGpu;

public unsafe struct RendererCtx{
    public WebGPU.Instance* Instance;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Elements are vertically associated with <c>Devices</c>.</para>
    /// </remarks>
    public Array<Adapter> Adapters;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Elements are vertically assocaited with <c>Adapters</c>.</para>
    /// </remarks>
    public Array<Device> Devices;
    public Memory.Arena TransientArena;
    public GraphicsPipeline GraphicsPipeline;
    public BlitPipeline BlitPipeline;
    public Buffer VertexBuffer;
    public Buffer IndexBuffer;
    /// <summary>
    ///     The user defined uniform buffer.
    /// </summary>
    public Buffer UserUniformBuffer;
    /// <summary>
    ///     The user defined storage buffer.
    /// </summary>
    public Buffer UserStorageBuffer;
    public VirtualTextureManager VirtualTextureManager;
    public SpriteManager SpriteManager;
    public WindowSurface WindowSurface;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Note that web gpu has its depth range set from 0 (near) to 1 (far).</para>
    /// </remarks>
    public Texture DepthTexture;
    public Texture FinalRenderTexture;
    /// <summary>
    ///     The desintation rectangle for rendering the final render texture onto the backbuffer.
    /// </summary>
    public Rectangle DestinationRectangle;
    public int ChosenDevice;
    public bool IsInitialised;
}

public unsafe struct GraphicsPipeline{
    public WebGPU.RenderPipeline* RenderPipeline;
    public WebGPU.PipelineLayout* Layout;
    public WebGPU.Sampler* NonFilterSampler;
    public WebGPU.BindGroupLayout* BindGroup0Layout;
    public WebGPU.BindGroupLayout* BindGroup1Layout;
    public WebGPU.BindGroupLayout* BindGroup2Layout;
    public WebGPU.BindGroup* BindGroup0;
    public WebGPU.BindGroup* BindGroup1;
    public WebGPU.BindGroup* BindGroup2;
    public bool IsInitialised;
}

public unsafe struct BlitPipeline{
    /// <summary>
    ///     The amount of entries in @group(0).
    /// </summary>
    public const uint BindGroupEntryCount = 2;
    /// <summary>
    ///     The binding of the sampler object in @group(0).
    /// </summary>
    public const uint SamplerBinding = 0;
    /// <summary>
    ///     The binding of the texture object in @group(0).
    /// </summary>
    public const uint TextureBinding = 1;
    public WebGPU.RenderPipeline* RenderPipeline;
    public WebGPU.Sampler* Sampler;
    public WebGPU.BindGroupLayout* BindGroupLayout;
    public WebGPU.BindGroup* BindGroup;
    public bool IsInitialised;
}

/// <summary>
///     The adapter provides information about the underlying implementation and harware,
///     specifying what its capabilities are.
/// </summary>
public unsafe struct Adapter{
    /// <summary>
    ///     A pointer to the underlying web gpu resource.
    /// </summary>
    public WebGPU.Adapter* Pointer;
    /// <summary>
    ///     Describe the maximum and minimum values that may limit the behaviour of the
    ///     underlying GPU and its driver.
    /// </summary>
    public WebGPU.SupportedLimits SupportedLimits;
    /// <summary>
    ///     Non mandatory extensions of web gpu, that adapters may or may not support.
    /// </summary>
    public Array<WebGPU.FeatureName> Features;
    /// <summary>
    ///     Extra information about the adapter; typically used facing, like its name, vendor, etc.
    /// </summary>
    public WebGPU.AdapterProperties Properties;
    public bool IsInitialised;
}

public unsafe struct Device{
    /// <summary>
    ///     A pointer to the underlying web gpu resource.
    /// </summary>
    public WebGPU.Device* Pointer;
    /// <summary>
    ///     The queue used to send both commands and data to the gpu.
    /// </summary>
    public WebGPU.Queue* Queue;
    public bool IsInitialised;
}

// force layout to match the unmanged C layout of webgpu.
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DeviceInitCtx{
    public WebGPU.Device** Device;
    public bool RequestEnded;
    public bool IsValid;
}

public unsafe struct SurfaceTexture{
    public WebGPU.SurfaceTexture WTexture;
    public WebGPU.TextureView* View;
    public WebGPU.Extent3D Extents;
}

public unsafe struct Buffer{
    public WebGPU.Buffer* Host;
    public WebGPU.Buffer* Device;
    /// <summary>
    ///     The length of the <c>Host</c> and <c>Device</c> buffers respectively.
    /// </summary>
    public uint LengthInBytes;
    /// <summary>
    ///     The count of the <c>Host</c> and <c>Device</c> buffers respectively.
    /// </summary>
    public uint CountInBytes;
}

// force layout to match the unmanaged C layout of webgpu.
[StructLayout(LayoutKind.Sequential, Size = 24)]
public struct Vertex{
    public Vector3 Position;
    public Vector2 UV;
    public static readonly uint OffsetOfPosition = (uint)Marshal.OffsetOf<Vertex>(nameof(Position));
    public static readonly uint OffsetOfUV = (uint)Marshal.OffsetOf<Vertex>(nameof(UV));
}

// force layout to match the unmanaged C layout of webgpu.
[StructLayout(LayoutKind.Sequential)]
public struct MapAsyncCtx{
    public bool RequestEnded;
    public bool IsValid;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>The value of a vertex shader location is the same as the respective WGSL attribute <c>@location(g)</c> in the shader.</para>
/// </remarks>
public struct ShaderVertexLocation{
    public const int Position = 0;
    public const int UV = 1;
    public const int SpriteIndex = 2;
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>The value of a shader binding is the same as the respective WGSL attribute <c>@group(g)</c> in the shader.</para>
/// </remarks>
public enum ShaderGroup : uint{
    Buffers = 0,
    TextureArrays = 1,
    Utilities = 2
}

/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>The value of a shader binding is the same as the respective WGSL attribute <c>@binding(b)</c> in the shader.</para>
/// </remarks>
public enum ShaderBinding : uint{
    UserUniform = 0,
    UserStorage = 1,
    VirtualTexturesUniform = 2,
    SpritesStorage = 3,
    NonFilterSampler = 0
} 

/// <summary>
///     The amount of bindings a shader group has.
/// </summary>
/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>Note that the <c>TextureArrays</c> group is not presesnt has it is dependent upon the user.</para>
/// </remarks>
public enum ShaderBindingCount : uint{
    Buffers = 4,
    Utilities = 1
}

public unsafe struct Texture{
    public WebGPU.Texture* Pointer;
    public WebGPU.TextureView* View;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>This is a copied value from texture creation and wont change the underlying texture extents.</para>
    /// </remarks>
    public WebGPU.Extent3D Extents;
    public bool IsIntialised;
}

public unsafe struct TextureArray{
    public WebGPU.Texture* Pointer;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>This is a copied value from texture creation and wont change the underlying texture extents.</para>
    /// </remarks>
    public WebGPU.TextureView* View;
    public WebGPU.Extent3D Extents;
    public StackArray<int> FreeLayerIndices;
    public bool IsInitialised;
}

// pack to 16 for web gpu uniform buffer arrays.
[StructLayout(LayoutKind.Sequential, Size = 16, Pack = 4)]
public struct VirtualTexture{
    /// <summary>
    ///     The maximum amount of virtual textures a shader can store.
    /// </summary>
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <para>
    ///         This is because a virtual texture's size is 16 bytes and the default max size of a WebGPU uniform buffer is 64Kib;
    ///         so only 4096 unique texture ids can be stored; which is more than enough for most games.
    ///     </para>
    /// </remarks>
    public const int MaxAmount = 4096;
    public int ShaderTextureArrayBinding;
    public int TextureArrayLayerIndex;
    public int IsLoaded;
    /// <summary>
    ///     This is padding for the wgsl uniform buffer; grabage data.
    /// </summary>
    public int Padding0;
}

public struct VirtualTextureManager{
    /// <summary>
    ///     The index where font textures are stored within the <c>TextureArrays</c> array.
    /// </summary>
    public const int FontTextureArrayIndex = 1;
    /// <summary>
    ///     The index where the first image textures are stored within the <c>TextureArrays</c> array.
    /// </summary>
    public const int ImageTextureArrayStartIndex = FontTextureArrayIndex + 1;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type="bullet">
    ///         <item>Contains a Nil element.</item>
    ///         <item>
    ///             Elements are vertically associated with <c>VirtualTextureFilePaths</c>, 
    ///             <c>VirtualTextureTypes</c> and <c>VirtualTextureFontData</c>.
    ///         </item>
    ///     </list>
    /// </remarks>
    public Array<VirtualTexture> VirtualTextures;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type="bullet">
    ///         <item>Contains a Nil element.</item>
    ///         <item>
    ///             Elements are vertically associated with <c>VirtualTextures</c>, 
    ///             <c>VirtualTextureTypes</c> and <c>VirtualTextureFontData</c>.
    ///         </item>
    ///     </list>
    /// </remarks>
    public Array<String> VirtualTextureFilePaths;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type="bullet">
    ///         <item>Contains a Nil element.</item>
    ///         <item>
    ///             Elements are vertically associated with <c>VirtualTextures</c>, 
    ///             <c>VirtualTextureTypes</c> and <c>VirtualTextureFilePaths</c>.
    ///         </item>
    ///     </list>
    /// </remarks>
    public Array<FontData> VirtualTextureFontData;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type="bullet">
    ///         <item>Contains a Nil element.</item>
    ///         <item>
    ///             Elements are vertically associated with <c>VirtualTextures</c>, 
    ///             <c>VirtualTextureFilePaths</c> and <c>VirtualTextureFontData</c>.
    ///         </item>
    ///     </list>
    /// </remarks>
    public Array<VirtualTextureType> VirtualTextureTypes;
    public Array<TextureArray> TextureArrays;
    public Buffer VirtualTextureBuffer;
    public bool IsInitialised;
}

/**
    WGSL requires that the total size of a struct must be a multiple of its largest member's alignment

    Remember to look at alignments and sizes when changing this: 
    https://www.w3.org/TR/WGSL/#alignment-and-size
**/
[StructLayout(LayoutKind.Sequential, Size = 128)]
public unsafe struct Sprite : System.IComparable<Sprite>{
    /// <summary>
    ///     The maximum amount of sprites a shader can store.
    /// </summary>
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <para>
    ///         This is because a sprite's size is 80 bytes and the default max size of a WebGPU SSBO is 128Mb;
    ///         so only 1,677,721 unique sprites can be stored; which is more than enough for most games.
    ///     </para>
    /// </remarks>
    public const int MaxAmount = 1677721;
    public Matrix4x4 Transform;
    public Region Region;
    public Colour Colour;
    public SpriteState State;
    public int VirtualTextureIndex;
    public int MaterialIndex;
    public int ColourState;
    public int Layer;

    public int CompareTo(
        Sprite other
    ){
        /**
            Sorts sprites in descending order by their Z translation value. Note that this is specically so that transparent objects 
            are sorted correctly as the depth buffer freaks out with tranparency. However, this will only work when the camera is 
            facing down Z+ (e.g, camera position: {0,0,-3} and looking at position {0,0,0}). As the camera is not expected to rotate
            move from its fixed position; this is okay.
        **/
        return other.Transform.M[14].CompareTo(Transform.M[14]);
    }
}

public enum SpriteState : int{
    Deallocated = 0,
    Inactive = 1,
    Active = 2
}

public struct SpriteManager{
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type = "bullet">
    ///         <item>Contains a <c>Nil</c> element.</item>    
    ///         <item>Elements vertically align with <c>SpriteGenerations</c> and <c>ChainSprites</c>.</item>    
    ///     </list>
    /// </remarks>
    public Array<Sprite> Sprites;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type = "bullet">
    ///         <item>Contains a <c>Nil</c> element.</item>    
    ///         <item>Elements vertically align with <c>Sprites</c> and <c>ChainSprites</c>.</item>    
    ///     </list>
    /// </remarks>
    public Array<int> SpriteGenerations;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <list type = "bullet">
    ///         <item>Contains a <c>Nil</c> element.</item>
    ///         <item>Elements are associated as a circular linked list.</item>
    ///         <item>Elements vertically align with <c>Sprites</c> and <c>GlyphSprites</c>.</item>    
    ///     </list>
    /// </remarks>
    public Array<ChainSprite> ChainSprites;
    /// <summary>
    ///     The indices of sprites in the <c>Sprites</c> array that are active for a single frame then deallocated one completed.
    /// </summary>
    public StackArray<SpriteId> OneFrameSpritesIndices;
    /// <summary>
    ///     A scratch buffer for all the sorted sprites.
    /// </summary>
    public Array<Sprite> SortedSprites;
    public Array<SpriteLayer> SpriteLayers;
    public Buffer SpriteBuffer;
    public bool IsInitialised;
}

public struct SpriteLayer{
    public int MaxSprites;
    public StackArray<int> FreeSpritesIndices;
}

public enum VirtualTextureType{
    Image,
    Font
}

public struct ChainSprite{
    /// <summary>
    ///     The index of the next glyph sprite associated with this one.
    /// </summary>
    public int NextSprite;
    /// <summary>
    ///     Whether or not this is the first sprite in the sprite chain.
    /// </summary>
    public bool IsFirst;
}

public unsafe struct WindowSurface{
    public WebGPU.Surface* Surface;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Mutating this value will not change the actual windows extents; this is a copied value from initialisation.</para>
    /// </remarks>
    public WebGPU.Extent3D WindowExtents;
    public bool IsInitialised;
}
