namespace MultiverseGraph.Core.Tools.Sprig {
    internal class Sprig {
        
        public Spine Spine { get; }
        
        public SHeadNode Head { get; }
        
        #region CONSTRUCTORS
        
        public Sprig(Diff rootDiff)
        {
            Spine = new Spine(this, new Position(), rootDiff);
            Head = (SHeadNode) Spine.Root;
        }
        
        public Sprig(Position position, Diff rootDiff)
        {
            Spine = new Spine(this, position, rootDiff);
            Head = (SHeadNode) Spine.Root;
        }
        
        #endregion
        
    }
}