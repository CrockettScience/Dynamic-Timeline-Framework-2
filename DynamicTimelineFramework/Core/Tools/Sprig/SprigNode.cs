namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class SprigNode {

        public SprigNode Last { get; set; }

        public ulong Index { get; set; }

        public Position Position { get; }
    
        public SprigNode(SprigNode last, Position position, ulong i)
        {
            Last = last;
            Index = i;
            Position = position;
        }
    }
}