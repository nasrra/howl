using Howl.ECS;
using Howl.Graphics.Text;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Test.Graphics.Text;

public class Text4096Test
{
    const string Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum varius magna ac urna euismod, nec dapibus purus volutpat. Sed vehicula magna non ligula malesuada, ac laoreet erat convallis. Nulla facilisi. Suspendisse potenti. Quisque sit amet turpis sed urna facilisis gravida. Integer dapibus eros vel risus facilisis, ut porttitor libero suscipit. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Fusce eget velit vitae neque ultrices accumsan. Etiam ut sapien a lectus ullamcorper commodo. Sed convallis erat vel purus dictum, vitae imperdiet est malesuada. In id tellus ac neque congue aliquet. Nam tempor orci nec massa cursus, id hendrerit erat suscipit. Sed et massa vel libero cursus convallis. Proin fermentum, nulla non laoreet tincidunt, nisl arcu scelerisque libero, vel posuere lacus dolor nec lorem. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Nulla facilisi. Praesent id massa quis magna cursus ultricies. Suspendisse potenti. Donec eget magna a velit pretium bibendum. Aliquam erat volutpat. Phasellus ac magna nec arcu tincidunt convallis. Donec vel sapien sit amet lectus blandit dictum. Nam vel nulla at nulla finibus pharetra in in nunc. Vivamus vitae odio nec arcu placerat ullamcorper. Curabitur in lorem eget libero dictum cursus. Quisque mattis, ex nec dictum pulvinar, enim est pretium lorem, at vulputate urna lorem vitae libero. Vestibulum at dolor vel justo vehicula tincidunt. Morbi in quam tincidunt, tincidunt nulla a, commodo sapien. Cras a lacus in justo sollicitudin rhoncus. Duis nec libero a arcu eleifend porttitor. Fusce dapibus sapien sed tellus scelerisque pretium. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Aenean sagittis ligula et sem commodo, sed rhoncus purus malesuada. Sed id sapien non tortor iaculis sollicitudin. Vestibulum vitae felis ac nisi efficitur tempus. Mauris imperdiet mauris vel dui dapibus fringilla. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin imperdiet felis sit amet mauris bibendum, nec porttitor nulla lobortis. Nam efficitur augue vel nulla rhoncus, sed fermentum velit tincidunt. Vestibulum ut leo a augue porta efficitur. Ut malesuada sem id mi fringilla, et iaculis purus tincidunt. Proin in arcu eu erat volutpat ultricies. Nam efficitur purus nec purus scelerisque, sed dictum libero imperdiet. In hac habitasse platea dictumst. Pellentesque a ligula ut odio tincidunt mollis. Donec quis mauris sit amet libero fermentum porta. Quisque auctor sapien nec lorem sodales tincidunt. Nullam non erat eu lacus placerat mollis. Curabitur tincidunt urna sed massa malesuada, nec tristique augue mattis. Phasellus a neque at sapien lacinia viverra. Donec sagittis purus et sapien tincidunt, at vehicula odio faucibus. Suspendisse potenti. Proin scelerisque risus ac magna tempus, nec pretium est fermentum. Etiam fringilla nunc ac erat tincidunt facilisis. Pellentesque vel massa ac augue gravida tincidunt. Quisque fermentum justo a ex porta, vitae rutrum nisi lacinia. Sed sagittis felis et urna tempor, sit amet volutpat mi convallis. Integer viverra metus ut sapien rhoncus, a venenatis lectus fermentum. Fusce sed mauris non risus sodales vehicula. Aliquam erat volutpat. Donec vitae elit ut nulla cursus feugiat. Sed id purus nec justo vulputate facilisis. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Nulla facilisi. Nam vel libero vel urna suscipit imperdiet. Quisque nec sapien in elit lacinia efficitur. Donec semper libero nec mauris feugiat, non vehicula dolor ultrices. Sed sagittis urna in turpis laoreet, ac placerat nunc ultrices. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Phasellus convallis eros at massa dapibus, ac efficitur mauris ultricies. In rhoncus purus sed nibh suscipit, non vulputate est lacinia. Mauris porttitor sem ut sem tempor, vitae laoreet nunc tristique. Nulla facilisi. Sed varius lacus at elit imperdie";

    [Fact]
    public unsafe void Constructor_Test()
    {
        Colour colour = Colour.White;
        Vector2 offset = new(1,2);
        GenIndex fontGenIndex = new(0,1);

        Text4096 text = new(
            new TextParameters(colour, offset, fontGenIndex, WorldSpace.World), 
            Text
        );

        // Verify text TextParameters.
        Assert.Equal(colour, text.TextParameters.Colour);
        Assert.Equal(offset, text.TextParameters.Offset);
        Assert.Equal(fontGenIndex, text.TextParameters.FontGenIndex);
        Assert.Equal(WorldSpace.World, text.TextParameters.WorldSpace);

        // Verify characters.
        Assert.Equal(4096, text.Length);
        string actual;
        actual = new string(text.Characters, 0, text.Length);
        Assert.Equal(Text,actual);
    }

    [Fact]
    public void SetCharacters_Test()
    {
        Text4096 text = new Text4096(
            new TextParameters(Colour.White, Vector2.Zero, new GenIndex(0,0), WorldSpace.World),
            ""
        );

        Span<char> characters = stackalloc char[Text4096.MaxLength];
        float num = 123456789.12f;
        num.TryFormat(characters, out int charsWritten, "0.00");

        // set the full span regardless of length.
        text.SetCharacters(characters);
        Assert.Equal(Text4096.MaxLength, text.Length);

        // set the span with a specefied length of the valid characters written to it.
        text.SetCharacters(characters, charsWritten);
        Assert.Equal(charsWritten, text.Length);
    }
}