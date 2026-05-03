using System;
using System.IO;
using System.Text.Json;
using Howl.DataStructures;
using Howl.Debug;
using Howl.Ecs;
using Howl.Graphics;
using Howl.Io;
using Howl.Math;
using Howl.Math.Shapes;
using Microsoft.Win32.SafeHandles;

namespace Howl.LevelManagement.Ldtk;

#nullable enable
public static class LdtkParser
{
    /// <summary>
    ///     Loads an ldtk project file from disc.
    /// </summary>
    /// <param name="path">the file path.</param>
    /// <returns>the parsed project file.</returns>
    public static Dto_Project? LoadDtoProject(byte[] scratchBuffer, string path)
    {
        if (File.Exists(path) != true)
        {
            System.Diagnostics.Debug.Assert(false, $"Project '{path}' does not exist in file directory.");            
            return null;
        }

        // read the level bytes into the level buffer in the state.
        using SafeFileHandle handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read);
        int bytesRead = RandomAccess.Read(handle, scratchBuffer, 0);
        
        // parse only the bytes that were written to.
        Span<byte> byteSlice = scratchBuffer.AsSpan(0, bytesRead);
        Dto_Project? dto = JsonSerializer.Deserialize<Dto_Project>(byteSlice); // Note: JsonSerializer doesnt allow for resuing an object, this will always be a GC allocation :)
        System.Diagnostics.Debug.Assert(dto!=null, $"Failed to deserialise LDTK project '{path}'.");
        
