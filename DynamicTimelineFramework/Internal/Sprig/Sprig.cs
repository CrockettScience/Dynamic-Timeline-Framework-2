using System;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig<T> : ISprig<T> where T : DTFObject {
        
        public Spine<T> Spine { get; }
        
        public SHeadNode<T> SpineHead { get; }

        public IPosition<T> this[ulong index] {
            get {
                var current = SpineHead.SprigHead;
                
                while (index < current.Index) {
                    current = current.Last;
                }

                return current.Position;
            }
        }
        
        #region CONSTRUCTORS
        
        public Sprig(Diff rootDiff)
        {
            Spine = new Spine<T>(this, new Position<T>(), rootDiff);
            SpineHead = (SHeadNode<T>) Spine.Root;
        }
        
        public Sprig(Position<T> defaultPosition, Diff rootDiff)
        {
            Spine = new Spine<T>(this, defaultPosition, rootDiff);
            SpineHead = (SHeadNode<T>) Spine.Root;
        }

        public Sprig(Sprig<T> branchFrom, Diff diff, Position<T> defaultPosition) {
            Spine = branchFrom.Spine;
            
            //Find the SprigNode to branch off of
            var current = branchFrom.SpineHead.SprigHead;

            while (current.Index > diff.Date) {
                current = current.Last;
            }
            
            var sprigHead = new SprigNode<T>(current, defaultPosition, diff.Date);
            SpineHead = new SHeadNode<T>(this, sprigHead, diff);
            
            //Splice the new SpineHead into the Spine
            branchFrom.SpineHead.AddBranch(SpineHead, diff.Date);
        }
        
        #endregion



        public void And(SprigVector<T> vector) {
            var currentLeft = SpineHead.SprigHead;
            var currentRight = vector.Head;
            
            var currentNew = new SprigNode<T>(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode<T>(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
                
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
            
            
            //Todo - we need to take the new sprig and replace the current sprig, but preserve both the spine and the branching sprig nodes
        }
    }
}