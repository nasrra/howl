using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Howl.ECS;

public class SystemRegistry : IDisposable
{
    /// <summary>
    /// The registered update systems within this registry.
    /// </summary>
    private List<UpdateSystem> updateSystems;
    
    /// <summary>
    /// The registered fixed update systems within this registry.
    /// </summary>
    private List<FixedUpdateSystem> fixedUpdateSystems;
    
    /// <summary>
    /// The registered draw systems within this registry.
    /// </summary>
    private List<DrawSystem> drawSystems;

    private bool disposed = false;
    public bool IsDisposed => disposed;

    /// <summary>
    /// Creates a new SystemRegistry instance.
    /// </summary>
    public SystemRegistry()
    {
        updateSystems = new();
        fixedUpdateSystems = new();
        drawSystems = new();
    }

    /// <summary>
    /// Registers an update system to take place in the update loop.
    /// </summary>
    /// <param name="updateSystem">The update system to register.</param>
    public void RegisterUpdateSystem(UpdateSystem updateSystem)
    {
        updateSystems.Add(updateSystem);
    }

    /// <summary>
    /// Registers a fixed-update system to take place in the fixed-update loop.
    /// </summary>
    /// <param name="fixedUpdateSystem"></param>
    public void RegisterFixedUpdateSystem(FixedUpdateSystem fixedUpdateSystem)
    {
        fixedUpdateSystems.Add(fixedUpdateSystem);
    }

    /// <summary>
    /// Registers a draw system to take place in the draw loop.
    /// </summary>
    /// <param name="drawSystem"></param>
    public void RegisterDrawSystem(DrawSystem drawSystem)
    {
        drawSystems.Add(drawSystem);
    }

    /// <summary>
    /// Calls all registered update system delegates.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        Span<UpdateSystem> span = CollectionsMarshal.AsSpan(updateSystems);
        for(int i = 0; i < span.Length; i++)
        {
            span[i](deltaTime);
        }
    }

    /// <summary>
    /// Calls all registered fixed-update system delegates.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void FixedUpdate(float deltaTime)
    {
        Span<FixedUpdateSystem> span = CollectionsMarshal.AsSpan(fixedUpdateSystems);
        for(int i = 0; i < span.Length; i++)
        {
            span[i](deltaTime);
        }
    }

    /// <summary>
    /// Calls all registered draw system delegates.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Draw(float deltaTime)
    {
        Span<DrawSystem> span = CollectionsMarshal.AsSpan(drawSystems);
        for(int i = 0; i < span.Length; i++)
        {
            span[i](deltaTime);
        }        
    }


    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            updateSystems = null;
            fixedUpdateSystems = null;
            drawSystems = null;
        }

        disposed = true;
    }

    ~SystemRegistry()
    {
        Dispose(false);
    }
}