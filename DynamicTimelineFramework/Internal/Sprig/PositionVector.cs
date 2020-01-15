using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class PositionVector
    {
        
        #region STATIC

        public static PositionVector operator &(PositionVector left, PositionVector right) {
            return new PositionVector(PositionNode.And(left.Head, right.Head));
        }
        
        public static PositionVector operator |(PositionVector left, PositionVector right) {
            return new PositionVector(PositionNode.Or(left.Head, right.Head));
        }
        
        #endregion
        
        public Position this[ulong date]
        {
            get => GetPositionNode(date).SuperPosition;
        }

        public PositionNode Head { get; set; }

        public PositionVector(PositionNode head)
        {
            Head = head;

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