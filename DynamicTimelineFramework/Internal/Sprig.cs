using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (!Head.Contains(dtfObject))
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
            var currentBuilderNode = Head;

            while (currentBuilderNode != null && !currentBuilderNode.HasPosition(obj)) {
                currentBuilderNode.Add(obj, initPosition);

                currentBuilderNode = (SprigNode) currentBuilderNode.Last;
            }
        }
        
        public bool CheckRooted(DTFObject dtfObject) {
            if (Head.Contains(dtfObject))
                return true;

            var rootSprig = Diff.Parent.Sprig;
            
            //Navigate backwards to find the sprig it's actually rooted in
            while (!rootSprig.Head.Contains(dtfObject)) 
                rootSprig = rootSprig.Diff.Parent.Sprig;
            
            //Only other case is if parent is rooted but child hasn't been checked yet
            //Check if parent is rooted
            if (dtfObject.Parent != null && CheckRooted(dtfObject.Parent)) {
                //Get timeline as it is in this universe
                var timeline = ToPositionVector(dtfObject.Parent);

                //Check to see if the child needs to be rooted as well
                if (!Manager.Owner.Compiler.CanConstrainLateralObject(dtfObject.Parent, dtfObject, dtfObject.ParentKey + "-BACK_REFERENCE-" + dtfObject.GetType().Name, timeline, rootSprig, out var deltaTranslation)) {
                    //Root and establish
                    RootObject(dtfObject);
                    And(deltaTranslation, dtfObject);
                    var newTimeline = ToPositionVector(dtfObject);
                    
                    //Add fixed vector to delta
                    Diff.AddToDelta(dtfObject, newTimeline);

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
        
        public bool And(SprigVector other) {
            var forThis = new SprigVector();
            var forParent = new SprigVector();
            
            //Separate the vector based on what is rooted
            foreach (var obj in other.GetRootedObjects()) {
                if(CheckRooted(obj))
                    forThis.Add(obj, other.ToPositionVector(obj));
                
                else 
                    forParent.Add(obj, other.ToPositionVector(obj));
            }
            
            var andTasks = new Queue<Func<bool>>();
            andTasks.Enqueue(() => And(forThis, 0, andTasks));
            
            var changeOccured = andTasks.Dequeue().Invoke();
            while (andTasks.Count > 0)
                andTasks.Dequeue().Invoke();

            return !forParent.IsEmpty() && Diff.Parent.Sprig.And(forParent) || changeOccured;
        }

        public bool And(PositionVector other, DTFObject dtfObject)
        {
            if (!CheckRooted(dtfObject))
                return Diff.Parent.Sprig.And(other, dtfObject);
            
            var sVector = new SprigVector();
            sVector.Add(dtfObject, other);

            return And(sVector);
        }
        
        private bool And(SprigVector vector, ulong fromDateOnward, Queue<Func<bool>> taskQueue)
        {
            //First, use the diffChain to acquire the a branchNode chain
            var branchNodeChain = new LinkedList<SpineBranchNode>();
            var branchDiffChain = new LinkedList<Diff>();
            var diffChain = Diff.GetDiffChain();

            //Get Spine
            var spine = Manager.Spine;
            
            //Use the Diff Chain to navigate through the Spine tree and acquire the nodes
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

                    if (branchNode.Date >= fromDateOnward) {
                        branchNodeChain.AddFirst(branchNode);
                        branchDiffChain.AddFirst(nextDiff);
                    }
                }
            }

            var currentSprig = Head;
            var currentVector = vector.Head;
            var branchEnum = branchNodeChain.GetEnumerator();
            var branchDiffEnum = branchDiffChain.GetEnumerator();
            var pendingTasks = new Queue<Func<bool>>();

            SprigNode head;
            var currentNew = head = (SprigNode) Head.AndFactory(currentSprig, currentVector);
            currentNew.Index = currentNew.Index < fromDateOnward ? fromDateOnward : currentNew.Index;
            
            branchEnum.MoveNext();
            branchDiffEnum.MoveNext();
            var bNode = branchEnum.Current;

            while (currentNew.Index != fromDateOnward)
            {
                while (bNode != null && currentNew.Index <= bNode.Date) {
                
                    foreach (var diff in bNode) {
                        //We only care about branches that veer off from this sprig
                        if (diff != branchDiffEnum.Current) {
                            var branch = bNode.GetBranch(diff);
                            var capturedSprigNode = currentNew;
                            var capturedBranchNode = bNode;

                            //Add to the pending queue of branch tasks
                            pendingTasks.Enqueue(() => {
                                //Change the first node's previous to reflect the new structure

                                //If the first node on the branch has the same position as the node "underneath" it on the new head, just replace it
                                if (branch.FirstNodeOnBranch.IsSamePosition(capturedSprigNode))
                                    branch.FirstNodeOnBranch = capturedSprigNode;

                                //Else, create a "new" node to represent that position
                                else {
                                    var last = (SprigNode) (capturedSprigNode.Index == capturedBranchNode.Date ? capturedSprigNode.Last : capturedSprigNode);
                                    var oldFirst = branch.FirstNodeOnBranch;
                                    branch.FirstNodeOnBranch = new SprigNode(last, capturedBranchNode.Date);
                                    branch.FirstNodeOnBranch.Add(oldFirst);
                                }

                                var maskVector = Manager.Owner.Compiler.GetCertaintySignature(capturedBranchNode.Date - 1, Head);

                                return branch.Next.Sprig.And(maskVector, capturedBranchNode.Date + 1, taskQueue);
                            });
                        }
                    }

                    branchEnum.MoveNext();
                    branchDiffEnum.MoveNext();
                    bNode = branchEnum.Current;
                }
                
                var leftGreaterPlaceholder = currentSprig.Index;

                if (currentSprig.Index >= currentVector.Index) 
                    currentSprig = (SprigNode) currentSprig.Last;

                if (currentVector.Index >= leftGreaterPlaceholder)
                    currentVector = (SprigNode) currentVector.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = (SprigNode) Head.AndFactory(currentSprig, currentVector);
                newNode.Index = newNode.Index < fromDateOnward ? fromDateOnward : newNode.Index;

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.IsSamePosition(currentNew)) {
                    currentNew.Index = newNode.Index;
                }

                else
                {
                    currentNew.Last = newNode;
                    currentNew = newNode;
                }
            }

            currentNew.Last = currentNew.Index == currentSprig.Index ? currentSprig.Last : currentSprig;
            branchEnum.Dispose();
            branchDiffEnum.Dispose();

            var changeOccured = !Head.Equals(head);

            if (changeOccured) {
                Head = head;
                    
                //Enqueue the pending tasks
                while (pendingTasks.Count > 0) {
                    taskQueue.Enqueue(pendingTasks.Dequeue());
                }

            }
                
            return changeOccured;

        }
    }
}