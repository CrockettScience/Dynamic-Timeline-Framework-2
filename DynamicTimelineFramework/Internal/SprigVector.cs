using System.Collections.Generic;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class SprigVector {
        private readonly bool _dynamicObjectActivity;

        public static SprigVector operator &(SprigVector left, SprigVector right) {
            return new SprigVector(Node.And(left.Head, right.Head));
        }
        
        public static SprigVector operator |(SprigVector left, SprigVector right) {
            return new SprigVector(Node.Or(left.Head, right.Head));
        }
        
        private readonly SprigNode _pointer = new SprigNode(null, ulong.MaxValue);
        
        private readonly Dictionary<DTFObject, bool> _objectActivity = new Dictionary<DTFObject, bool>();
        public int ActiveObjects { get; private set; }

        public SprigNode Head {
            get => (SprigNode) _pointer.Last;
            private set => _pointer.Last = value;
        }

        public SprigVector(bool dynamicObjectActivity = false)
        {
            _dynamicObjectActivity = dynamicObjectActivity;
            Head = new SprigNode(null, 0);
        }

        public bool IsActive(DTFObject obj)
        {
            return !_dynamicObjectActivity || _objectActivity[obj];
        }

        public void DisableObject(DTFObject obj)
        {
            if (_objectActivity[obj]) {
                _objectActivity[obj] = false;
                ActiveObjects--;
            }
        }
        
        public SprigVector(SprigNode head) {
            Head = head;
        }

        public void Add(DTFObject obj, PositionVector vector) {
            var currentPositionNode = vector.Head;
            var currentBuilderNode = _pointer;
            
            _objectActivity[obj] = true;
            ActiveObjects++;

            while (currentPositionNode != null) {
                while (currentBuilderNode.Last.Index > currentPositionNode.Index) {
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    currentBuilderNode.Add(obj, currentPositionNode.SuperPosition);
                }
                
                if (currentBuilderNode.Last.Index < currentPositionNode.Index) {
                    currentBuilderNode.Last = new SprigNode(this, (SprigNode) currentBuilderNode.Last, currentPositionNode.Index, obj, currentPositionNode.SuperPosition);
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    
                    currentBuilderNode.Add((SprigNode) currentBuilderNode.Last);
                }

                else if (currentBuilderNode.Last.Index == currentPositionNode.Index) {
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    
                    currentBuilderNode.Add(obj, currentPositionNode.SuperPosition);
                }

                currentPositionNode = (PositionNode) currentPositionNode.Last;
            }
        }

        public PositionVector ToPositionVector(DTFObject obj) {
            var currentS = (SprigNode) _pointer.Last;
            PositionNode currentP;
            var posHead = currentP = new PositionNode(null, currentS.Index, currentS.GetPosition(obj));

            currentS = (SprigNode) currentS.Last;

            while (currentS != null) {
                var newP = new PositionNode(null, currentS.Index, currentS.GetPosition(obj));

                if (newP.IsSamePosition(currentP))
                    currentP.Index = newP.Index;

                else 
                    currentP = (PositionNode) (currentP.Last = newP);
                
                currentS = (SprigNode) currentS.Last;
            }
            
            return new PositionVector(posHead);
        }

        public bool Validate() {
            return Head.Validate();
        }
    }
}