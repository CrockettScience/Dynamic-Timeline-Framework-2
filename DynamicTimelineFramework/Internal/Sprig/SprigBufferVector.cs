using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class SprigBufferVector : ISprigVector<PositionBuffer>
    {
        #region STATIC

        public static SprigBufferVector operator &(SprigBufferVector left, SprigBufferVector right) {
            return new SprigBufferVector(Node<PositionBuffer>.And(left.Head, right.Head));
        }
        
        public static SprigBufferVector operator |(SprigBufferVector left, SprigBufferVector right) {
            return new SprigBufferVector(Node<PositionBuffer>.Or(left.Head, right.Head));
        }
        
        #endregion

        public Node<PositionBuffer>.INode Head { get; private set; }
        
        public SprigBufferVector(int size, List<SprigPositionVector> vectors, bool emptySpaceValue)
        {
            //Empty space value will be the default bit state for values not accounted for by the given vectors.
            //Set to FALSE if you want to OR the vector with a sprig buffer while preserving the state of the values,
            //Set to TRUE if you want to AND the vector with a sprig buffer while preserving the state of the values
             
            var bufferBase = (Node<PositionBuffer>.INode) new BufferNode(null, 0, new PositionBuffer(size, emptySpaceValue));

            foreach (var vector in vectors)
            {
                bufferBase = Node<PositionBuffer>.And(bufferBase, vector.Head);
            }

            Head = bufferBase;

        }
        
        public SprigBufferVector(Node<PositionBuffer>.INode head)
        {

            Head = head;

        }

        public ISprigVector<PositionBuffer> Copy()
        {
            return new SprigBufferVector(Head.Copy());
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