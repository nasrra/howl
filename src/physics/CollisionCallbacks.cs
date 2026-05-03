using Howl.Collections;

namespace Howl.Physics;

public class CollisionCallbacks<T> where T : allows ref struct
{
    /// <summary>
    ///     The <c>OnEnter</c> callbacks for a physics body.
    /// </summary>
    public StackArray<CollisionCallback<T>>[] OnEnterCallbacks;

    /// <summary>
    ///     The <c>OnSustain</c> callbacks for a physics body.
    /// </summary>
    public StackArray<CollisionCallback<T>>[] OnSustainCallbacks;

    /// <summary>
    ///     The <c>OnExitCallbacks</c> for a physics body.
    /// </summary>
    public StackArray<CollisionCallback<T>>[] OnExitCallbacks;

    /// <summary>
    ///     Creates a new collision callback
    /// </summary>
    /// <param name="maxPhysicsBodyCount">the maximum amount of physics bodies.</param>
    /// <param name="maxCallbacks">the maximum amount of callbacks that a physics body can have.</param>
    public CollisionCallbacks(int maxPhysicsBodyCount, int maxCallbacks)
    {
        OnEnterCallbacks = new StackArray<CollisionCallback<T>>[maxPhysicsBodyCount];
        for(int i = 0; i < maxPhysicsBodyCount; i++)
        {
            OnEnterCallbacks[i] = new(maxCallbacks);
        }

        OnExitCallbacks = new StackArray<CollisionCallback<T>>[maxPhysicsBodyCount];
        for(int i = 0; i < maxPhysicsBodyCount; i++)
        {
            OnExitCallbacks[i] = new(maxCallbacks);
        }

        OnSustainCallbacks = new StackArray<CollisionCallback<T>>[maxPhysicsBodyCount];
        for(int i = 0; i < maxPhysicsBodyCount; i++)
        {
            OnSustainCallbacks[i] = new(maxCallbacks);
        }
    }
}


public static class CollisionCallbacks
{
    /// <summary>
    ///     Pushes a callback onto the <c>OnEnter</c> callback stack at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection to push into.</param>
    /// <param name="callback">the call back to push.</param>
    /// <param name="index">the index of the callback stack to push onto.</param>
    public static void PushOnEnterCallback<T>(this CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
    {
        StackArray.Push(callbacks.OnEnterCallbacks[index], callback);
    }

    /// <summary>
    ///     Clears a stack of <c>OnEnter</c> callbacks at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
    /// <param name="index">the index of the stack to clear.</param>
    public static void ClearOnEnterCallbacks<T>(this CollisionCallbacks<T> collisionCallbacks, int index)
    {
        StackArray.ClearCount(collisionCallbacks.OnEnterCallbacks[index]);
    }

    /// <summary>
    ///     Pushes a callback onto the <c>OnSustain</c> callback stack at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection to push into.</param>
    /// <param name="callback">the call back to push.</param>
    /// <param name="index">the index of the callback stack to push onto.</param>
    public static void PushOnSustainCallback<T>(this CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
    {
        StackArray.Push(callbacks.OnSustainCallbacks[index], callback);
    }

    /// <summary>
    ///     Clears a stack of <c>OnSustain</c> callbacks at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
    /// <param name="index">the index of the stack to clear.</param>
    public static void ClearOnSustainCallbacks<T>(this CollisionCallbacks<T> callbacks, int index)
    {
        StackArray.ClearCount(callbacks.OnSustainCallbacks[index]);
    }

    /// <summary>
    ///     Pushes a callback onto the <c>OnExit</c> callback stack at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection to push into.</param>
    /// <param name="callback">the call back to push.</param>
    /// <param name="index">the index of the callback stack to push onto.</param>
    public static void PushOnExitCallback<T>(this CollisionCallbacks<T> callbacks, CollisionCallback<T> callback, int index)
    {
        StackArray.Push(callbacks.OnExitCallbacks[index], callback);
    }

    /// <summary>
    ///     Clears a stack of <c>OnExit</c> callbacks at a given index.
    /// </summary>
    /// <param name="callbacks">the callback collection that contains the stack to clear.</param>
    /// <param name="index">the index of the stack to clear.</param>
    public static void ClearOnExitCallbacks<T>(this CollisionCallbacks<T> callbacks, int index)
    {
        StackArray.ClearCount(callbacks.OnExitCallbacks[index]);
    }
}