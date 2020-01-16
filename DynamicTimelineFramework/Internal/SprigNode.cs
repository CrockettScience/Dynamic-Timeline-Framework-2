using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class SprigNode : Node.INode
    {
        public Node.INode Last { get; set; }
        
        public ulong Index { get; set; }
        
        public bool Validate() {
            throw new System.NotImplementedException();
        }

        public Node.INode AndFactory(Node.INode left, Node.INode right, ulong index) {
            throw new System.NotImplementedException();
        }

        public Node.INode OrFactory(Node.INode left, Node.INode right, ulong index) {
            throw new System.NotImplementedException();
        }

        public bool IsSamePosition(Node.INode other) {
            throw new System.NotImplementedException();
        }

        private readonly Dictionary<DTFObject, Position> _positions;

        public SprigNode(SprigNode last, ulong index, DTFObject obj, Position pos)
        {
            Index = index;
            Last = last;
            _positions = new Dictionary<DTFObject, Position>();
            
            if(pos != null)
                _positions[obj] = pos;
        }
        
        public SprigNode(SprigNode last, ulong index)
        {
            Index = index;
            Last = last;
            _positions = new Dictionary<DTFObject, Position>();
        }

        public void Add(DTFObject obj, Position pos)
        {
            _positions[obj] = pos;
        }
        
        public void Add(SprigNode otherNode)
        {
            foreach (var pair in otherNode._positions)
                
                _positions[pair.Key] = pair.Value;
        }

        public Position GetPosition(DTFObject obj) {
            return _positions[obj];
        }
        
        public bool HasPosition(DTFObject obj) {
            return _positions.ContainsKey(obj);
        }
        
        public SprigNode GetSprigNode(ulong date)
        {
            var current = this;

            while (current.Index > date)
            {
                current = (SprigNode) current.Last;
            }

            return current;
        }

        public override bool Equals(object obj) {
            if (!(obj is SprigNode other)) return false;

            if (Index != other.Index || !_positions.Equals(other._positions)) return false;

            return Last?.Equals(other.Last) ?? true;

        }
    }
}