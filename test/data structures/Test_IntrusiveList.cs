using Howl.Collections;
using Howl.Text;

namespace Howl.Test.DataStructures;

public class Test_IntrusiveList
{

    public Test_IntrusiveList()
    {
        // Clear default listeners that show dialogs
        System.Diagnostics.Trace.Listeners.Clear();
        // Add a listener that throws an exception on failure
        System.Diagnostics.Trace.Listeners.Add(new ThrowingTraceListener());
    }

    [Fact]
    public void AddToTree_Fail_Test()
    {
        IntrusiveList.State state = new(12);
        
        // cannot add nil.
        Assert.ThrowsAny<Exception>(()=>IntrusiveList.AddToTree(state, 0));
        
        // cannot add nil.
        Assert.ThrowsAny<Exception>(()=>IntrusiveList.AddToTree(state, 0, 0));    
        
        // parent not in tree.        
        Assert.ThrowsAny<Exception>(()=>IntrusiveList.AddToTree(state, 0, 1));
    
    }

    [Fact]
    public void RemoveFromTree_Fail_Test(){

        IntrusiveList.State state = new(12);

        // not in tree.
        Assert.False(IntrusiveList.RemoveFromTree(state, 3));
    }

    [Fact]
    public void AddToTree_Success_Test()
    {
        IntrusiveList.State state = new(5);
        IntrusiveList.Node[] nodes = state.Nodes;
        SwapBackArray<int> rootIndices = state.RootIndices;
        
        int rootIndex = 1;
        int expectedRootIndex = 1;
        
        IntrusiveList.Node expectedParent;

        {   // root node test.
        
            IntrusiveList.AddToTree(state, 1);

            expectedParent = new(){
                Parent = 0, 
                FirstChild = 0, 
                NextSibling = rootIndex,
                PreviousSibling = rootIndex,
                RootDenseIndex = expectedRootIndex,
                InTree = true
            };

            IntrusiveList.AddToTree(state, rootIndex);

            Assert.Equal(expectedParent, nodes[rootIndex]);
            Assert.Equal(rootIndex, state.RootIndices[expectedRootIndex]);
            Assert.Equal(2, rootIndices.Count); // note account for the Nil (+1);
        }

        {   // add to root test.
            
            int firstChildIndex = 2;
            int siblingIndexA = 4;
            int siblingIndexB = 3;

            expectedParent.FirstChild = firstChildIndex;

            IntrusiveList.Node expectedFirstChild;
            IntrusiveList.Node expectedSiblingA;
            IntrusiveList.Node expectedSiblingB;

            {   // add first child.    

                expectedFirstChild = new()
                {
                    Parent = rootIndex,
                    FirstChild = 0,
                    NextSibling = firstChildIndex,
                    PreviousSibling = firstChildIndex,
                    RootDenseIndex = 0,
                    InTree = true
                };

                IntrusiveList.AddToTree(state, firstChildIndex, rootIndex);

                Assert.Equal(expectedParent, nodes[rootIndex]);
                Assert.Equal(expectedFirstChild, nodes[firstChildIndex]);
            }


            {   // add sibling A.
                
                expectedSiblingA = new()
                {
                    Parent = rootIndex,
                    FirstChild = 0,
                    NextSibling = firstChildIndex,
                    PreviousSibling = firstChildIndex,
                    RootDenseIndex = 0,
                    InTree = true
                };
                
                expectedFirstChild.NextSibling = siblingIndexA;
                expectedFirstChild.PreviousSibling = siblingIndexA;

                IntrusiveList.AddToTree(state, siblingIndexA, rootIndex);

                Assert.Equal(expectedParent, nodes[rootIndex]);
                Assert.Equal(expectedFirstChild, nodes[firstChildIndex]);
                Assert.Equal(expectedSiblingA, nodes[siblingIndexA]);
            }

            {   // add sibling B.
                
                expectedSiblingB = new()
                {   
                    Parent = rootIndex,
                    FirstChild = 0,
                    NextSibling = firstChildIndex,
                    PreviousSibling = siblingIndexA,
                    RootDenseIndex = 0,
                    InTree = true
                };

                expectedFirstChild.PreviousSibling = siblingIndexB;
                expectedSiblingA.NextSibling = siblingIndexB;

                IntrusiveList.AddToTree(state, siblingIndexB, rootIndex);

                Assert.Equal(expectedParent, nodes[rootIndex]);
                Assert.Equal(expectedFirstChild, nodes[firstChildIndex]);
                Assert.Equal(expectedSiblingA, nodes[siblingIndexA]);
                Assert.Equal(expectedSiblingB, nodes[siblingIndexB]);
            }
        }
    }

