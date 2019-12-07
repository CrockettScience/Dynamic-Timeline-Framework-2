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

                    diffEnum.MoveNext();
                    var currentDiff = diffEnum.Current;
                    
                    diffEnum.MoveNext();
                    var nextTurn = diffEnum.Current;

                    SpineNode currentNode = spine.RootBranch;

                    while (currentNode is SpineBranchNode branchNode) {
                        Diff nextDiff;

                        if (nextTurn != null && branchNode.Contains(nextTurn)) {
                            currentDiff = nextTurn;

                            diffEnum.MoveNext();
                            nextTurn = diffEnum.Current;

                            nextDiff = nextTurn;
                        }

                        else nextDiff = currentDiff;

                        //Realign all branches
                        foreach (var diff in branchNode) {
                            
                            //If it's the branch we're moving to next, we need to change it's bNode to the bNode on the new head
                            if (Equals(diff, nextDiff)) {
                                branchNode.GetBranch(diff).ParentSprigHead = GetBufferNode(branchNode.Date);
                            }

                            //If it's a different branch, we need to change the bNode's previous to reflect the new structure
                            else {
                                var bNode = branchNode.GetBranch(diff).ParentSprigHead;
                                bNode.Last = GetBufferNode(bNode.Index);
                                
                                //Todo - Mask the other branch here to reflect the change
                            }
                        }

                        currentNode = branchNode[nextDiff];
                    }
                }
            }
        }
        
        public Sprig(ulong index, Sprig root, Diff diff)
        {
            Diff = diff;
            
            if(root == null)
                _head = new BufferNode(null, index, new PositionBuffer());
            else 
                Head = new BufferNode(root.GetBufferNode(index - 1), index, new PositionBuffer());
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