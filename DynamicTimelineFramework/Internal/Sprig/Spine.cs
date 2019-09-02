using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Spine{
        
        public SpineNode Root { get; }

        public Spine(Sprig initialHeadOwner, Position initialPosition, Diff rootDiff)
        {
            Root = new SBranchNode(0, rootDiff)
            {
                [rootDiff] = new SHeadNode(initialHeadOwner, null, initialPosition, 0, rootDiff)
            }; 
        }
    }
}