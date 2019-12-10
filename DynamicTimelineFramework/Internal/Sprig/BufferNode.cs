using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class BufferNode : Node<PositionBuffer>.INode {

        public PositionBuffer SuperPosition { get; set; }
        public Node<PositionBuffer>.INode Last { get; set; }
        public ulong Index { get; set; }
        
        public Node<PositionBuffer>.INode MakeNode(Node<PositionBuffer>.INode last, ulong index, PositionBuffer superPosition)
        {
            return new BufferNode(last, index, superPosition);
        }

        public BufferNode(Node<PositionBuffer>.INode last, ulong index, PositionBuffer superPosition)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
        }

        public override bool Equals(object obj) {
            if (!(obj is BufferNode other)) return false;

            if (Index != other.Index || !SuperPosition.Equals(other.SuperPosition)) return false;

            return Last?.Equals(other.Last) ?? true;

        }


        public Node<PositionBuffer>.INode Copy()
        {
            return new BufferNode((BufferNode) Last.Copy(), Index, (PositionBuffer) SuperPosition.Copy());
        }
    }
}