        return dto;
    }

    /// <summary>
    ///     Loads an ldtk project file into a state instance.
    /// </summary>
    /// <param name="state">the parser state to load the project into.</param>
    /// <param name="filePath">the file path relative to <c><see cref="LevelManager.LevelsFolder"/></c></param>
    /// <returns>true, if the project was loaded successfully; otherwise false.</returns>
    public static bool LoadProject(LdtkParserState state, string filePath)
    {
        // Note: C# is an awesome language and doesnt support Span<char> for file reading.
        // so this will always allocate garbage :)
        string pathString = LevelManager.LevelsFolder + filePath;
        
        Dto_Project? dto = LoadDtoProject(state.scratchBuffer, pathString);
        state.ProjectDirectoryPath = Path.GetDirectoryName(pathString);

        if (dto == null)
        {
            return false;
        }

        state.Project = dto; 

        // store the loaded level strings with their indices so that the user can
        // use only the name of the level when loading; instead of having to know the
        // unstable index undereath that ldtk changes depending on how the project is structured.
        state.LevelIdentifierToIndex.Clear();
        for(int i = 0; i < dto.Levels.Length; i++)
        {
            state.LevelIdentifierToIndex.Add(dto.Levels[i].Identifier, i);
        }

        return true;
    }

    /// <summary>
    ///     Loads an ldtk level file from disc.
    /// </summary>
    /// <param name="scratchBuffer">the scratch buffer used when reading the file.</param>
    /// <param name="path">the file path.</param>
    /// <returns>the parsed level file.</returns>
    public static Dto_Level? LoadDtoLevel(byte[] scratchBuffer, string path)
    {
        if (File.Exists(path) != true)
        {
            System.Diagnostics.Debug.Assert(false, $"Level file '{path}' does not exist in file directory.");
            return null;
        }

        // read the level bytes into the level buffer in the state.
        using SafeFileHandle handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read);
        int bytesRead = RandomAccess.Read(handle, scratchBuffer, 0);
        
        // parse only the bytes that were written to.
        Span<byte> byteSlice = scratchBuffer.AsSpan(0, bytesRead);
        Dto_Project? dto = JsonSerializer.Deserialize<Dto_Project>(byteSlice); // Note: JsonSerializer doesnt allow for resuing an object, this will always be a GC allocation :)
        System.Diagnostics.Debug.Assert(dto!=null, $"Failed to deserialise LDTK project '{path}'.");

        byte[] bytes = File.ReadAllBytes(path);
        return JsonSerializer.Deserialize<Dto_Level>(bytes);
    }

    /// <summary>
    ///     Loads a level from a ldtk level file and parses it into a Howl application.
    /// </summary>
    /// <param name="app">the howl application instance to parse into.</param>
    /// <param name="state">the ldtk parser state with the loaded project containing the level.</param>
    /// <param name="entities">the entities to write to.</param>
    /// <param name="sprites">the sprites to write to.</param>
    /// <param name="transforms">the transforms to write to.</param>
    /// <param name="levelIdentifier">the identifier string given to the level in the LDTK project.</param>
    /// <returns>true, if the level was successfully loaded; otherwise false.</returns>
    public static unsafe bool LoadLevel(HowlAppState app, LdtkParserState state, EntityRegistry entities, ComponentArray<Sprite> sprites, ComponentArray<Transform> transforms, string levelIdentifier)
    {
        System.Diagnostics.Debug.Assert(state.Project!=null, $"Cannot load level '{levelIdentifier}' without a loaded project file.");

        // get the project level within the loaded project.
        int levelIndex = state.LevelIdentifierToIndex[levelIdentifier];
        Dto_ProjectLevel projDto = state.Project.Levels[levelIndex];

        // get the path of the actual level to load.
        // Note: C# is an awesome language and doesnt support Span<char> for file reading.
        // so this will always allocate garbage :)
        string pathString = LevelManager.LevelsFolder + projDto.ExternalRelPath;

        // load the level file.
        Dto_Level? level = LoadDtoLevel(state.scratchBuffer, pathString);

        if (level == null)
        {
            return false;
        }

        // parse level into app ecs.
        
        for(int layerIndex = 0; layerIndex < level.LayerInstances.Length; layerIndex++)
        {
            Dto_LayerInstance layer = level.LayerInstances[layerIndex];

            if (layer.AutoLayerTiles.Length > 0) // auto tile layer.
            {
                ParseAutoTiles(app, entities, sprites, transforms, layer.AutoLayerTiles, state.ProjectDirectoryPath, layer.TilesetRelPath, layer.GridSize, state.PixelsPerUnit);
            }
            else if(layer.IntGridCsv.Length > 0 && layer.TilesetRelPath == null) // int grid layer.
            {
                state.ParseLevelIntGrid(app, new IntGridView(layer.Identifier, layer.IntGridCsv.AsSpan(), layer.Width, layer.Height, layer.GridSize));
            }
        }

        return true;
    }

    public static void ParseAutoTiles(HowlAppState app, EntityRegistry entities, ComponentArray<Sprite> sprites, ComponentArray<Transform> transforms, Dto_AutoLayerTile[] tiles, string projectDirectory, string tilesetRelPath, 
        int cellSize, int pixelsPerUnit
    )
    {
        GenId genId = default;

        // allocate the auto tiles.
        for(int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
        {
            string tileMapFilePath = PathUtils.FlattenPath(projectDirectory, tilesetRelPath);

            if(EntityRegistry.Allocate(entities, ref genId) == GenIdResult.Ok)
            {
                Dto_AutoLayerTile tile = tiles[tileIndex];

                Rectangle sourceRect = new Rectangle(tile.Src[0], tile.Src[1], cellSize, cellSize);

                float unitFactored = 1f/pixelsPerUnit;

                Sprite sprite = Renderer.ConstructSprite(app, Colour.White, sourceRect, new Vector2(1,1), 
                    tileMapFilePath, 0, SpriteOrigin.TopLeft, DrawSpace.World
                );

                ComponentArray.Allocate(sprites, entities, genId, sprite);

                Vector2 position = new Vector2(tile.Px[0], -tile.Px[1]) * unitFactored;
                Vector2 scale = Vector2.One * unitFactored;

                Transform transform = new Transform(position, scale, 0);
                ComponentArray.Allocate(transforms, entities, genId, transform);                
            }
        }                
    }
}