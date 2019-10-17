using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineHeadNode : SpineNode
    {
        public override Sprig RootSprig { get; }

        public SpineHeadNode(Sprig root, ulong index, Diff diff)
        {
            RootSprig = new Sprig(index, root, diff);
        }

        public override void Alloc(int space, int startIndex)
        {
            RootSprig.Alloc(space, startIndex);
        }
    }
}