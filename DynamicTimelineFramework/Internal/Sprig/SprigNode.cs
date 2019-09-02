using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigNode {

        public SprigNode Last { get; set; }

        public ulong Index { get; set; }

        public Position Position { get; }
    
        public SprigNode(SprigNode last, Position position, ulong i)
        {
            Last = last;
            Index = i;
            Position = position;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SprigNode other)) return false;
            
            if (!Position.Equals(other.Position) || Index != other.Index)
                return false;
                
            //Recursive until last is null
            if (Last != null)
            {
                return other.Last != null && Last.Equals(other.Last);
            }

            return other.Last == null;

        }
    }
}