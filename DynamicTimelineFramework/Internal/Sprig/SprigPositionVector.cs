using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigPositionVector : ISprigVector<Position>, IOperativeSliceProvider
    {
        
        #region STATIC

        public static SprigPositionVector operator &(SprigPositionVector left, SprigPositionVector right) {
            return new SprigPositionVector(left.OperativeSlice, Node<Position>.And(left.Head, right.Head));
        }
        
        public static SprigPositionVector operator |(SprigPositionVector left, SprigPositionVector right) {
            return new SprigPositionVector(left.OperativeSlice, Node<Position>.Or(left.Head, right.Head));
        }
        
        #endregion
        
        public Position this[ulong date]
        {
            get => GetPositionNode(date).SuperPosition;
        }

        private PositionNode _head;

        public OperativeSlice OperativeSlice { get; }

        public Node<Position>.INode Head
        {
            get => _head;

            set
            {
                _head = (PositionNode) value;
                
                if(_head.OperativeSliceProvider != this)
                    _head.OperativeSliceProvider = this;
            }
        }

        public SprigPositionVector(OperativeSlice operativeSlice, Node<Position>.INode head)
        {
            OperativeSlice = operativeSlice;
            Head = head;

        }
        
        public SprigPositionVector(OperativeSlice operativeSlice, Type objectType, Node<PositionBuffer>.INode bufferHead)
        {
            OperativeSlice = operativeSlice;
            Head = new PositionNode(null, bufferHead.Index, bufferHead.SuperPosition.PositionAtSlice(objectType, operativeSlice));
            
            var bufferCurrent = bufferHead.Last;
            var vectorCurrent = Head;

            while (bufferCurrent != null)
            {
                vectorCurrent.Last = new PositionNode(null, bufferCurrent.Index, bufferCurrent.SuperPosition.PositionAtSlice(objectType, operativeSlice));

                vectorCurrent = (PositionNode) vectorCurrent.Last;
                bufferCurrent = bufferCurrent.Last;
                
            }
        }

        private PositionNode GetPositionNode(ulong date)
        {
            var current = (PositionNode) Head;

            while (current.Index > date)
            {
                current = (PositionNode) current.Last;
            }

            return current;
        }
        
        public ISprigVector<Position> Copy() {
            return new SprigPositionVector(OperativeSlice, Head.Copy());
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