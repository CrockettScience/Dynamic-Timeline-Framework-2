using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class PositionNode : Node<Position>.INode, IOperativeSliceProvider
    {
        private Position _position;
        
        public Position SuperPosition
        {
            get => _position;

            set
            {
                _position = value;
                _position.OperativeSliceProvider = this;
            }
        }

        public Node<Position>.INode Last { get; set; }
        
        public ulong Index { get; set; }

        public Slice OperativeSlice { get; set; }
        
        public Node<Position>.INode MakeNode(Node<Position>.INode last, ulong index, Position superPosition)
        {
            return new PositionNode(last, index, superPosition, OperativeSlice);
        }

        public PositionNode(Node<Position>.INode last, ulong index, Position superPosition, Slice operativeSlice)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
            OperativeSlice = operativeSlice;
        }

        public Node<Position>.INode Copy()
        {
            return new PositionNode(Last.Copy(), Index, (Position) SuperPosition.Copy(), OperativeSlice);
        }
    }
}