using Howl.Io;

namespace Howl.Test.Io;

public class Test_PathUtils
{
    [Fact]
    public void FlattenPath_Test()
    {
        string directoryPath = "assets/levels/test-levels/";
        string relativePath = "../../textures/image.png";
        string expectedPath = "assets/textures/image.png";
        string result = PathUtils.FlattenPath(directoryPath, relativePath);
        Assert.Equal(expectedPath, result);
    }
}