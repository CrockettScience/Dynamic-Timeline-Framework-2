using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class SprigVector {

        public static SprigVector operator &(SprigVector left, SprigVector right) {
            return new SprigVector(Node.And(left.Head, right.Head));
        }
        
        public static SprigVector operator |(SprigVector left, SprigVector right) {
            return new SprigVector(Node.Or(left.Head, right.Head));
        }
        
        private readonly SprigNode _pointer = new SprigNode(null, ulong.MaxValue);

        public SprigNode Head {
            get => (SprigNode) _pointer.Last;
            private set => _pointer.Last = value;
        }

        public SprigVector() {
            Head = new SprigNode(null, 0);
        }
        
        public SprigVector(SprigNode head) {
            Head = head;
        }

        public void Add(DTFObject obj, PositionVector vector) {
            var currentPositionNode = vector.Head;
            var currentBuilderNode = _pointer;

            while (currentPositionNode != null) {
                while (currentBuilderNode.Last.Index > currentPositionNode.Index) {
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    currentBuilderNode.Add(obj, currentPositionNode.SuperPosition);
                }
                
                if (currentBuilderNode.Last.Index < currentPositionNode.Index) {
                    currentBuilderNode.Last = new SprigNode((SprigNode) currentBuilderNode.Last, currentPositionNode.Index, obj, currentPositionNode.SuperPosition);
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

        public bool Validate() {
            return Head.Validate();
        }
    }
}