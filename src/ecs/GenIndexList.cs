using System;
using System.Collections.Generic;
using Howl.Collections;

namespace Howl.ECS;

public sealed class GenIndexList<T> : IGenIndexList
{
    public List<SparseEntry> Sparse {get; set;}
    
    public SwapbackList<DenseEntry<T>> Dense;

    private bool disposed;
    public bool IsDisposed => disposed;

    public GenIndexList(){
        Sparse = new();
        Dense = new();
    }
    
    
    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Sparse.Clear();
            Sparse = null;
            Dense.Clear();
            Dense = null;
        }

        disposed = true;
    }

    ~GenIndexList(){
        Dispose(false);
    }
}