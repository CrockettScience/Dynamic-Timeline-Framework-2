using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class Sprig
    {
        public Position this[ulong date, DTFObject obj]
        {
            get
            {
                var sNode = GetSprigNode(date);
                
                return sNode.HasPosition(obj) ? sNode.GetPosition(obj) : Diff.Parent.Sprig[date, obj];
            }
        }
        
        public Diff Diff { get; }
        
        public SprigManager Manager { get; set; }
        
        public SprigNode Head { get; private set; }

        public Sprig(ulong branchIndex, Sprig root, Diff diff)
        {
            Diff = diff;
            
            if(root == null)
                Head = new SprigNode(null, branchIndex);
            //else
            //Todo - Use info from diff to instantiate positions for relevent objects
                //Head = new BufferNode(root.GetBufferNode(branchIndex - 1), branchIndex, new PositionBuffer(root.Manager.BitCount, true));
        }

        public PositionVector ToPositionVector(DTFObject dtfObject) {
            PositionNode currentPNode;
            var pHead = currentPNode = new PositionNode(null, Head.Index, Head.GetPosition(dtfObject));

            var currentSNode = Head.Last;
            
            while (currentSNode != null) {
                var newPNode = new PositionNode(null, currentSNode.Index, Head.GetPosition(dtfObject));

                if (newPNode.SuperPosition.Equals(currentPNode.SuperPosition))
                    currentPNode.Index = newPNode.Index;
                else
                    currentPNode = currentPNode.Last = newPNode; 
                
                currentSNode = currentSNode.Last;
            }
            
            return new PositionVector(pHead);
        }
        
        public SprigNode GetSprigNode(ulong date) {
            return Head.GetSprigNode(date);
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
        
        public bool And<T>(ISprigVector<T> other, params DTFObject[] objects) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.And(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
            {
                SetHeadAndRealign(0, (BufferNode) newHead, objects);
                return true;
            }

            return false;
        }

        private void SetHeadAndRealign(ulong realignFromDateOnward, BufferNode head, IReadOnlyCollection<DTFObject> objects)
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

                            //If it's a different branch, we need to change the first node's previous to reflect the new structure
                            if (branchNode.Date > 0) {
                                var branchSprig = branch.Next.Sprig;
                                var last = underneath.Index == branchNode.Date ? underneath.Last : underneath;
                                
                                //Mask the other branch here to reflect the change
                                var timelineVector = Manager.Owner.Compiler.GetTimelineSignatureForForwardConstraints(branchNode.Date - 1, this, objects);
                                var newBranchHead = (BufferNode) Node<PositionBuffer>.And(branchSprig.Head, timelineVector.Head, branchNode.Date);

                                if(!newBranchHead.Equals(branchSprig.Head))
                                    branchSprig.SetHeadAndRealign(branchNode.Date + 1, newBranchHead, objects);

                                //If the first node on the branch has the same value as the node "underneath" it on the new head, just replace it
                                if (branch.FirstNodeOnBranch.SuperPosition.Equals(underneath.SuperPosition))
                                    branch.FirstNodeOnBranch = underneath;

                                else
                                    branch.FirstNodeOnBranch = new BufferNode(last, branchNode.Date, branch.FirstNodeOnBranch.SuperPosition);
                            }
                        }
                    }
                }
            }
            
            Head = head;
        }
    }
}