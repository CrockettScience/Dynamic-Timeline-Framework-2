using System.Collections.Concurrent;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig
    {
        private BufferNode _head;
        
        public Diff Diff { get; }
        
        public SprigBuilder Builder { get; set; }
        
        public BufferNode Head
        {
            get => _head;

            set
            {
                _head = value;
                
                //Get Diff Chain
                var diffChain = Diff.GetDiffChain();
                
                //Get Spine
                var spine = Builder.Spine;
                
                //Use the Diff Chain to navigate through the Spine tree and realign the branch references
                using (var diffEnum = diffChain.GetEnumerator()) {
                    
                    //Todo - am I accounting for the edge case of 1 diff only?

                    diffEnum.MoveNext();
                    var currentDiff = diffEnum.Current;
                    
                    diffEnum.MoveNext();
                    var lookingForDiff = diffEnum.Current;

                    SpineNode currentNode = spine.Root;

                    while (currentNode is SpineBranchNode branchNode) {
                        Diff nextDiff;

                        if (branchNode.Contains(lookingForDiff)) {
                            currentDiff = lookingForDiff;

                            diffEnum.MoveNext();
                            lookingForDiff = diffEnum.Current;

                            nextDiff = lookingForDiff;
                        }

                        else nextDiff = currentDiff;

                        //Realign all branches
                        foreach (var diff in branchNode) {
                            
                            //If it's the branch we're moving to next, we need to change it's bNode to the bNode on the new head
                            if (Equals(diff, nextDiff)) {
                                branchNode.GetBranch(diff).BNode = GetBufferNode(branchNode.Date);
                            }

                            //If it's a different branch, we need to change the bNode's previous to reflect the new structure
                            else {
                                var bNode = branchNode.GetBranch(diff).BNode;
                                bNode.Last = GetBufferNode(bNode.Index);
                                
                                //Todo - Mask the other branch here to reflect the change
                                // - Could be challenging. Consider flagging branch for "lazy" maintenance?
                                // - Actually, lazy maintenance could be more efficient anyways
                            }
                        }

                        currentNode = branchNode[nextDiff];
                    }
                }
            }
        }
        
        public Sprig(ulong index, Sprig root, Diff diff)
        {
            Head = new BufferNode(root?.GetBufferNode(index - 1), index, new PositionBuffer());
            Diff = diff;
        }

        public void Alloc(int space, int startIndex)
        {
            var current = Head;
            var buffer = current.SuperPosition;
            
            //Don't allocate if the buffer is already allocated
            while (buffer != null && buffer.Length == startIndex)
            {
                buffer.Alloc(space);
                
                //Perform the same on the last
                current = (BufferNode) current.Last;
                buffer = current?.SuperPosition;
            }
        }

        public Position GetPosition(ulong date, DTFObject obj) {
            var bNode = GetBufferNode(date);

            var slice = new Slice(bNode.SuperPosition, obj.SprigBuilderSlice.LeftBound, obj.SprigBuilderSlice.RightBound);
            
            return new Position(obj.GetType(), slice);
        }
        
        private BufferNode GetBufferNode(ulong date)
        {
            var current = Head;

            while (current.Index > date)
            {
                current = (BufferNode) current.Last;
            }

            return current;
        }
        
        public void And<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.And(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = (BufferNode) newHead;
        }
        
        public void Or<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.Or(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = (BufferNode) newHead;
        }
    }
}