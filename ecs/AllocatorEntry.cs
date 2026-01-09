namespace howl.ecs;

public struct AllocatorEntry
{
    public int generation;
    public bool isActive;

    public AllocatorEntry(int generation, bool isActive)
    {
        this.generation = generation;
        this.isActive = isActive;
    }

    public override string ToString()
    {
        return $"[AllocatorEntry]: \n generation: {generation} \n isActive: {isActive}";
    }
}