namespace MultiverseGraph.Core.Tools.Sprig {
    internal class SHeadNode : SpineNode
    {
        public Sprig Owner { get; }
        
        public SprigNode Head { get; }

        public SHeadNode(Sprig owner, SprigNode last, Position position, int i, Diff diff) : base(diff)
        {
            Head = new SprigNode(last, position, i);
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
                var newBranchNode = new SBranchNode(branchIndex, child.Diff);
                current[child.Diff] = newBranchNode;
                
                //Add child branch as a branch to the new node as well as a connection to the new head
                newBranchNode[child.Diff] = child;
                newBranchNode[newHead.Diff] = newHead;
            }
        }
    }
}