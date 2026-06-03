namespace Text;

public static class Char
{
    /// <summary>
    ///     Gets the amount of characters required to write a numerical value as a string.
    /// </summary>
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para></para>
    /// </remarks>
    public static int CalculateCount(int value)
    {
        int count = 0;
        while(value > 0)
        {
            value /= 10;
            count++;
        }
        return count;
    }
}