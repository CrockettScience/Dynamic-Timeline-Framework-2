using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class PositionNode : Node<Position>.INode, IOperativeSliceProvider
    {
        private Position _position;

        private IOperativeSliceProvider _operativeSliceProvider;
        
        private PositionNode _last;
        
        public Position SuperPosition
        {
            get => _position;

            set
            {
                _position = value;
                _position.OperativeSliceProvider = this;
            }
        }

        public IOperativeSliceProvider OperativeSliceProvider
        {
            get => _operativeSliceProvider;
            
            set
            {
                _operativeSliceProvider = value;
                if(_last != null)
                    _last.OperativeSliceProvider = _operativeSliceProvider;
            }
        }

        public Node<Position>.INode Last
        {
            get => _last;

            set
            {
                _last = (PositionNode) value;
                
                if(_last != null)
                    _last._operativeSliceProvider = _operativeSliceProvider;
            }
        }

        public ulong Index { get; set; }

        public OperativeSlice OperativeSlice => OperativeSliceProvider.OperativeSlice;

        public Node<Position>.INode MakeNode(Node<Position>.INode last, ulong index, Position superPosition)
        {
            return new PositionNode(last, index, superPosition);
        }

        public PositionNode(Node<Position>.INode last, ulong index, Position superPosition)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
        }

        public Node<Position>.INode Copy()
        {
            return new PositionNode(Last.Copy(), Index, (Position) SuperPosition.Copy());
        }
    }
}