using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Howl.Ecs;

public class ComponentRegistryNew : IDisposable
{
    /// <summary>
    ///     The max amounnt of entries that a component array can store in a component registry.
    /// </summary>
    public const int MaxComponentArrayLength = GenId.UniqueIndicesCount;

    /// <summary>
    ///     The minimum amount of entries that a component array can store in a component registry.
    /// </summary>
    public const int MinComponentArrayLength = 2; // 2 as there are Nils within the component arrays.

    /// <summary>
    ///     An any-map that stores strong references to component arrays of registered component types.
    /// </summary>
    /// <remarks>
    ///     Note: static casting is required to get the underlying component array.
    /// </remarks>
    public List<IComponentArray> Components;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed = false;

    /// <summary>
    ///     Gets the length of component arrays in <c>Components</c>.
    /// </summary>
    public int TotalComponentArrayLength;

    /// <summary>
    ///     Creates a new Component Registry instance.
    /// </summary>
    /// <remarks>
    ///     <c><paramref name="totalComponentArrayLength"/></c> will be clamped between the <c><see cref="MinComponentArrayLength"/></c> and <c><see cref="MaxComponentArrayLength"/></c>.
    /// </remarks>
    /// <param name="totalComponentArrayLength">the length that all for all component arrays stored and initialised by this registry.</param>
    public ComponentRegistryNew(int totalComponentArrayLength)
    {
        System.Diagnostics.Debug.Assert(totalComponentArrayLength >= MinComponentArrayLength && totalComponentArrayLength <= MaxComponentArrayLength, 
            "componentCount '{componentCount}' is not between minimum '{MinComponentCount}' and maximum value '{MaxComponentCount}'"
        );
        TotalComponentArrayLength = Howl.Math.Math.Clamp(totalComponentArrayLength, MinComponentArrayLength, MaxComponentArrayLength);

        Components = new();

        // add entries for each already intiailised component type id.
        // Note: this is needed for unit testing as registered 
        // component id's are carried over between test cases
        // because they are static classes.
        for(int i = 0; i < ComponentTypeId.Count; i++)
            Components.Add(null);
    }

    /// <summary>
    ///     Registers a component and creates a component array instance within a component registry.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="registry">the component registry instance to register the component in.</param>
    /// <returns>the id of the newly registered component.</returns>
    public static int RegisterComponent<T>(ComponentRegistryNew registry)
    {
        if(ComponentType<T>.IsInitialised == false)
        {
            // intialise the component type id if it hasnt been already.
            ComponentType<T>.Initialise();
            registry.Components.Add(null);
        }
        
        int id = ComponentType<T>.GetId();
        
        if(registry.Components[id] == null){
            
            // heap alocate a new collection of components
            // if one hasnt already been made.
            ComponentArray<T> components = new(registry.TotalComponentArrayLength);
            registry.Components[id] = components;
        }

        return id;
    }

    /// <summary>
    ///     Gets a component array of a specified type from a component registry.
    /// </summary>
    /// <typeparam name="T">the type of component to retrieve.</typeparam>
    /// <param name="registry">the component registry instance to get the component array from.</param>
    /// <returns>the component array of the specified type stored by the component registry.</returns>
    public static ComponentArray<T> GetComponents<T>(ComponentRegistryNew registry)
    {
        Span<IComponentArray> span = CollectionsMarshal.AsSpan(registry.Components);

        int id = ComponentType<T>.GetId();
        
        // intialise the array if it hasnt been already.
        // Note: this is needed for unit testing as registered 
        // component id's are carried over between test cases
        // because they are static classes.
        if(span[id] == null)
            span[id] = new ComponentArray<T>(registry.TotalComponentArrayLength);

        return span[id] as ComponentArray<T>;
    } 

    /// <summary>
    ///     Enforces a nil value on all component arrays in a component registry.
    /// </summary>
    /// <param name="registry"></param>
    public static void EnforceNil(ComponentRegistryNew registry)
    {
        Span<IComponentArray> span = CollectionsMarshal.AsSpan(registry.Components);
        for(int i = 0; i < span.Length; i++)
        {
            span[i].EnforceNil();
        }        
    }





    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(ComponentRegistryNew registry)
    {
        if(registry.Disposed)
        {
            return;
        }

        registry.Disposed = true;

        // disposal all components.
        for(int i = 0; i < registry.Components.Count; i++)
        {
            registry.Components[i].Dispose();    
        }
        registry.Components = null;

        registry.TotalComponentArrayLength = 0;

        GC.SuppressFinalize(registry);
    }

    ~ComponentRegistryNew()
    {
        Dispose(this);
    }
}