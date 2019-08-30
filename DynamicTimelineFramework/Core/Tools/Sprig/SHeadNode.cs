using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class SHeadNode<T> : SpineNode where T : DTFObject
    {
        public Sprig<T> Owner { get; }
        
        public SprigNode<T> SprigHead { get; }

        public SHeadNode(Sprig<T> owner, SprigNode<T> last, Position<T> position, ulong i, Diff diff) : base(diff)
        {
            SprigHead = new SprigNode<T>(last, position, i);
            Owner = owner;
            Diff = diff;
        }

        public SHeadNode(Sprig<T> owner, SprigNode<T> sprigHead, Diff diff) : base(diff) {
            SprigHead = sprigHead;
            Owner = owner;
            Diff = diff;
        }

        public void AddBranch(SHeadNode<T> newHead, ulong branchIndex)
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
                var newBranchNode = new SBranchNode(branchIndex, child.Diff);
                current[child.Diff] = newBranchNode;
                
                //Add child branch as a branch to the new node as well as a connection to the new head
                newBranchNode[child.Diff] = child;
                newBranchNode[newHead.Diff] = newHead;
            }
        }
    }
}