using System;

public class EntityRegistry : IDisposable
{
    public const int MaxEntityCount = GenId.UniqueIndicesCount;
    public const int MinEntityCount = 2;

    /// <summary>
    /// The GenIds of all entities.
    /// </summary>
    public GenId[] GenIds;

    /// <summary>
    ///     A the available slots in this collection that can be allocated to.
    /// </summary>
    public StackArray<int> FreeSlots;

    /// <summary>
    ///     Whether or not an entity has been allocated/is in use or free/deallocated. 
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             Index 0 is reserved as a <c>Nil</c> sentinel and should not be used for data.
    ///         </item>
    ///         <item>
    ///             This is a parallel array associated with <c>Data</c>, <c>Flags</c>, and <c>Generations</c> by index.
    ///         </item>
    ///     </list>
    /// </remarks>
    public bool[] Allocated;

    /// <summary>
    ///     Whether or not this instanc is disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new EntityRegistry instance.
    /// </summary>
    /// <remarks>
    ///     <c><paramref name="maxEntityCount"/></c> will be clamped between the <c><see cref="MinEntityCount"/></c> and <c><see cref="MaxEntityCount"/></c>
    /// </remarks>
    /// <param name="maxEntityCount">the total amount of entities this registry can store.</param>
    public EntityRegistry(int maxEntityCount)
    {
        System.Diagnostics.Debug.Assert(maxEntityCount >= MinEntityCount && maxEntityCount <= MaxEntityCount, 
            "maxEntityCount '{maxEntityCount}' is not between minimum '{MinEntityCount}' and maximum value '{MaxEntityCount}'"
        );
        maxEntityCount = Howl.Math.Math.Clamp(maxEntityCount, MinEntityCount, MaxEntityCount);
        GenIds = new GenId[maxEntityCount];

        // set the indexes for the gen ids.
        for(int i = 1; i < maxEntityCount; i++)
        {
            GenIds[i] = new(i, 0);
        }
 
        Allocated = new bool[maxEntityCount];

        FreeSlots = new(maxEntityCount);

        // append entry 1 as the next free slot available; not zero as zero is Nil.
        StackArray.Push(FreeSlots, 1);
    }

    /// <summary>
    ///     Allocates a new entity in a entity registry instance.
    /// </summary>
    /// <param name="registry">the registry instance to allocate a new entity into.</param>
    /// <param name="genId">the gen id of the newly allocated entity.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.MemoryLimitHit"/>
    ///         </item>
    ///     </list>
    /// </returns>
    public static GenIdResult Allocate(EntityRegistry registry, ref GenId genId)
    {
        if(registry.FreeSlots.Count == 0)
        {
            return GenIdResult.MemoryLimitHit;
        }
        
        // get the next available slot to allocate in.
        int slot = StackArray.Pop(registry.FreeSlots);

        
        // check if its neighbour can be allocated as well.
        int nextSlot = slot + 1;
        if(nextSlot > 0 && nextSlot < registry.GenIds.Length)
        {
            // add to the stack if it is also free.
            if (registry.Allocated[nextSlot] == false)
            {
                StackArray.Push(registry.FreeSlots, nextSlot);            
            }
        }

        // update the gen index with the newly allocate data.
        registry.Allocated[slot] = true;
        genId = registry.GenIds[slot];

        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Deallocates an entity from a entity registry instance.
    /// </summary>
    /// <param name="registry">the registry instance to deallocate from.</param>
    /// <param name="genId">the gen id of the entity to deallocate.</param>
    /// <returns>
    ///     <list type="bullet">
    ///         <item> 
    ///             <see cref="GenIdResult.Ok"/>
    ///         </item>
    ///         <item>
    ///             <see cref="GenIdResult.StaleGenId"/>
    ///         </item>
    ///     </list>
    /// </returns>    
    public static GenIdResult Deallocate(EntityRegistry registry, GenId genId)
    {
        int index = GenId.GetIndex(genId);

        // do nothing if the gen index is stale.
        if(registry.GenIds[index] != genId)
        {
            return GenIdResult.StaleGenId;
        }

        // increment the generation so that any gen indices pointing to this data are invalidated (making them stale pointers).
        registry.GenIds[index] = GenId.IncrementGeneration(registry.GenIds[index]);

        // deallocate the entity.
        registry.Allocated[index] = false;
        StackArray.Push(registry.FreeSlots, index);

        return GenIdResult.Ok;
    }

    /// <summary>
    ///     Gets whether or not a gen id is stale within a entity registry instance.
    /// </summary>
    /// <param name="registry">the entity registry instance to query.</param>
    /// <param name="genId">the specified gen id.</param>
    /// <returns>true, if the gen id is stale; otherwise false</returns>
    public static bool IsGenIdStale(EntityRegistry registry, GenId genId)
    {
        return registry.GenIds[GenId.GetIndex(genId)] != genId;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(EntityRegistry registry)
    {
        if (registry.Disposed)
        {
            return;
        }

        registry.Disposed = true;

        registry.GenIds = null;

        StackArray.Dispose(registry.FreeSlots);
        registry.FreeSlots = null;

        registry.Allocated = null;

        GC.SuppressFinalize(registry);
    }

    ~EntityRegistry()
    {
        Dispose(this);
    }
}