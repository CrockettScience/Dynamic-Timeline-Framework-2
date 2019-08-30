using DynamicTimelineFramework.Core.Tools.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class Sprig<T> : ISprig<T> where T : DTFObject {
        
        public Spine<T> Spine { get; }
        
        public SHeadNode<T> SpineHead { get; }

        public IPosition<T> this[ulong index] {
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
            Spine = new Spine<T>(this, new Position<T>(), rootDiff);
            SpineHead = (SHeadNode<T>) Spine.Root;
        }
        
        public Sprig(Position<T> defaultPosition, Diff rootDiff)
        {
            Spine = new Spine<T>(this, defaultPosition, rootDiff);
            SpineHead = (SHeadNode<T>) Spine.Root;
        }

        public Sprig(Sprig<T> branchFrom, Diff diff, Position<T> defaultPosition) {
            Spine = branchFrom.Spine;
            
            //Find the SprigNode to branch off of
            var current = branchFrom.SpineHead.SprigHead;

            while (current.Index > diff.Date) {
                current = current.Last;
            }
            
            var sprigHead = new SprigNode<T>(current, defaultPosition, diff.Date);
            SpineHead = new SHeadNode<T>(this, sprigHead, diff);
            
            //Splice the new SpineHead into the Spine
            branchFrom.SpineHead.AddBranch(SpineHead, diff.Date);
        }
        
        #endregion
    }
}