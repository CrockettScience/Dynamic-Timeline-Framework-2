using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class Spine<T> where T : DTFObject {
        
        public SpineNode Root { get; }

        public Spine(Sprig<T> initialHeadOwner, Position<T> initialPosition, Diff rootDiff)
        {
            Root = new SBranchNode(0, rootDiff)
            {
                [rootDiff] = new SHeadNode<T>(initialHeadOwner, null, initialPosition, 0, rootDiff)
            }; 
        }
    }
}