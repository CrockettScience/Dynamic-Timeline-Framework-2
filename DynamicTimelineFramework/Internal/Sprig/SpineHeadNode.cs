using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineHeadNode : SpineNode
    {
        public override Sprig Sprig { get; }

        public SpineHeadNode(Sprig root, ulong index, Diff diff)
        {
            Sprig = new Sprig(index, root, diff);
        }

        public override void Alloc(int space, int startIndex)
        {
            Sprig.Alloc(space, startIndex);
        }
    }
}