    [Fact]
    public void RemoveFromTree_Success_Test()
    {
        IntrusiveList.State state = new(20);
        IntrusiveList.Node[] nodes = state.Nodes;
        SwapBackArray<int> rootIndices = state.RootIndices;

        int firstLayerRoot = 1;

        int firstLayerFirstChild = 3;
        int firstLayerSiblingA = 4;
        int firstLayerSiblingB = 12;
        int firstLayerSiblingC = 13;

        int secondLayerFirstChild = 19;
        int secondLayerSiblingA = 8;
        int secondLayerSiblingB = 6;

        IntrusiveList.Node expectedFirstLayerRoot;
        IntrusiveList.Node expectedFirstLayerFirstChild;
        IntrusiveList.Node expectedFirstLayerSiblingA;
        IntrusiveList.Node expectedFirstLayerSiblingB; // note: this is the second layer root.
        IntrusiveList.Node expectedFirstLayerSiblingC;
        IntrusiveList.Node expectedSecondLayerFirstChild;
        IntrusiveList.Node expectedSecondLayerSiblingA;
        IntrusiveList.Node expectedSecondLayerSiblingB;
        
        {   // build tree.

            expectedFirstLayerRoot = new()
            {
                Parent = 0,
                FirstChild = firstLayerFirstChild,
                NextSibling = firstLayerRoot,
                PreviousSibling = firstLayerRoot,
                RootDenseIndex = 1,
                InTree = true,
            };

            expectedFirstLayerFirstChild = new()
            {
                Parent = firstLayerRoot,
                FirstChild = 0,
                NextSibling = firstLayerSiblingA,
                PreviousSibling = firstLayerSiblingC,
                RootDenseIndex = 0,
                InTree = true,
            };
            
            expectedFirstLayerSiblingA = new()
            {
                Parent = firstLayerRoot,
                FirstChild = 0,
                NextSibling = firstLayerSiblingB,
                PreviousSibling = firstLayerFirstChild,
                RootDenseIndex = 0,
                InTree = true,
            };

            // note: this is the second layer root.
            expectedFirstLayerSiblingB = new()
            {
                Parent = firstLayerRoot,
                FirstChild = secondLayerFirstChild,
                NextSibling = firstLayerSiblingC,
                PreviousSibling = firstLayerSiblingA,
                RootDenseIndex = 0,
                InTree = true,
            };

            expectedFirstLayerSiblingC = new()
            {
                Parent = firstLayerRoot,
                NextSibling = firstLayerFirstChild,
                PreviousSibling = firstLayerSiblingB,
                RootDenseIndex = 0,
                InTree = true,
            };

            expectedSecondLayerFirstChild = new()
            {
                Parent = firstLayerSiblingB,
                FirstChild = 0,
                NextSibling = secondLayerSiblingA,
                PreviousSibling = secondLayerSiblingB,
                RootDenseIndex = 0,
                InTree = true
            };

            expectedSecondLayerSiblingA = new()
            {
                Parent = firstLayerSiblingB,
                FirstChild = 0,
                NextSibling = secondLayerSiblingB,
                PreviousSibling = secondLayerFirstChild,
                RootDenseIndex = 0,
                InTree = true,  
            };

            expectedSecondLayerSiblingB = new()
            {
                Parent = firstLayerSiblingB,
                FirstChild = 0,
                NextSibling = secondLayerFirstChild,
                PreviousSibling = secondLayerSiblingA,
                RootDenseIndex = 0,
                InTree = true,
            };

            IntrusiveList.AddToTree(state, firstLayerRoot);
            IntrusiveList.AddToTree(state, firstLayerFirstChild, firstLayerRoot);
            IntrusiveList.AddToTree(state, firstLayerSiblingA, firstLayerRoot);
            IntrusiveList.AddToTree(state, firstLayerSiblingB, firstLayerRoot);
            IntrusiveList.AddToTree(state, firstLayerSiblingC, firstLayerRoot);
            IntrusiveList.AddToTree(state, secondLayerFirstChild, firstLayerSiblingB);
            IntrusiveList.AddToTree(state, secondLayerSiblingA, firstLayerSiblingB);
            IntrusiveList.AddToTree(state, secondLayerSiblingB, firstLayerSiblingB);

            Assert.Equal(expectedFirstLayerRoot, nodes[firstLayerRoot]);   
            Assert.Equal(expectedFirstLayerFirstChild, nodes[firstLayerFirstChild]);
            Assert.Equal(expectedFirstLayerSiblingA, nodes[firstLayerSiblingA]);
            Assert.Equal(expectedFirstLayerSiblingB, nodes[firstLayerSiblingB]);
            Assert.Equal(expectedFirstLayerSiblingC, nodes[firstLayerSiblingC]);
            Assert.Equal(expectedSecondLayerFirstChild, nodes[secondLayerFirstChild]);
            Assert.Equal(expectedSecondLayerSiblingA, nodes[secondLayerSiblingA]);
            Assert.Equal(expectedSecondLayerSiblingB, nodes[secondLayerSiblingB]);
        }

        {   // first layer sibling A removal.
            
            expectedFirstLayerFirstChild.NextSibling = firstLayerSiblingB;
            expectedFirstLayerSiblingB.PreviousSibling = firstLayerFirstChild;
            expectedFirstLayerSiblingA.InTree = false;

            IntrusiveList.RemoveFromTree(state, firstLayerSiblingA);

            Assert.Equal(expectedFirstLayerRoot, nodes[firstLayerRoot]);   
            Assert.Equal(expectedFirstLayerFirstChild, nodes[firstLayerFirstChild]);
            Assert.Equal(expectedFirstLayerSiblingA, nodes[firstLayerSiblingA]);
            Assert.Equal(expectedFirstLayerSiblingB, nodes[firstLayerSiblingB]);
            Assert.Equal(expectedFirstLayerSiblingC, nodes[firstLayerSiblingC]);
            Assert.Equal(expectedSecondLayerFirstChild, nodes[secondLayerFirstChild]);
            Assert.Equal(expectedSecondLayerSiblingA, nodes[secondLayerSiblingA]);
            Assert.Equal(expectedSecondLayerSiblingB, nodes[secondLayerSiblingB]);
        }

        {   // second layer sibling B removal.
            
            expectedSecondLayerSiblingA.NextSibling = secondLayerFirstChild;
            expectedSecondLayerSiblingA.PreviousSibling = secondLayerFirstChild;
            expectedSecondLayerFirstChild.NextSibling = secondLayerSiblingA; 
            expectedSecondLayerFirstChild.PreviousSibling = secondLayerSiblingA; 
            
            expectedSecondLayerSiblingB.InTree = false;

            IntrusiveList.RemoveFromTree(state, secondLayerSiblingB);

            Assert.Equal(expectedFirstLayerRoot, nodes[firstLayerRoot]);   
            Assert.Equal(expectedFirstLayerFirstChild, nodes[firstLayerFirstChild]);
            Assert.Equal(expectedFirstLayerSiblingA, nodes[firstLayerSiblingA]);
            Assert.Equal(expectedFirstLayerSiblingB, nodes[firstLayerSiblingB]);
            Assert.Equal(expectedFirstLayerSiblingC, nodes[firstLayerSiblingC]);
            Assert.Equal(expectedSecondLayerFirstChild, nodes[secondLayerFirstChild]);
            Assert.Equal(expectedSecondLayerSiblingA, nodes[secondLayerSiblingA]);
            Assert.Equal(expectedSecondLayerSiblingB, nodes[secondLayerSiblingB]);
        }

        {   // parent removal - nodes should become parented to their parent's parent.
            
            expectedSecondLayerFirstChild.Parent = firstLayerRoot;       
            expectedSecondLayerFirstChild.PreviousSibling = firstLayerSiblingC;

            expectedSecondLayerSiblingA.Parent = firstLayerRoot;
            expectedSecondLayerSiblingA.NextSibling = firstLayerFirstChild;

            expectedFirstLayerFirstChild.PreviousSibling = secondLayerSiblingA;
            expectedFirstLayerFirstChild.NextSibling = firstLayerSiblingC;
            
            expectedFirstLayerSiblingC.PreviousSibling = firstLayerFirstChild;
            expectedFirstLayerSiblingC.NextSibling = secondLayerFirstChild;

            expectedFirstLayerSiblingB.InTree = false;

            IntrusiveList.RemoveFromTree(state, firstLayerSiblingB);
        
            Assert.Equal(expectedFirstLayerRoot, nodes[firstLayerRoot]);   
            Assert.Equal(expectedFirstLayerFirstChild, nodes[firstLayerFirstChild]);
            Assert.Equal(expectedFirstLayerSiblingA, nodes[firstLayerSiblingA]);
            Assert.Equal(expectedFirstLayerSiblingB, nodes[firstLayerSiblingB]);
            Assert.Equal(expectedFirstLayerSiblingC, nodes[firstLayerSiblingC]);
            Assert.Equal(expectedSecondLayerFirstChild, nodes[secondLayerFirstChild]);
            Assert.Equal(expectedSecondLayerSiblingA, nodes[secondLayerSiblingA]);
            Assert.Equal(expectedSecondLayerSiblingB, nodes[secondLayerSiblingB]);
        }

        {   // root removal - nodes should become roots if their parent is not a child of another node.
            expectedFirstLayerRoot.RootDenseIndex = 0;
            expectedFirstLayerRoot.InTree = false;

            int expectedFirstLayerFirstChildRootIndex = 1;
            int expectedFirstLayerSiblingCRootIndex = 2;
            int expectedSecondLayerFirstChildRootIndex = 3;
            int expectedSecondLayerSiblingARootIndex = 4;

            expectedFirstLayerFirstChild.Parent = 0;
            expectedFirstLayerFirstChild.NextSibling = firstLayerFirstChild;
            expectedFirstLayerFirstChild.PreviousSibling = firstLayerFirstChild;
            expectedFirstLayerFirstChild.RootDenseIndex = expectedFirstLayerFirstChildRootIndex; 
            
            expectedFirstLayerSiblingC.Parent = 0;
            expectedFirstLayerSiblingC.NextSibling = firstLayerSiblingC;
            expectedFirstLayerSiblingC.PreviousSibling = firstLayerSiblingC;
            expectedFirstLayerSiblingC.RootDenseIndex = expectedFirstLayerSiblingCRootIndex;
            
            expectedSecondLayerFirstChild.Parent = 0;
            expectedSecondLayerFirstChild.NextSibling = secondLayerFirstChild;
            expectedSecondLayerFirstChild.PreviousSibling = secondLayerFirstChild;
            expectedSecondLayerFirstChild.RootDenseIndex = expectedSecondLayerFirstChildRootIndex;

            expectedSecondLayerSiblingA.Parent = 0;
            expectedSecondLayerSiblingA.NextSibling = secondLayerSiblingA;
            expectedSecondLayerSiblingA.PreviousSibling = secondLayerSiblingA;
            expectedSecondLayerSiblingA.RootDenseIndex = expectedSecondLayerSiblingARootIndex;

            IntrusiveList.RemoveFromTree(state, firstLayerRoot);

            Assert.Equal(firstLayerFirstChild, rootIndices[expectedFirstLayerFirstChildRootIndex]);
            Assert.Equal(firstLayerSiblingC, rootIndices[expectedFirstLayerSiblingCRootIndex]);
            Assert.Equal(secondLayerFirstChild, rootIndices[expectedSecondLayerFirstChildRootIndex]);
            Assert.Equal(secondLayerSiblingA, rootIndices[expectedSecondLayerSiblingARootIndex]);
            Assert.Equal(5, rootIndices.Count); // note account for the Nil (+1);

            Assert.Equal(expectedFirstLayerRoot, nodes[firstLayerRoot]);   
            Assert.Equal(expectedFirstLayerFirstChild, nodes[firstLayerFirstChild]);
            Assert.Equal(expectedFirstLayerSiblingA, nodes[firstLayerSiblingA]);
            Assert.Equal(expectedFirstLayerSiblingB, nodes[firstLayerSiblingB]);
            Assert.Equal(expectedFirstLayerSiblingC, nodes[firstLayerSiblingC]);
            Assert.Equal(expectedSecondLayerFirstChild, nodes[secondLayerFirstChild]);
            Assert.Equal(expectedSecondLayerSiblingA, nodes[secondLayerSiblingA]);
            Assert.Equal(expectedSecondLayerSiblingB, nodes[secondLayerSiblingB]);
        }
    }
}