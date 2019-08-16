namespace MultiverseGraph.Core.Tools.Sprig {
    internal class SprigNode {

        public SprigNode Last { get; }
        
        public int Index { get; }
        
        public Position Position { get; }
    
        public SprigNode(SprigNode last, Position position, int i)
        {
            Last = last;
            Index = i;
            Position = position;
        }
    }
}