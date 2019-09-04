using System;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig {
        
        public Spine Spine { get; }
        
        public SHeadNode SpineHead { get; }

        public Position this[ulong index] {
            get {
                var current = SpineHead.SprigHead;
                
                while (index < current.Index) {
                    current = current.Last;
                }

                return current.Position;
            }
        }
        
        #region CONSTRUCTORS
        
        public Sprig(Type type, Diff rootDiff)
        {
            Spine = new Spine(this, Position.Alloc(type), rootDiff);
            SpineHead = (SHeadNode) Spine.Root;
        }
        
        public Sprig(Position defaultPosition, Diff rootDiff)
        {
            Spine = new Spine(this, defaultPosition, rootDiff);
            SpineHead = (SHeadNode) Spine.Root;
        }

        public Sprig(Sprig branchFrom, Diff diff, Position defaultPosition) {
            Spine = branchFrom.Spine;
            
            //Find the SprigNode to branch off of
            var current = branchFrom.SpineHead.SprigHead;

            while (current.Index > diff.Date) {
                current = current.Last;
            }
            
            var sprigHead = new SprigNode(current, defaultPosition, diff.Date);
            SpineHead = new SHeadNode(this, sprigHead, diff);
            
            //Splice the new SpineHead into the Spine
            branchFrom.SpineHead.AddBranch(SpineHead, diff.Date);
        }
        
        #endregion

        public bool And(SprigVector vector) {
            var currentLeft = SpineHead.SprigHead;
            var currentRight = vector.Head;
            
            var headNew = new SprigNode(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
            var currentNew = headNew;
            
            if(headNew.Position.Uncertainty == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
                
                if(newNode.Position.Uncertainty == -1)
                    throw new ParadoxException();
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.Position.Equals(currentNew.Position))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
            }

            if (headNew.Equals(SpineHead.SprigHead))
            {
                //No need to replace the structure, we can return false
                return false;
            }
            
            //Todo - We need to take the new sprig and replace the current sprig, but preserve both the spine and the branching sprig nodes
            //Todo - We need to figure out a function that can recompute a sprig vector representation of the reduction in certainty on other branched paths
            return true;
        }

        public SprigVector ToVector()
        {
            var current = SpineHead.SprigHead;
            var currentVec = new SprigNode(null, current.Position, current.Index);
            var vector = new SprigVector(current);

            while (current.Last != null)
            {
                current = current.Last;
                        
                currentVec.Last = new SprigNode(null, current.Position, current.Index);

                currentVec = currentVec.Last;
            }

            return vector;
        }
    }
}