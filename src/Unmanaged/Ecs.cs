using Howl.Unmanaged.Collections;

namespace Howl.Unmanaged.Ecs
{
    public struct GenIdAllocator
    {
        public const int MinLength = 2;
        public const int MaxLength = GenId.UniqueIndicesCount;

        public Array<GenId> GenIds;
        
        public Array<bool> Allocated;

        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>Contains a <c>Nil</c> element.</para>
        /// </remarks>
        public Collections.StackArray<int> FreeSlots;

        public int Length;

        public bool IsInitialised;

        public static bool Initialise(ref GenIdAllocator allocator, ref Memory.Arena arena, int length)
        {
            if (allocator.IsInitialised)
            {
                Debug.Panic("Already Initialised.");
                return false;
            }

            Debug.Assert(length <= MaxLength && length >= MinLength,
                $"length '{length}' is not between '{MinLength}' and '{MaxLength}'"
            );

            Array.Initialise(ref allocator.GenIds, ref arena, length);
            Array.Initialise(ref allocator.Allocated, ref arena, length);
            StackArray.Initialise(ref allocator.FreeSlots, ref arena, length);

            // set the indexes for the gen ids.
            for(int i = 1; i < length; i++)
            {
                allocator.GenIds[i] = new(i, 0);
            }

            StackArray.Push(ref allocator.FreeSlots, 1);
        
            allocator.IsInitialised = true;
            return true;
        }

        /// <summary>
        ///     Allocates an gen id from a allocator instance.
        /// </summary>
        public static GenIdResult Allocate(ref GenIdAllocator allocator, ref GenId genId)
        {
            if(allocator.FreeSlots.Count == 0)
            {
                Debug.LogError("Memory Limit Hit", stackDepth: 2);
                return GenIdResult.MemoryLimitHit;
            }
            
            // get the next available slot to allocate in.
            int slot = Collections.StackArray.Pop(ref allocator.FreeSlots);
            
            // check if its neighbour can be allocated as well.
            int nextSlot = slot + 1;
            if(nextSlot > 0 && nextSlot < allocator.GenIds.Length)
            {
                // add to the stack if it is also free.
                if (allocator.Allocated[nextSlot] == false)
                {
                    Collections.StackArray.Push(ref allocator.FreeSlots, nextSlot);            
                }
            }

            // update the gen index with the newly allocate data.
            allocator.Allocated[slot] = true;
            genId = allocator.GenIds[slot];

            return GenIdResult.Ok;
        }

        /// <summary>
        ///     Deallocates an gen id from a allocator instance.
        /// </summary>
        public static GenIdResult Deallocate(ref GenIdAllocator allocator, GenId genId)
        {
            int index = GenId.GetIndex(genId);

            // do nothing if the gen index is stale.
            if(allocator.GenIds[index] != genId)
            {
                return GenIdResult.StaleGenId;
            }

            DeallocateUnsafe(ref allocator, index);
            
            return GenIdResult.Ok;
        }

        /// <summary>
        ///     Deallocates an gen id from a allocator instance.
        /// </summary>
        /// <remarks>
        ///    <para>Remarks:</para>
        ///    <para>stale gen id checks are not enforced; the id will always run through the deallocation procedure.</para>
        /// </remarks>
        public static void DeallocateUnsafe(ref GenIdAllocator allocator, int entityIndex)
        {        
            // increment the generation so that any gen indices pointing to this data are invalidated (making them stale pointers).
            allocator.GenIds[entityIndex] = GenId.IncrementGeneration(allocator.GenIds[entityIndex]);

            // deallocate the entity.
            allocator.Allocated[entityIndex] = false;
            Collections.StackArray.Push(ref allocator.FreeSlots, entityIndex);
        }

        /// <summary>
        ///     Gets whether or not a gen id is stale within a allocator instance.
        /// </summary>
        /// <returns>true, if the gen id is stale; otherwise false</returns>
        public static bool IsGenIdStale(ref GenIdAllocator allocator, GenId genId)
        {
            return allocator.GenIds[GenId.GetIndex(genId)] != genId;
        }
    }
}
