using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

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
        
        public static SprigBufferVector operator &(SprigBufferVector left, PositionVector right) {
            return new SprigBufferVector(Node<PositionBuffer>.And(left.Head, right.Head));
        }
        
        public static SprigBufferVector operator |(SprigBufferVector left, PositionVector right) {
            return new SprigBufferVector(Node<PositionBuffer>.Or(left.Head, right.Head));
        }
        
        #endregion

        public PositionBuffer this[ulong date]
        {
            get => GetBufferNode(date).SuperPosition;
        }

        public Node<PositionBuffer>.INode Head { get; private set; }
        
        public SprigBufferVector(Node<PositionBuffer>.INode head)
        {

            Head = head;

        }
        
        public BufferNode GetBufferNode(ulong date)
        {
            var current = (BufferNode) Head;

            while (current.Index > date)
            {
                current = (BufferNode) current.Last;
            }

            return current;
        }
        
        public SprigBufferVector(int size, bool defaultValue = false)
        {
            Head = new BufferNode(null, 0, new PositionBuffer(size, defaultValue));
        }

        public ISprigVector<PositionBuffer> Copy()
        {
            return new SprigBufferVector(Head.Copy());
        }

        public bool Validate(SprigBuilder builder)
        {
            foreach (var dtfObject in builder.Registry)
            {
                if (!((PositionNode) ToPositionVector(dtfObject).Head).Validate())
                    return false;
            }

            return true;
        }
        
        public PositionVector ToPositionVector(DTFObject dtfObject)
        {
            return new PositionVector(dtfObject.SprigBuilderSlice, dtfObject.GetType(), Head);
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