using System;
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
        
        private readonly SprigNode _pointer = new SprigNode(null, ulong.MaxValue);

        public SprigNode Head {
            get => (SprigNode) _pointer.Last;
            set => _pointer.Last = value;
        }

        public Sprig(ulong branchIndex, Sprig root, Diff diff)
        {
            Diff = diff;
            Head = new SprigNode(root.GetSprigNode(branchIndex - 1), branchIndex);
            
            //Use info from diff to instantiate positions for relevant objects
            foreach (var affectedObject in diff.AffectedObjects) {
                RootObject(affectedObject);
            }
        }
        
        public Sprig(Diff rootDiff)
        {
            Diff = rootDiff;
            Head = new SprigNode(null, 0);
                
        }

        public PositionVector ToPositionVector(DTFObject dtfObject) {
            if (!IsRooted(dtfObject))
                return Diff.Parent.Sprig.ToPositionVector(dtfObject);
            
            PositionNode currentPNode;
            var currentSNode = Head;
            var pHead = currentPNode = new PositionNode(null, currentSNode.Index, currentSNode.GetPosition(dtfObject));

            while (currentSNode.Last != null) {
                currentSNode = (SprigNode) currentSNode.Last;
                
                var newPNode = new PositionNode(null, currentSNode.Index, currentSNode.GetPosition(dtfObject));

                if (newPNode.SuperPosition.Equals(currentPNode.SuperPosition))
                    currentPNode.Index = newPNode.Index;
                
                else {
                    currentPNode.Last = newPNode;
                    currentPNode = newPNode;
                }

            }
            
            return new PositionVector(pHead);
        }
        
        public SprigNode GetSprigNode(ulong date) {
            return Head.GetSprigNode(date);
        }

        public void RootObject(DTFObject obj) {
            var initPosition = Position.Alloc(obj.GetType(), true);
            var currentBuilderNode = (SprigNode) _pointer.Last;

            while (currentBuilderNode != null && !currentBuilderNode.HasPosition(obj)) {
                currentBuilderNode.Add(obj, initPosition);

                currentBuilderNode = (SprigNode) currentBuilderNode.Last;
            }
        }

        public bool IsRooted(DTFObject dtfObject) {
            if (Head.Contains(dtfObject))
                return true;

            //Only other case is if parent is rooted but child hasn't been checked yet
            //Check if parent is rooted
            if (dtfObject.Parent != null & IsRooted(dtfObject.Parent)) {
                //Get timeline as it is in this universe
                var timeline = ToPositionVector(dtfObject.Parent);

                //Check to see if the child needs to be rooted as well
                if (!Manager.Owner.Compiler.CanConstrainLateralObject(dtfObject.Parent, dtfObject, dtfObject.ParentKey + "-BACK_REFERENCE-" + dtfObject.GetType().Name, timeline, this, out var result)) {
                    //It does
                    var lats = new HashSet<DTFObject> {dtfObject};
                    Diff.AddToDelta(dtfObject, result);
                    Diff.AddAffectedLateralObjects(dtfObject, result, lats);

                    return true;
                }
            }

            return false;
        }

        public IEnumerable<DTFObject> GetRootedObjects() {
            return Head.GetRootedObjects();
        }

        public bool Validate() {
            var current = Head;

            while (current != null) {
                foreach (var obj in current.GetRootedObjects()) {
                    if (current.GetPosition(obj).Uncertainty == -1)
                        return false;
                }

                current = (SprigNode) current.Last;
            }

            return true;
        }
        
        public void And(SprigVector other)
        {
            
            //Todo - Something wrong here
            var newHead = Node.And(Head, other.Head);
            
            if (!Head.Equals(newHead))
            {
                SetHeadAndRealign(0, newHead);
            }
            
            if(other.ActiveObjects > 0)
                Diff.Parent?.Sprig.And(other);
        }
        
        public bool And(PositionVector other, DTFObject dtfObject)
        {
            if (!IsRooted(dtfObject)) {
                return Diff.Parent.Sprig.And(other, dtfObject);
            }
            
            var sVector = new SprigVector();
            sVector.Add(dtfObject, other);
            
            var newHead = Node.And(Head, sVector.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
            {
                SetHeadAndRealign(0, newHead);
                return true;
            }

            return false;
        }

        private void SetHeadAndRealign(ulong realignFromDateOnward, SprigNode head)
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
                            var underneath = head.GetSprigNode(branchNode.Date);
                            var branch = branchNode.GetBranch(diff);
                            
                            if (branchNode.Date > 0) {
                                var branchSprig = branch.Next.Sprig;
                                var last = (SprigNode) (underneath.Index == branchNode.Date ? underneath.Last : underneath);

                                //Mask the other branch here to reflect the change
                                var maskVector = Manager.Owner.Compiler.GetTimelineVector(branchNode.Date - 1, Head);
                                var newBranchHead = Node.And(branchSprig.Head, maskVector.Head, branchNode.Date);

                                //Apply the mask only if we need to. If no changes occured, don't change anything
                                if(!newBranchHead.Equals(branchSprig.Head))
                                    branchSprig.SetHeadAndRealign(branchNode.Date + 1, newBranchHead);
                                
                                //Change the first node's previous to reflect the new structure

                                //If the first node on the branch has the same position as the node "underneath" it on the new head, just replace it
                                if (branch.FirstNodeOnBranch.IsSamePosition(underneath))
                                    branch.FirstNodeOnBranch = underneath;

                                //Else, create a "new" node to represent that position
                                else {
                                    var oldFirst = branch.FirstNodeOnBranch;
                                    branch.FirstNodeOnBranch = new SprigNode(last, branchNode.Date);
                                    branch.FirstNodeOnBranch.Add(oldFirst);
                                }
                            }
                        }
                    }
                }
            }
            
            Head = head;
        }
    }
}