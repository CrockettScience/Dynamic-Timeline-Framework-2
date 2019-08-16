using System;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices.WindowsRuntime;

namespace MultiverseGraph.Core
{
    ///
    /// OLd implementation of the sprig data structure. For future reference.
    internal class ContinuityBranch
    {
        private ContinuityBranchNode _head;
        
        public Position this[ulong index]
        {
            get
            {
                var current = Head;
                
                //Iterate back until the node range covers the index
                while (current.StartIndex > index)
                {
                    current = current.Previous;
                }

                return current.Position;
            }
        }

        public ContinuityBranch()
        {
            Head = new ContinuityBranchNode(new Position(), 0);
        }
        
        public ContinuityBranch(Position position)
        {
            Head = new ContinuityBranchNode(position, 0);
        }

        private ContinuityBranch(ContinuityBranchNode head)
        {
            Head = head;
        }

        private ContinuityBranchNode Head
        {
            get => _head;

            set
            {
                //Unset old node as head
                if (_head != null) _head.HeadOwner = null;
                
                //Set new node as head
                value.HeadOwner = this;
                
                //Set value
                _head = value;
            }
        }

        private void Insert(ContinuityBranchNode node)
        {
            var current = Head;
            
            while (current.StartIndex >= node.StartIndex)
                current = current.Previous;

            node.Previous = current;
        }

        public void And(ContinuityBranch other)
        {
            //Make a new branch from scratch, as a separate instance
            var newContinuityBranch = this & other;

            //If no changes occured, no need to do anything
            if (newContinuityBranch.Equals(this)) return;
                
            //Iterate through each node and inform them
            var current = Head;
            
            //Save precious time and reduce complexity by simultaneously logging a map of ignored nodes.
            //These are nodes passed into the sever operation as nodes that do not need to be informed of the severance
            var ignoreNodes = new HashSet<ContinuityBranchNode>();

            while (current.Previous != null)
            {
                //TODO (DUMMY IN PLACE) - Get the propogated graft
                var propogatedGraft = new ContinuityBranch();

                ignoreNodes.Add(current);
                
                current.SeverOtherBranches(newContinuityBranch, ignoreNodes, propogatedGraft);

                current = current.Previous;
            }

            Head = newContinuityBranch.Head;
        }

        private static ContinuityBranch And(ContinuityBranch left, ContinuityBranch right, ulong lowerBound)
        {
            var currentLeft = left.Head;
            var currentRight = right.Head;
            
            var currentNew = new ContinuityBranchNode(currentLeft.Position & currentRight.Position, Math.Max(Math.Max(currentLeft.StartIndex, currentRight.StartIndex), lowerBound));
            var newBranch = new ContinuityBranch(currentNew);

            var leftGreaterPlaceholder = currentLeft.StartIndex;
            
            if (currentLeft.StartIndex >= currentRight.StartIndex)
                currentLeft = currentLeft.Previous;
            
            if (currentRight.StartIndex >= leftGreaterPlaceholder)
                currentRight = currentRight.Previous;

            
            //This might miss the first one?
            while (currentNew.StartIndex != lowerBound)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new ContinuityBranchNode(currentLeft.Position & currentRight.Position, Math.Max(Math.Max(currentLeft.StartIndex, currentRight.StartIndex), lowerBound));
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.Position == currentNew.Position)
                    currentNew.StartIndex = newNode.StartIndex;

                else
                    currentNew.Previous = newNode;
                
                leftGreaterPlaceholder = currentLeft.StartIndex;
            
                if (currentLeft.StartIndex >= currentRight.StartIndex)
                    currentLeft = currentLeft.Previous;
            
                if (currentRight.StartIndex >= leftGreaterPlaceholder)
                    currentRight = currentRight.Previous;
            }
            
            //Graft the lower bound to the rest of left
            if(currentNew.StartIndex != 0)
                left.Insert(currentNew);

