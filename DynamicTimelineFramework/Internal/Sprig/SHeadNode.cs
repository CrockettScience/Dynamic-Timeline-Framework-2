using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SHeadNode : SpineNode
    {
        public Sprig Owner { get; }
        
        public SprigNode SprigHead { get; }

        public SHeadNode(Sprig owner, SprigNode last, Position position, ulong i, Diff diff) : base(diff)
        {
            SprigHead = new SprigNode(last, position, i);
            Owner = owner;
            Diff = diff;
        }

        public SHeadNode(Sprig owner, SprigNode sprigHead, Diff diff) : base(diff) {
            SprigHead = sprigHead;
            Owner = owner;
            Diff = diff;
        }

        public void AddBranch(SHeadNode newHead, ulong branchIndex)
        {
            var current = Parent;
            var child = (SpineNode) this;

            while (branchIndex < current.BranchIndex)
            {
                child = current;
                current = current.Parent;
            }

            if (branchIndex == current.BranchIndex)
            {
                //Just add a new path directly to the head
                current[newHead.Diff] = newHead;
            }

            else
            {
                //Make a new branchNode to replace the connection to last, whose index is the branch index passed into the method
                //Todo - Somehow compute the "crumple lengths"
                var newBranchNode = new SBranchNode(branchIndex, child.Diff);
                current[child.Diff] = newBranchNode;
                
                //Add child branch as a branch to the new node as well as a connection to the new head
                newBranchNode[child.Diff] = child;
                newBranchNode[newHead.Diff] = newHead;
            }
        }
    }
}