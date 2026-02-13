using System;
using System.Collections.Generic;

namespace Howl.ECS;

internal interface IGenIndexList : IDisposable
{
    /// <summary>
    /// Gets whether or not this has been disposed.
    /// </summary>
    public bool IsDisposed{get;}

    public List<SparseEntry> Sparse {get; set;}
}