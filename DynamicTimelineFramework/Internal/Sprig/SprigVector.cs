using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigVector{
        public SprigNode Head { get; private set; }

        public Position this[ulong index] {
            get {
                var current = Head;
                
                while (index < current.Index) {
                    current = current.Last;
                }

                return current.Position;
            }
        }

        public SprigVector(SprigNode head) {
            Head = head;
        }
        
        public SprigVector(Type type) {
            Head = new SprigNode(null, Position.Alloc(type), 0);
        }

        public SprigVector Copy() {
            var copyHead = new SprigNode(null, Head.Position, Head.Index);
            
            var current = Head.Last;
            var copyNext = copyHead;

            while (current != null) {
                copyNext.Last = new SprigNode(null, current.Position, copyNext.Index);

                current = current.Last;
                copyNext = copyNext.Last;
            }
            
            return new SprigVector(copyHead);
            
        }

        public static SprigVector operator &(SprigVector left, SprigVector right) {
            var currentLeft = left.Head;
            var currentRight = right.Head;
            
            var currentNew = new SprigNode(null, currentLeft.Position & currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
            var newSprigVector = new SprigVector(currentNew);
            
            if(currentNew.Position.Uncertainty == -1)
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

            return newSprigVector;
        }
        
        public static SprigVector operator |(SprigVector left, SprigVector right) {
            var currentLeft = left.Head;
            var currentRight = right.Head;
            
            var currentNew = new SprigNode(null, currentLeft.Position | currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
            var newSprigVector = new SprigVector(currentNew);
            
            if(currentNew.Position.Uncertainty == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode(null, currentLeft.Position | currentRight.Position, Math.Max(currentLeft.Index, currentRight.Index));
                
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

            return newSprigVector;
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

        public void Crumple(ulong index, ulong amount) {
            //Crumple the nodes from index back by the amount, removing the nodes in between

            var crumpleZoneStart = index - amount;
            var current = Head;

            while (current != null) {
                if (current.Index < index) {
                    if (current.Index >= crumpleZoneStart) {
                        
                        //Remove the node altogether
                        current.Last = current.Last.Last;
                    }
                    
                    else if(current.Index > 0){
                        current.Index += amount;
                    }
                }

                current = current.Last;
            }
        }
    }
}