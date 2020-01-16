using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal {
    internal class SprigVector {

        public static SprigVector operator &(SprigVector left, SprigVector right) {
            
        }
        
        public static SprigVector operator |(SprigVector left, SprigVector right) {
            
        }
        
        private readonly SprigNode _head = new SprigNode(null, ulong.MaxValue);

        public SprigVector()
        {
            _head.Last = new SprigNode(null, 0);
        }

        public void Add(DTFObject obj, PositionVector vector)
        {
            var currentPositionNode = vector.Head;
            var currentBuilderNode = _head;

            while (currentPositionNode != null)
            {
                while (currentBuilderNode.Last.Index > currentPositionNode.Index)
                {
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    currentBuilderNode.Add(obj, currentPositionNode.SuperPosition);
                }
                
                if (currentBuilderNode.Last.Index < currentPositionNode.Index)
                {
                    currentBuilderNode.Last = new SprigNode((SprigNode) currentBuilderNode.Last, currentPositionNode.Index, obj, currentPositionNode.SuperPosition);
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    
                    currentBuilderNode.Add((SprigNode) currentBuilderNode.Last);
                }

                else if (currentBuilderNode.Last.Index == currentPositionNode.Index)
                {
                    currentBuilderNode = (SprigNode) currentBuilderNode.Last;
                    
                    currentBuilderNode.Add(obj, currentPositionNode.SuperPosition);
                }

                currentPositionNode = (PositionNode) currentPositionNode.Last;
            }
        }

        public bool Validate() {
            return _head.Validate();
        }
    }
}