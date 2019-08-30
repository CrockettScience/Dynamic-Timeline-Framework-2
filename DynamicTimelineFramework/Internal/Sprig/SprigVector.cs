using System;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigVector<T> : ISprig<T> where T : DTFObject {

        private SprigNode<T> _head;
        
        public IPosition<T> this[ulong index] {
            get {
                var current = _head;
                
                while (index < current.Index) {
                    current = current.Last;
                }

                return current.Position;
            }
        }

        public SprigVector(SprigNode<T> head) {
            _head = head;
        }

        public static SprigVector<T> operator &(SprigVector<T> left, SprigVector<T> right) {
            var currentLeft = left._head;
            var currentRight = right._head;
            
            var currentNew = new SprigNode<T>(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
            var newSprigVector = new SprigVector<T>(currentNew);

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

            return newSprigVector;
        }
        
        public void ShiftForward(ulong amount) {
            //Shift up all indices, and let the excess "fall off." Edge nodes are assumed to "stretch"
            var current = _head;
            
            while (ulong.MaxValue - current.Index < amount) {
                
                current = current.Last;
                _head = current;
            }

            while (current.Last != null) {
                
                current.Index += amount;
                current = current.Last;
            }
        }
        
        public void ShiftBackward(ulong amount) {
            //Shift down all indices, and let the excess "fall off". Edge nodes are assumed to "stretch"
            var current = _head;

            if (current.Index < amount)
                return;

            while (current.Last != null) {
                current.Index -= amount;

                if (current.Last.Index < amount) {
                    current.Last = null;
                    return;
                }

                current = current.Last;
            }
        }
    }
}