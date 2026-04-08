using System;
using Howl.Math;
using Howl.Ecs;

namespace Howl.Graphics;

public interface ITextureManager : IDisposable
{
    /// <summary>
    /// Loads a new Texture (.png or .jpg) asset into memory.
    /// </summary>
    /// <param name="texturePath">The file path to the Texture asset; relative to AssetManager.AssetsFolder</param>
    /// <param name="genIndex">The GenIndex assigned to the texture that was loaded.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseAlreadyAllocated"/>
    ///             </description>
    ///         </item>
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item> 
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult LoadTexture(string texturePath, out GenIndex genIndex);

    /// <summary>
    /// Unloads a Texture asset from memeory.
    /// </summary>
    /// <param name="index">The GenIndex associated with the loaded Texture.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public GenIndexResult UnloadTexture(in GenIndex index);

    /// <summary>
    /// Gets the dimensions/resolution of a loaded texture.
    /// </summary>
    /// <param name="genIndex">The loaded texture's GenIndex.</param>
    /// <param name="dimensions">The horizontal (x) and vertical (y) dimensions of the texture - in pixels.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.DenseNotAllocated"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.StaleGenIndex"/>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="GenIndexResult.Ok"/>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public abstract GenIndexResult GetTextureDimensions(in GenIndex genIndex, out Vector2 dimensions);
}