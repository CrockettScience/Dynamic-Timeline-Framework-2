using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal {
    internal class SpineHeadNode : SpineNode
    {
        public override Sprig Sprig { get; }

        public SpineHeadNode(Sprig root, ulong index, Diff diff) {
            Sprig = index == 0 ? new Sprig(diff) : new Sprig(index, root, diff);
        }
    }
}