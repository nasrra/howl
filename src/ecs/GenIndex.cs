namespace Howl.ECS;

public struct GenIndex
{
    public int index;
    public int generation;

    public GenIndex(int index, int generation)
    {
        this.index = index;
        this.generation = generation;
    }

    public override string ToString()
    {
        return $"[GenIndex]: index: {index} generation: {generation}";
    }
}