using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class BufferVector : ISprigVector<PositionBuffer>
    {
        #region STATIC

        public static BufferVector operator &(BufferVector left, BufferVector right) {
            return new BufferVector(Node<PositionBuffer>.And(left.Head, right.Head));
        }
        
        public static BufferVector operator |(BufferVector left, BufferVector right) {
            return new BufferVector(Node<PositionBuffer>.Or(left.Head, right.Head));
        }
        
        public static BufferVector operator &(BufferVector left, PositionVector right) {
            return new BufferVector(Node<PositionBuffer>.And(left.Head, right.Head));
        }
        
        public static BufferVector operator |(BufferVector left, PositionVector right) {
            return new BufferVector(Node<PositionBuffer>.Or(left.Head, right.Head));
        }
        
        #endregion

        public PositionBuffer this[ulong date]
        {
            get => GetBufferNode(date).SuperPosition;
        }

        public Node<PositionBuffer>.INode Head { get; private set; }
        
        public BufferVector(Node<PositionBuffer>.INode head)
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
        
        public BufferVector(int size, bool defaultValue = false)
        {
            Head = new BufferNode(null, 0, new PositionBuffer(size, defaultValue));
        }

        public ISprigVector<PositionBuffer> Copy()
        {
            return new BufferVector(Head.Copy());
        }

        public bool Validate(SprigManager manager, params DTFObject[] objects)
        {
            foreach (var dtfObject in objects)
            {
                if (!((PositionNode) ToPositionVector(dtfObject).Head).Validate())
                    return false;
            }

            return true;
        }
        
        public PositionVector ToPositionVector(DTFObject dtfObject)
        {
            return new PositionVector(dtfObject.SprigManagerSlice, dtfObject.GetType(), Head);
        }

        public void Clear(IEnumerable<DTFObject> objects, bool value = false) {
            var current = Head;

            while (current != null) {
                foreach (var dtfObject in objects) {
                    current.SuperPosition.Clear(dtfObject.SprigManagerSlice, value);
                }
                
                current = current.Last;
            }
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