using System;
using System.Collections.Generic;

namespace Howl.Collections;

[Serializable]
public sealed class SwapbackList<T> : List<T>{
    
    /// <summary>
    /// Removes the element at the specified index after swapping it with the last element.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
    public new void RemoveAt(int index){
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

        if (Count <= 1){
            // If the list has 0 or 1 element, simply remove the element.
            base.RemoveAt(index);
            return;
        }

        int lastIndex = Count - 1;

        // Swap the element at the specified index with the last element.
        T temp = this[index];
        this[index] = this[lastIndex];
        this[lastIndex] = temp;

        // Remove the last element (which is now the one originally at `index`).
        base.RemoveAt(lastIndex);
    }

    /// <summary>
    /// Removes the first occurrence of the specified element after swapping it with the last element.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public new bool Remove(T item){
        int index = IndexOf(item);
        if (index == -1)
            return false;
        RemoveAt(index);
        return true;
    }
}
