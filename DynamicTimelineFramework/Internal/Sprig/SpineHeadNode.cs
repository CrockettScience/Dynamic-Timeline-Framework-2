using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineHeadNode : SpineNode
    {
        public Sprig Sprig { get; }

        public SpineHeadNode()
        {
            Sprig = new Sprig();
        }

        public override void Alloc(int space, int startIndex)
        {
            Sprig.Alloc(space, startIndex);
        }
    }
}