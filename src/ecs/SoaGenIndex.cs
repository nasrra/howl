namespace Howl.ECS;

public class Soa_GenIndex
{
    /// <summary>
    /// Gets and sets the indices.
    /// </summary>
    public int[] Indices;

    /// <summary>
    /// Gets and sets the generations.
    /// </summary>
    public int[] Generations;

    /// <summary>
    /// Creates a new Structure-Of-Array's GenIndex instance.
    /// </summary>
    /// <param name="capacity">the capacity of the underling array's.</param>
    public Soa_GenIndex(int capacity)
    {
        Indices = new int[capacity];
        Generations = new int[capacity];
    }
}