using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class SprigNode<T> where T : DTFObject {

        public SprigNode<T> Last { get; set; }

        public ulong Index { get; set; }

        public Position<T> Position { get; }
    
        public SprigNode(SprigNode<T> last, Position<T> position, ulong i)
        {
            Last = last;
            Index = i;
            Position = position;
        }
    }
}