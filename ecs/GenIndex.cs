namespace howl.ecs;

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
        return $"[GenIndex]:\nindex: {index}\ngeneration: {generation}";
    }
}