            return newBranch;
        }

        public static ContinuityBranch operator &(ContinuityBranch left, ContinuityBranch right)
        {
            return And(left, right, 0);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is ContinuityBranch other)) return false;

            return Head.Equals(other.Head);

        }

        internal class ContinuityBranchNode
        {
            private ContinuityBranchNode _previous;

            internal bool IsHead => HeadOwner != null;

            internal ContinuityBranch HeadOwner { get; set; }
            internal Position Position { get; }
            internal ulong StartIndex { get; set; }

            internal ContinuityBranchNode Previous
            {
                get => _previous;
                
                set
                {
                    //Unsub from _previous severed event
                    if (_previous != null)
                    {
                        _previous.Severed -= OnSevered;
                        _previous.Graft -= OnGraft;
                    }

                    if (value == null) return;
                    
                    //Sub to the new event
                    value.Severed += OnSevered;
                    value.Graft += OnGraft;

                    _previous = value;
                }
            }

            private void OnGraft(object sender, GraftEventArgs e)
            {
                //Check the pager to see if a head has already been found
                if (e.Pager.HeadFound)
                    return;
                
                //Find the head
                if (!IsHead)
                {
                    //Continue the event based DFS
                    Graft?.Invoke(this, e);
                    return;

                }
                
                //Inform the pager to enable other events to stop
                e.Pager.HeadFound = true;
                
                //AND the relevant parts of the graft
                
                var newActiveBranch = And(HeadOwner, e.PropogatedGraft, e.GraftPoint);
                
                //If no changes occured, no need to do anything
                if (newActiveBranch.Equals(HeadOwner)) return;
                
                //Iterate through each node and inform them
                var current = this;
            
                //Save precious time and reduce complexity by simultaneously logging a map of ignored nodes.
                //These are nodes passed into the sever operation as nodes that do not need to be informed of the severance
                var ignoreNodes = new HashSet<ContinuityBranchNode>();

                while (current.StartIndex > e.GraftPoint)
                {
                    //TODO (DUMMY IN PLACE) - Get the propogated graft
                    var propogatedGraft = new ContinuityBranch();

                    ignoreNodes.Add(current);
                
                    current.SeverOtherBranches(newActiveBranch, ignoreNodes, propogatedGraft);

                    current = current.Previous;
                }

                HeadOwner.Head = newActiveBranch.Head;
            }
            
            private void OnSevered(object sender, SeveredEventArgs e)
            {
                //Waste of time, we only care about the other branches
                if (e.IgnoreNodes.Contains(this)) return;
                
                //"Seek" down the new structure to find it's new proper previous
                var current = e.NewActiveBranch.Head;

                //Side note: by the fact that this event is being executed, we guarantee StartIndex will never be 0
                //since this could only be executed by an event raised by a node with a lesser starting index.
                //So no need to check for null
                while (current.StartIndex >= StartIndex)
                    current = current.Previous;
                    
                //Set as the new previous
                Previous = current;
                
                //Apply the graft
                GraftOtherBranches(e.IgnoreNodes, e.PropagatedGraft, StartIndex);
                
            }

            private event EventHandler<SeveredEventArgs> Severed;
            private event EventHandler<GraftEventArgs> Graft; 

            internal ContinuityBranchNode(Position position, ulong startIndex)
            {
                Position = position;
                StartIndex = startIndex;
                
            }

            internal void SeverOtherBranches(ContinuityBranch activeBranch, HashSet<ContinuityBranchNode> ignoreNodes, ContinuityBranch propagatedGraft)
            {
                Severed?.Invoke(this, new SeveredEventArgs(activeBranch, ignoreNodes, propagatedGraft));
            }
            
            private void GraftOtherBranches(HashSet<ContinuityBranchNode> ignoreNodes, ContinuityBranch propogatedGraft, ulong graftPoint)
            {
                Graft?.Invoke(this, new GraftEventArgs(ignoreNodes, propogatedGraft, new HeadFoundPager(), graftPoint));
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ContinuityBranchNode other)) return false;

                return Position.Equals(other.Position) && 
                       StartIndex == other.StartIndex &&
                       _previous.Equals(other._previous);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (_previous != null ? _previous.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ Position.GetHashCode();
                    hashCode = (hashCode * 397) ^ StartIndex.GetHashCode();
                    return hashCode;
                }
            }

            private class SeveredEventArgs : EventArgs
            {
                public ContinuityBranch NewActiveBranch { get; }
                public HashSet<ContinuityBranchNode> IgnoreNodes { get; }
                public ContinuityBranch PropagatedGraft { get; }

                public SeveredEventArgs(ContinuityBranch newActiveBranch, HashSet<ContinuityBranchNode> ignoreNodes, ContinuityBranch propagatedGraft)
                {
                    NewActiveBranch = newActiveBranch;
                    IgnoreNodes = ignoreNodes;
                    PropagatedGraft = propagatedGraft;
                }
            }

            private class GraftEventArgs : EventArgs
            {
                public HashSet<ContinuityBranchNode> IgnoreNodes { get; }
                public ContinuityBranch PropogatedGraft { get; }

                public HeadFoundPager Pager { get; }
                
                public ulong GraftPoint { get; }

                public GraftEventArgs(HashSet<ContinuityBranchNode> ignoreNodes, ContinuityBranch propogatedGraft, HeadFoundPager pager, ulong graftPoint)
                {
                    IgnoreNodes = ignoreNodes;
                    PropogatedGraft = propogatedGraft;
                    Pager = pager;
                    GraftPoint = graftPoint;
                }
            }

            private class HeadFoundPager
            {
                public bool HeadFound { get; set; }
            }
            
        }
        
        
    }
}