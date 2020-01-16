using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal {
    internal class SpineHeadNode : SpineNode
    {
        public override Sprig Sprig { get; }

        public SpineHeadNode(Sprig root, ulong index, Diff diff)
        {
            Sprig = new Sprig(index, root, diff);
        }
    }
}