namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class Sprig {
        
        public Spine Spine { get; }
        
        public SHeadNode SpineHead { get; }

        public Position this[ulong index] {
            get {
                var current = SpineHead.SprigHead;
                
                while (index < current.Index) {
                    current = current.Last;
                }

                return current.Position;
            }
        }
        
        #region CONSTRUCTORS
        
        public Sprig(Diff rootDiff)
        {
            Spine = new Spine(this, new Position(), rootDiff);
            SpineHead = (SHeadNode) Spine.Root;
        }
        
        public Sprig(Position defaultPosition, Diff rootDiff)
        {
            Spine = new Spine(this, defaultPosition, rootDiff);
            SpineHead = (SHeadNode) Spine.Root;
        }

        public Sprig(Sprig branchFrom, Diff diff, Position defaultPosition) {
            Spine = branchFrom.Spine;
            
            //Find the SprigNode to branch off of
            var current = branchFrom.SpineHead.SprigHead;

            while (current.Index > diff.Date) {
                current = current.Last;
            }
            
            var sprigHead = new SprigNode(current, defaultPosition, diff.Date);
            SpineHead = new SHeadNode(this, sprigHead, diff);
            
            //Splice the new SpineHead into the Spine
            branchFrom.SpineHead.AddBranch(SpineHead, diff.Date);
        }
        
        #endregion
        
        
        
    }
}