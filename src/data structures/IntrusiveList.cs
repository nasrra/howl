using System;
using System.Runtime.CompilerServices;
using Howl.Collections;

namespace Howl.DataStructures;

public static class IntrusiveList
{
    public const int MinLength = 1;
    public const int MaxLength = int.MaxValue;

    public struct Node
    {
        /// <summary>
        ///     the <c>nodeIndex</c> of this node's parent.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>when <c>0</c> is stored, this points to the <c>Nil</c> element and is invalid.</para>
        /// </remarks>
        public int Parent;

        /// <summary>
        ///     the <c>nodeIndex</c> of this node's next sibling.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>this value is self-recursive, meaning the next sibling loops back to this node's index.</para>
        /// </remarks>
        public int NextSibling;

        /// <summary>
        ///     the <c>nodeIndex</c> of this node's previous sibling.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>this value is self-recursive, meaning the previous sibling loops back to this node's index.</para>
        /// </remarks>
        public int PreviousSibling;

        /// <summary>
        ///     the <c>nodeIndex</c> of this node's first child.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>when <c>0</c> is stored, this points to the <c>Nil</c> element and is invalid.</para>
        /// </remarks>
        public int FirstChild;

        /// <summary>
        ///     the <c>nodeIndex</c> of this node's index in <c><see cref="State.RootIndices"/></c>.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>contains a <c>Nil</c> element</para>.
        ///    <para>when <c>0</c> is stored, this points to the <c>Nil</c> element and is invalid.</para>
        /// </remarks>
        public int RootDenseIndex;

        /// <summary>
        ///     whether or not this node is within the state's tree.
        /// </summary>
        public bool InTree;

        public override string ToString()
        {
            return $"""
            {nameof(Node)} 
                Parent: {Parent},
                NextSibling: {NextSibling},
                PreviousSibling: {PreviousSibling},
                FirstChild: {FirstChild},
                RootDenseIndex: {RootDenseIndex},
                InTree: {InTree}
            """;
        }
    }

    public class State
    {
        public Node[] Nodes;

        /// <remarks>
        ///     Remarks: contains a <c>Nil</c> element.
        /// </remarks>
        public SwapBackArray<int> RootIndices;

        public bool Disposed;

        public State(int length)
        {
            System.Diagnostics.Debug.Assert(length >= MinLength, 
                $"IntrusiveListState must have a length greater than '{length}'."
            );

            length = Math.Math.Clamp(length, MinLength, MaxLength);

            Nodes = new Node[length];
            RootIndices = new(length);

            // append the Nil.
            RootIndices.Append(0);
        }

        ~State()
        {
            Dispose(this);
        }
    }
    
    /// <summary>
    ///     Adds a root node to the tree.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool AddToTree(State state, int nodeIndex)
    {
        // node cannot be the Nil.
        if(nodeIndex == 0)
        {
            System.Diagnostics.Debug.Assert(false, "node: '{nodeIndex}' cannot be the Nil element.");
            return false;
        }

        Node[] nodes = state.Nodes;
        ref Node node = ref nodes[nodeIndex];
        SwapBackArray<int> roots = state.RootIndices;

        if (node.InTree)
        {
            return false;
        }

        // the node is a root.
        node.RootDenseIndex = SwapBackArray.Append(roots, nodeIndex);

        // node has no other siblings.
        node.NextSibling = nodeIndex;
        node.PreviousSibling = nodeIndex;

        node.InTree = true;
        return true;
    }

