namespace Howl.Generic;

public ref struct Ref<T>
{
    bool valid = false;

    /// <summary>
    /// Gets whether or not the data is valid.
    /// </summary>

    public bool Valid => valid;

    ref T value;

    /// <summary>
    /// Gets a reference to the data stored in this struct.
    /// </summary>

    public ref T Value => ref value;

    public Ref(ref T value, bool valid)
    {
        this.value = ref value;
        this.valid = valid;
    }
}