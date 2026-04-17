namespace Howl.Generic;

public readonly ref struct ReadOnlyRef<T>
{
    readonly bool valid = false;

    /// <summary>
    /// Gets whether or not the data is valid.
    /// </summary>

    public readonly bool Valid => valid;

    readonly ref T value;

    /// <summary>
    /// Gets a reference to the data stored in this struct.
    /// </summary>

    public ref T Value => ref value;

    public ReadOnlyRef(ref T value, bool valid)
    {
        this.value = ref value;
        this.valid = valid;
    }
}