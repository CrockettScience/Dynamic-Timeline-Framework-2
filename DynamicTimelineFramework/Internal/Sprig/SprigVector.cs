
using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigVector{
        
        #region STATIC

        public static SprigVector operator &(SprigVector left, SprigVector right) {
            var currentLeft = left.Head;
            var currentRight = right.Head;
            
            var currentNew = new SprigNode<Position>(null, Math.Max(currentLeft.Index, currentRight.Index), currentLeft.SuperPosition & currentRight.SuperPosition);
            var newSprigVector = new SprigVector(left.Slice, currentNew);
            
            if(currentNew.SuperPosition.Uncertainty == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode<Position>(null, Math.Max(currentLeft.Index, currentRight.Index), currentLeft.SuperPosition & currentRight.SuperPosition);
                
                if(newNode.SuperPosition.Uncertainty == -1)
                    throw new ParadoxException();
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
            }

            return newSprigVector;
        }
        
        public static SprigVector operator |(SprigVector left, SprigVector right) {
            var currentLeft = left.Head;
            var currentRight = right.Head;
            
            var currentNew = new SprigNode<Position>(null, Math.Max(currentLeft.Index, currentRight.Index), currentLeft.SuperPosition | currentRight.SuperPosition);
            var newSprigVector = new SprigVector(left.Slice, currentNew);
            
            if(currentNew.SuperPosition.Uncertainty == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //OR the positions and set the index to the greater of the nodes
                var newNode = new SprigNode<Position>(null, Math.Max(currentLeft.Index, currentRight.Index), currentLeft.SuperPosition | currentRight.SuperPosition);
                
                if(newNode.SuperPosition.Uncertainty == -1)
                    throw new ParadoxException();
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
            }

            return newSprigVector;
        }
        
        #endregion
        
        public Slice Slice { get; }
        public SprigNode<Position> Head { get; set; }
        
        public SprigVector(DTFObject obj)
        {
            Slice = obj.HeadlessSlice;
            Head = new SprigNode<Position>(null, 0, Position.Alloc(obj.GetType()));
            
        }

        public SprigVector(Slice slice, SprigNode<Position> head)
        {
            Slice = slice;
            Head = head;

        }
        
        public SprigVector Copy() {
            return new SprigVector(Slice, Head.Copy());
        }
        
        public void ShiftForward(ulong amount) {
            //Shift up all indices, and let the excess "fall off." Edge nodes are assumed to "stretch"
            var current = Head;
            
            while (ulong.MaxValue - current.Index < amount) {
                
                current = current.Last;
                Head = current;
            }

            while (current.Last != null) {
                
                current.Index += amount;
                current = current.Last;
            }
        }
        
        public void ShiftBackward(ulong amount) {
            //Shift down all indices, and let the excess "fall off". Edge nodes are assumed to "stretch"
            var current = Head;

            while (current.Last != null) {
                current.Index = current.Index <= amount ? 0 : current.Index - amount;

                if (current.Index == 0) {
                    current.Last = null;
                    return;
                }

                current = current.Last;
            }
        }
        
#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            if (!(obj is SprigVector other)) return false;

            return Head.Equals(other.Head);
        }
    }
}