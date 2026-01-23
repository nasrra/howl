namespace Howl.ECS;

internal class ComponentStorage
{
    /// <summary>
    /// Gets the stored GenIndexList.
    /// </summary>
    public readonly IGenIndexList GenIndexList;
    
    /// <summary>
    /// Creates a new ComponentStorage instance.
    /// </summary>
    /// <param name="genIndexList">The gen index list of components to store.</param>
    public ComponentStorage(IGenIndexList genIndexList)
    {
        GenIndexList = genIndexList;
    }
}