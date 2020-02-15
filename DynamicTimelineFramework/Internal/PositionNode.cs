using System;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal
{
    internal class PositionNode : Node.INode
    {
        public Position SuperPosition { get; set; }

        public Node.INode Last { get; set; }

        public ulong Index { get; set; }
        
        public Node.INode MakeNode(Node.INode last, ulong index, Position superPosition) {
            return new PositionNode((PositionNode) last, index, superPosition);
        }

        public PositionNode(PositionNode last, ulong index, Position superPosition)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
        }

        public bool Validate()
        {
            if (SuperPosition.Uncertainty == -1)
                return false;

            return Last == null || Last.Validate();
        }

        public Node.INode AndFactory(Node.INode left, Node.INode right) {
            var leftPos = (PositionNode) left;
            var rightPos = (PositionNode) right;
            
            return new PositionNode(null, Math.Max(leftPos.Index, rightPos.Index), leftPos.SuperPosition & rightPos.SuperPosition);
        }

        public Node.INode OrFactory(Node.INode left, Node.INode right) {
            var leftPos = (PositionNode) left;
            var rightPos = (PositionNode) right;
            
            return new PositionNode(null, Math.Max(leftPos.Index, rightPos.Index), leftPos.SuperPosition | rightPos.SuperPosition);
        }

        public bool IsSamePosition(Node.INode other) {
            return SuperPosition.Equals(((PositionNode) other).SuperPosition);
        }
    }
}