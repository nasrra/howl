using System.Collections.Generic;

namespace Howl.Collections;

public static class Nil
{
    public const int Index = 0;

    public static void Enforce<T>(this T[] array)
    {
#if DEBUG
        if (EqualityComparer<T>.Default.Equals(array[0], default) != true)
        {
            Debug.Log.MethodCall(Debug.LogType.Error, 2, "Nil value written to!");
        }
#endif
        array[0] = default;
    }
}