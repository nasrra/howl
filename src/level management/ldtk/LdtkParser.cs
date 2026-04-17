using System.IO;
using System.Text.Json;

namespace Howl.LevelManagement.Ldtk;

#nullable enable
public static class LdtkParser
{
    public static Dto_Project? LoadProject(string path)
    {
        if (File.Exists(path) != true)
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);           
        Dto_Project? dto = JsonSerializer.Deserialize<Dto_Project>(bytes);
        return dto;
    }

    public static Dto_Level? LoadLevel(string path)
    {
        if (File.Exists(path) != true)
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        Dto_Level? dto = JsonSerializer.Deserialize<Dto_Level>(bytes);
        return dto;
    }
}