using System;
using System.Collections.Concurrent;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig
    {
        public Position this[ulong date, DTFObject obj]
        {
            get
            {
                var bNode = GetBufferNode(date);

                var slice = new ReferenceSlice(bNode.SuperPosition, obj.SprigBuilderSlice.LeftBound, obj.SprigBuilderSlice.RightBound);
            
                return new Position(obj.GetType(), slice);
            }
        }
        
        public Diff Diff { get; }
        
        public SprigBuilder Builder { get; set; }
        
        public BufferNode Head { get; private set; }

        public Sprig(ulong branchIndex, Sprig root, Diff diff)
        {
            Diff = diff;
            
            if(root == null)
                Head = new BufferNode(null, branchIndex, new PositionBuffer());
            else
                Head = new BufferNode(root.GetBufferNode(branchIndex - 1), branchIndex,
                    new PositionBuffer(root.Builder.IndexedSpace, true));
        }

        public void Alloc(int space, int startIndex)
        {
            var current = Head;
            var buffer = current.SuperPosition;
            
            //Don't allocate if the buffer is already allocated
            while (buffer != null && buffer.Length == startIndex)
            {
                buffer.Alloc(space, true);
                
                //Perform the same on the last
                current = (BufferNode) current.Last;
                buffer = current?.SuperPosition;
            }
        }

        public SprigPositionVector ToPositionVector(DTFObject dtfObject)
        {
            return new SprigPositionVector(dtfObject.SprigBuilderSlice, dtfObject.GetType(), Head);
        }
        
        public SprigBufferVector ToBufferVector()
        {
            return new SprigBufferVector(Head);
        }
        
        public BufferNode GetBufferNode(ulong date)
        {
            var current = Head;

            while (current.Index > date)
            {
                current = (BufferNode) current.Last;
            }

            return current;
        }
        
        public bool And<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.And(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
            {
                SetHeadAndRealign(0, (BufferNode) newHead);
                return true;
            }

            return false;
        }
        
        public bool Or<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.Or(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
            {
                SetHeadAndRealign(0, (BufferNode) newHead);
                return false;
            }

            return true;
        }

        private void SetHeadAndRealign(ulong realignFromDateOnward, BufferNode head)
        {
            Head = head;
            
            //Get Diff Chain
            var diffChain = Diff.GetDiffChain();

            //Get Spine
            var spine = Builder.Spine;
            
            //Use the Diff Chain to navigate through the Spine tree and realign the branch references
            using (var diffEnum = diffChain.GetEnumerator()) {

                diffEnum.MoveNext();
                var nextDiff = diffEnum.Current;

                var nextTurn = diffEnum.MoveNext() ? diffEnum.Current : null;

                SpineNode currentNode = spine.RootBranch;
                while (currentNode is SpineBranchNode branchNode) {

                    if (nextTurn != null && branchNode.Contains(nextTurn)) {
                        nextDiff = nextTurn;

                        nextTurn = diffEnum.MoveNext() ? diffEnum.Current : null;
                    }

                    currentNode = branchNode[nextDiff];

                    //Realign all branches that come on or after realignFromDateOnward
                    if (branchNode.Date >= realignFromDateOnward) {
                        foreach (var diff in branchNode) {

                            //If it's the branch we're moving to next, we need to change the first node to the correct one on the new head
                            if (Equals(diff, nextDiff)) {
                                branchNode.GetBranch(diff).FirstNodeOnBranch = GetBufferNode(branchNode.Date);
                            }

                            //If it's a different branch, we need to change the first node's previous to reflect the new structure
                            else if (branchNode.Date > 0) {
                                var branch = branchNode.GetBranch(diff);

                                var firstNodeOnBranch = branch.FirstNodeOnBranch;
                                var previousNode = GetBufferNode(branchNode.Date - 1);

                                while (previousNode.Index >= firstNodeOnBranch.Index)
                                    previousNode = (BufferNode) previousNode.Last;

                                firstNodeOnBranch.Last = previousNode;

                                //Mask the other branch here to reflect the change
                                var timelineVector = Builder.Owner.Compiler.GetTimelineVector(branchNode.Date - 1, GetBufferNode(branchNode.Date - 1).SuperPosition);
                                var branchSprig = branch.Next.ParentSprig;
                                var newBranchHead = (BufferNode) Node<PositionBuffer>.And(branchSprig.Head, timelineVector.Head, branchNode.Date + 1);

                                if(!newBranchHead.Equals(branchSprig.Head))
                                    branchSprig.SetHeadAndRealign(branchNode.Date + 1, newBranchHead);
                            }
                        }
                    }
                }
            }
        }
    }
}