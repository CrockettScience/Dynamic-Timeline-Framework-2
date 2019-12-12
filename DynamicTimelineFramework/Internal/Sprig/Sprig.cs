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

                var slice = new ReferenceSlice(bNode.SuperPosition, obj.SprigManagerSlice.LeftBound, obj.SprigManagerSlice.RightBound);
            
                return new Position(obj.GetType(), slice);
            }
        }
        
        public Diff Diff { get; }
        
        public SprigManager Manager { get; set; }
        
        public BufferNode Head { get; private set; }

        public Sprig(ulong branchIndex, Sprig root, Diff diff)
        {
            Diff = diff;
            
            if(root == null)
                Head = new BufferNode(null, branchIndex, new PositionBuffer());
            else
                Head = new BufferNode(root.GetBufferNode(branchIndex - 1), branchIndex,
                    new PositionBuffer(root.Manager.BitCount, true));
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

        public PositionVector ToPositionVector(DTFObject dtfObject)
        {
            return new PositionVector(dtfObject.SprigManagerSlice, dtfObject.GetType(), Head);
        }
        
        public BufferVector ToBufferVector()
        {
            return new BufferVector(Head);
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

        public bool Validate()
        {
            foreach (var dtfObject in Manager.Registry)
            {
                if (!((PositionNode) ToPositionVector(dtfObject).Head).Validate())
                    return false;
            }

            return true;
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
            
            //Get Diff Chain
            var diffChain = Diff.GetDiffChain();

            //Get Spine
            var spine = Manager.Spine;
            
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
                        foreach (var diff in branchNode)
                        {

                            var underneath = head.GetBufferNode(branchNode.Date);
                            var branch = branchNode.GetBranch(diff);

                            //If it's the branch we're moving to next, we need to change the first node to the correct one on the new head
                            if (Equals(diff, nextDiff)) {
                                branchNode.GetBranch(diff).First = underneath;
                            }

                            //If it's a different branch, we need to change the first node's previous to reflect the new structure
                            else if (branchNode.Date > 0) {
                                var branchSprig = branch.Next.Sprig;
                                var last = underneath.Index == branchNode.Date ? underneath.Last : underneath;
                                
                                //If the first node on the branch has the same value as the node "underneath" it on the new head, just replace it
                                if (branch.FirstNodeOnBranch.SuperPosition.Equals(underneath.SuperPosition))
                                    branch.FirstNodeOnBranch = underneath;

                                else
                                    branch.FirstNodeOnBranch = new BufferNode(last, branchNode.Date, branch.FirstNodeOnBranch.SuperPosition);
                                
                                //Mask the other branch here to reflect the change
                                var timelineVector = Manager.Owner.Compiler.GetTimelineVector(branchNode.Date - 1, last.SuperPosition);
                                var newBranchHead = (BufferNode) Node<PositionBuffer>.And(branchSprig.Head, timelineVector.Head, branchNode.Date + 1);

                                if(!newBranchHead.Equals(branchSprig.Head))
                                    branchSprig.SetHeadAndRealign(branchNode.Date + 1, newBranchHead);
                            }
                        }
                    }
                }
            }
            
            Head = head;
        }
    }
}