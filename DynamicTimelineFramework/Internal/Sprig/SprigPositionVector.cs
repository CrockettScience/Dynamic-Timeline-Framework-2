using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigPositionVector : ISprigVector<Position>
    {
        
        #region STATIC

        public static SprigPositionVector operator &(SprigPositionVector left, SprigPositionVector right) {
            return new SprigPositionVector(left.Slice, Node<Position>.And(left.Head, right.Head));
        }
        
        public static SprigPositionVector operator |(SprigPositionVector left, SprigPositionVector right) {
            return new SprigPositionVector(left.Slice, Node<Position>.Or(left.Head, right.Head));
        }
        
        #endregion
        
        public Slice Slice { get; }
        public Node<Position>.INode Head { get; private set; }

        public SprigPositionVector(Slice slice, Node<Position>.INode head)
        {
            Slice = slice;
            Head = head;

        }
        
        public ISprigVector<Position> Copy() {
            return new SprigPositionVector(Slice, Head.Copy());
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
    }
}