using System;
using System.ComponentModel;
using Howl.Collections;
using Howl.Generic;

namespace Howl.ECS;

public class EcsState : IDisposable
{
    public ComponentRegistryNew Components;
    
    public EntityRegistry Entities;

    public bool Disposed;

    public EcsState(int maxEntities)
    {
        Entities = new(maxEntities);
        Components = new(maxEntities);
    }

    public static ComponentArray<T> GetComponents<T>(EcsState state)
    {
        return ComponentRegistryNew.GetComponents<T>(state.Components);
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);        
    }

    public static void Dispose(EcsState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        ComponentRegistryNew.Dispose(state.Components);
        state.Components = null;

        EntityRegistry.Dispose(state.Entities);
        state.Entities = null;

        GC.SuppressFinalize(state);
    }

    ~EcsState()
    {
        Dispose(this);
    }
}