    /// <summary>
    ///     Adds a node to the tree.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>if <c><paramref name="parentIndex"/></c> is zero, this will become a root node.</para>
    /// </remarks>
    /// <returns>
    ///     true, if successfully added to the tree; otherwise false if already added.
    /// </returns>
    public static bool AddToTree(State state, int nodeIndex, int parentIndex)
    {
        // node cannot be the Nil.
        if(nodeIndex == 0)
        {
            System.Diagnostics.Debug.Assert(false, "node: '{nodeIndex}' cannot be the Nil element.");
            return false;
        }

        // add as a root if parent index is zero.
        if(parentIndex == 0)
        {
            AddToTree(state, nodeIndex);
        }

        Node[] nodes = state.Nodes;
        ref Node node = ref nodes[nodeIndex];

        if (node.InTree)
        {
            return false;
        }
        
        ref Node parent = ref nodes[parentIndex];
        if (parent.InTree == false)
        {
            System.Diagnostics.Debug.Assert(false, " parent: '{parentIndex}' is not in the tree!");
            return false;
        }

        node.Parent = parentIndex;
        if(parent.FirstChild == 0) // only set if it is pointing to the Nil.
        {
            parent.FirstChild = nodeIndex;
            
            // node has no other siblings (as it is the first child).
            node.NextSibling = nodeIndex;
            node.PreviousSibling = nodeIndex;
        }
        else
        {
            // get the last child.
            int lastChildIndex = nodes[parent.FirstChild].PreviousSibling;
            ref Node lastChild = ref nodes[lastChildIndex];
            
            // get the first child.
            int firstChildIndex = parent.FirstChild;
            ref Node firstChild = ref nodes[firstChildIndex];

            // connect last child to the new node.
            lastChild.NextSibling = nodeIndex;
            node.PreviousSibling = lastChildIndex;

            node.NextSibling = firstChildIndex;
            firstChild.PreviousSibling = nodeIndex;
        }

        node.InTree = true;
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>
    ///     true, if successfully removed from the tree; otherwise false if already removed.
    /// </returns>
    public static bool RemoveFromTree(State state, int nodeIndex)
    {
        // node cannot be the Nil.
        if(nodeIndex == 0)
        {
            System.Diagnostics.Debug.Assert(false, "{nodeIndex} cannot be the Nil element.");
            return false;
        }

        Node[] nodes = state.Nodes;
        SwapBackArray<int> roots = state.RootIndices;
        ref Node node = ref nodes[nodeIndex];

        if (node.InTree == false)
        {
            return false;
        }
        
        int parentIndex = node.Parent;
        int firstChildIndex = node.FirstChild;

        // deallocate from parent.
        if(parentIndex != 0)
        {
            ref Node parent = ref nodes[node.Parent];
            
            // if this node doesnt have any children;
            if(node.FirstChild == 0)
            {
                // nil the parents child.
                if(parent.FirstChild == nodeIndex)
                {
                    parent.FirstChild = 0;
                }
            }
            else
            {
                if(parent.FirstChild == nodeIndex)
                {
                    // move the children to the parent.
                    parent.FirstChild = node.FirstChild;

                    // deallocate from children by setting their parent to this node's parent.
                    ref Node child = ref nodes[node.FirstChild];
                    while (true)
                    {
                        child.Parent = parentIndex;
                        
                        int nextSiblingIndex = child.NextSibling;
                        
                        if(nextSiblingIndex == firstChildIndex)
                        {
                            break;
                        }

                        child = ref nodes[nextSiblingIndex];
                    }
                }
                else
                {
                    // append this node's children to it's parents children.

                    int parentFirstChildIndex = parent.FirstChild;
                    ref Node parentFirstChild = ref nodes[parentFirstChildIndex];
                    
                    int parentLastChildIndex = parentFirstChild.PreviousSibling;
                    ref Node parentLastChild = ref nodes[parentLastChildIndex];
                    
                    parentLastChild.NextSibling = node.FirstChild;

                    int currentSiblingIndex = node.FirstChild;
                    ref Node child = ref nodes[currentSiblingIndex];
                    child.PreviousSibling = parentLastChildIndex;
                    
                    while (true)
                    {
                        child.Parent = parentIndex;
                        
                        int nextSiblingIndex = child.NextSibling;
                        
                        if(nextSiblingIndex == firstChildIndex)
                        {
                            child.NextSibling = parentFirstChildIndex;
                            parentFirstChild.PreviousSibling = currentSiblingIndex;
                            break;
                        }

                        currentSiblingIndex = nextSiblingIndex;
                        child = ref nodes[nextSiblingIndex];
                    }

                    // don't perform sibling deallocation at the end of this function.
                    // as the re-ordering of siblings in the parent has aready done this.
                    // goto End;
                }
            }
        }
        else
        {
            // remove the node from the roots array.
            // performing the dense index swap as well.
            ref Node lastRoot = ref nodes[roots[roots.Count-1]];
            lastRoot.RootDenseIndex = node.RootDenseIndex;
            SwapBackArray.RemoveAt(roots, node.RootDenseIndex);
            node.RootDenseIndex = 0;

            if (firstChildIndex != 0)
            {
                // deallocate from children by making them root nodes in the tree.
                int currentSiblingIndex = firstChildIndex;
                ref Node child = ref nodes[currentSiblingIndex]; 
                while (true)
                {
                    
                    child.Parent = 0;

                    // add children to root stack array.
                    child.RootDenseIndex = SwapBackArray.Append(roots, currentSiblingIndex);

                    // children are now roots, so they should no longer be associated with thier siblings.
                    int nextSiblingIndex = child.NextSibling;
                    child.NextSibling = currentSiblingIndex;
                    child.PreviousSibling = currentSiblingIndex;

                    if(nextSiblingIndex == firstChildIndex)
                    {
                        break;
                    }
                    
                    currentSiblingIndex = nextSiblingIndex;

                    // go to the next sibling of the child.
                    child = ref nodes[currentSiblingIndex];
                }

                // no need to deallocate from siblings, as this has already done that.
                goto End;
            }
        }


        // deallocate from siblings.
        ref Node nextSibling = ref nodes[node.NextSibling];
        nextSibling.PreviousSibling = node.PreviousSibling;

        ref Node previousSibling = ref nodes[node.PreviousSibling];
        previousSibling.NextSibling = node.NextSibling;

        End:
        node.InTree = false;
        return true;
    }

    public static void Dispose(State state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        state.Nodes = null;
        state.RootIndices = null;

        GC.SuppressFinalize(state);
    }
}