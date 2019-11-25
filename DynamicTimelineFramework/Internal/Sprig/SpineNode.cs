using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal abstract class SpineNode
    {
        public abstract void Alloc(int space, int startIndex);
        public abstract Sprig ParentSprig { get; }
    }
}