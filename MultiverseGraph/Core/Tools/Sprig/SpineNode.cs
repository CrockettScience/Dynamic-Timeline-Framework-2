namespace MultiverseGraph.Core.Tools.Sprig {
    internal abstract class SpineNode
    {

        public SBranchNode Parent { get; set; }
        
        public Diff Diff { get; set; }

        public SpineNode(Diff diff)
        {
            Diff = diff;
        }
    }
}