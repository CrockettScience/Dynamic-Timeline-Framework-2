
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Spine {
        private SpineBranchNode _root;

        public Spine(Diff rootDiff)
        {
            _root = new SpineBranchNode(0);
            _root.AddBranch(rootDiff);
        }

        public void Alloc(int space, int startIndex)
        {
            //Though somewhat recursive, basically amounts to a Depth-First iteration of the spine tree
            _root.Alloc(space, startIndex);
        }

        public Sprig AddBranch(LinkedList<Diff> diffChain) {
            //Use the diff chain to follow the trail of diffs from the spine's root to place the new branch
            var diffToAdd = diffChain.Last.Value;
            
            using (var diffEnum = diffChain.GetEnumerator()) {
                var current = _root;

                diffEnum.MoveNext();
                var atDiff = diffEnum.Current;
                diffEnum.MoveNext();
                var lookingForDiff = diffEnum.Current;
                
                var nextDiff = current.Contains(lookingForDiff) ? lookingForDiff : atDiff;
                var nextDate = current[nextDiff] is SpineBranchNode b0 ? b0.Date : ulong.MaxValue;

                while (nextDate < diffToAdd.Date) {
                    current = (SpineBranchNode) current[nextDiff];

                    if (current.Contains(lookingForDiff)) {
                        nextDiff = lookingForDiff;

                        diffEnum.MoveNext();
                        lookingForDiff = diffEnum.Current;
                    }

                    nextDate = current[nextDiff] is SpineBranchNode b1 ? b1.Date : ulong.MaxValue;

                }
                
                //Current is looking at the branch node that comes before the spine
                //node the new branch needs to be between
                if (current.Date == lookingForDiff.Date) {
                    return current.AddBranch(lookingForDiff);
                }
                
                var atDiffNext = current[nextDiff];
                var intermediary = new SpineBranchNode(lookingForDiff.Date);
                current[nextDiff] = intermediary;

                intermediary[nextDiff] = atDiffNext;
                return intermediary.AddBranch(lookingForDiff);
            }
        }
    }
}