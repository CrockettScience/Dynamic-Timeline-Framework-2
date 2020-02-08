using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class SprigNode : Node.INode {
        public readonly SprigVector Owner;
        
        public Node.INode Last { get; set; }
        
        public ulong Index { get; set; }

        private readonly Dictionary<DTFObject, Position> _positions;

        public SprigNode(SprigVector owner, SprigNode last, ulong index, DTFObject obj, Position pos)
        {
            Owner = owner;
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

        public IEnumerable<DTFObject> GetRootedObjects() {
            return _positions.Keys;
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

        public bool Contains(DTFObject obj) {
            return _positions.ContainsKey(obj);
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
        
        public bool Validate() {
            foreach (var pair in _positions) {
                if (pair.Value.Uncertainty == -1)
                    return false;
            }

            return true;    
        }

        public Node.INode AndFactory(Node.INode left, Node.INode right, ulong index) {
            var leftNode = (SprigNode) left;
            var rightNode = (SprigNode) right;

            var newNode = leftNode.Copy();

            foreach (var rPair in rightNode._positions)
                if (newNode._positions.ContainsKey(rPair.Key) && (rightNode.Owner == null || rightNode.Owner.IsActive(rPair.Key)))
                    newNode._positions[rPair.Key] &= rPair.Value;

            newNode.Index = Node.Max(left.Index, right.Index, index);

            return newNode;
        }

        public Node.INode OrFactory(Node.INode left, Node.INode right, ulong index) {
            var leftNode = (SprigNode) left;
            var rightNode = (SprigNode) right;

            var newNode = leftNode.Copy();

            foreach (var rPair in rightNode._positions)
                if (newNode._positions.ContainsKey(rPair.Key) && (rightNode.Owner == null || rightNode.Owner.IsActive(rPair.Key)))
                    newNode._positions[rPair.Key] |= rPair.Value;

            newNode.Index = Node.Max(left.Index, right.Index, index);

            return newNode;
        }

        public bool IsSamePosition(Node.INode node) {
            if (!(node is SprigNode other)) return false;
            
            foreach (var pair in _positions) {
                if (!other._positions.ContainsKey(pair.Key) || !other._positions[pair.Key].Equals(pair.Value)) return false;
            }

            return true;
        }

        private SprigNode Copy() {
            var copy = new SprigNode(null, Index);

            foreach (var pair in _positions) {
                copy._positions[pair.Key] = pair.Value;
            }

            return copy;
        }

        public override bool Equals(object obj) {
            if (!(obj is SprigNode other)) return false;

            if (Index != other.Index) 
                return false;

            if (!IsSamePosition(other))
                return false;

            return Last == null || Last.Equals(other.Last);
        }
    }
}