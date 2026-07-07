using System.Diagnostics.CodeAnalysis;
using Howl.Text;
using N_Howl.N_Collections;
using N_Howl.N_Math;
using WebGPU = N_Howl.N_Rendering.N_WebGpu;

namespace N_Howl.N_Rendering;

public struct RendererCtxInitInfo{
    public int MaxVirtualTextures;

    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <para>
    ///         The order of the textures determines their binding value within the shader.
    ///         E.g, texture index 0 = @binding(0), texture index 1 = @binding(1), etc ... 
    ///     </para>
    /// </remarks>
    public Array<ImageTexturesInitInfo> ImageTextureInitInfos;
    public FontTexturesInitInfo FontTexturesInitInfo;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///     <para>
    ///         The order of the layers determines the order that they are drawn.
    ///         E.g, layer index 4 will be above layers 3, 2, 1, etc, ... 
    ///     </para>
    /// </remarks>
    public Array<SpriteLayerCreateInfo> SpriteLayerCreateInfos;
    public int MaxFilePathLength;
    public uint MaxUserUniformBufferSizeInBytes;
    public uint MaxUserStorageBufferSizeInBytes;
    public uint TransientArenaSizeInBytes;
    public uint FinalRenderTextureWidth;
    public uint FinalRenderTextureHeight;
    public String GraphicsPipelineShaderFilePath;
}

public struct ImageTexturesInitInfo{
    public uint Width;
    public uint Height;
    /// <summary>
    ///     The maximum amount of textures of this type that can be loaded at a time.
    /// </summary>
    public uint MaxTextures;
}

public struct SpriteLayerCreateInfo{
    public int MaxSprites;
}

public struct SpriteId{
    public GenId GenId;
    /// <remarks>
    ///    <para><b>Remarks:</b></para>
    ///    <para>Indicates the layer this sprite ID came from and should return to.</para>
    /// </remarks>
    public int Layer; 
}

public struct Region{
    public Vector2I TopLeft;
    public Vector2I BotRight;

    public static bool operator ==(Region lhs, Region rhs){
        return lhs.TopLeft == rhs.TopLeft && lhs.BotRight == rhs.BotRight;
    }

    public static bool operator !=(Region lhs, Region rhs){
        return !(lhs==rhs);
    }

    public override bool Equals(object obj){
        return obj is Region o && o == this;
    }

    public override int GetHashCode(){
        return base.GetHashCode();
    }
}

public struct FontTexturesInitInfo{
    /// <summary>
    ///     the width of the texture to write the glyph data to.
    /// </summary>
    public uint TextureWidth;
    /// <summary>
    ///     the height of the texture to write the glyph data to.
    /// </summary>
    public uint TextureHeight;
    public uint BaseGlyphIndex;
    public int GlyphCount;
    /// <summary>
    ///     The virtual textures to intialise as font textures.
    /// </summary>
    public Array<int> VirtualTextures;
}

public enum ColourState : int{
    Tint = 0,
    Override = 1
}

public enum SpriteType : byte{
    Solo,
    Chain
}
