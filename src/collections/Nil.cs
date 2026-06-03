
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
            Debug.LogError("Nil value written to!", stackDepth: 2);
        }
#endif
        array[0] = default;
    }

    public static void Enforce<T>(this T[] array, int stride)
    {

        for(int i = 0; i < stride; i++)
        {            
#if DEBUG
            if (EqualityComparer<T>.Default.Equals(array[i], default) != true)
            {
                Debug.LogError("Nil value written to!", stackDepth: 2);
            }
#endif
            array[i] = default;
        }
    }
}