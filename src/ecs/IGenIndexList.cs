using System;

namespace Howl.ECS;

internal interface IGenIndexList : IDisposable
{
    /// <summary>
    /// Gets whether or not this has been disposed.
    /// </summary>
    public bool IsDisposed{get;}

    /// <summary>
    /// resizes the sparse entry list.
    /// 
    /// Note: sparse entries can only grow, not shrink.
    /// A 'length' that is lower than the current length will not cause a resize;
    /// returning false.     
    /// </summary>
    /// <param name="count">The length to resize to.</param>
    /// <returns>true, when the operation successfully increased </returns>
    public bool ResizeSparseEntries(int count);    
}