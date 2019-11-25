using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineHeadNode : SpineNode
    {
        public override Sprig ParentSprig { get; }

        public SpineHeadNode(Sprig root, ulong index, Diff diff)
        {
            ParentSprig = new Sprig(index, root, diff);
        }

        public override void Alloc(int space, int startIndex)
        {
            ParentSprig.Alloc(space, startIndex);
        }
    }
}