using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig
    {
        private BufferNode _head;
        
        public Diff Diff { get; }
        
        public BufferNode Head
        {
            get => _head;

            set
            {
                //Todo - We need to inform the spine that a new head is assigned, which will inform the neighboring branches
            }
        }
        
        public Sprig(ulong index, Sprig root, Diff diff)
        {
            Head = new BufferNode(root?.GetBufferNode(index - 1), index, new PositionBuffer());
            Diff = diff;
        }

        public void Alloc(int space, int startIndex)
        {
            var current = Head;
            var buffer = current.SuperPosition;
            
            //Don't allocate if the buffer is already allocated
            while (buffer != null && buffer.Length == startIndex)
            {
                buffer.Alloc(space);
                
                //Perform the same on the last
                current = (BufferNode) current.Last;
                buffer = current?.SuperPosition;
            }
        }

        public Position GetPosition(ulong date, DTFObject obj) {
            var bNode = GetBufferNode(date);

            var slice = new Slice(bNode.SuperPosition, obj.SprigBuilderSlice.LeftBound, obj.SprigBuilderSlice.RightBound);
            
            return new Position(obj.GetType(), slice);
        }
        
        public BufferNode GetBufferNode(ulong date)
        {
            var current = Head;

            while (current.Index > date)
            {
                current = (BufferNode) current.Last;
            }

            return current;
        }
        
        public void And<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.And(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = (BufferNode) newHead;
        }
        
        public void Or<T>(ISprigVector<T> other) where T : BinaryPosition
        {
            var newHead = Node<PositionBuffer>.Or(Head, other.Head);
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = (BufferNode) newHead;
        }
    }
}