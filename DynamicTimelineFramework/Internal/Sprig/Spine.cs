
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Spine {
        public SpineBranchNode RootBranch { get; }

        public Spine(Diff rootDiff, Universe rootUniverse)
        {
            RootBranch = new SpineBranchNode(0, null, rootDiff);
            
            rootUniverse.Sprig = RootBranch.AddBranch(rootDiff);
        }

        public void Alloc(int space, int startIndex)
        {
            //Though somewhat recursive, basically amounts to a Depth-First iteration of the spine tree
            RootBranch.Alloc(space, startIndex);
        }

        public Sprig AddBranch(LinkedList<Diff> diffChain) 
        {
            //Use the diff chain to follow the trail of diffs from the spine's root to place the new branch
            var diffToAdd = diffChain.Last.Value;
            
            using (var diffEnum = diffChain.GetEnumerator()) 
            {
                //The diff chain includes the diff that hasn't been spliced in yet, so there's always at least two diffs
                //in the chain; rootDiff and diffToAdd
                
                diffEnum.MoveNext();
                var rootDiff = diffEnum.Current;
                
                var currentBranch = RootBranch;
                var nextDiff = rootDiff;
                
                if (RootBranch[rootDiff] is SpineBranchNode firstBranch && firstBranch.Date < diffToAdd.Date)
                {
                    currentBranch = firstBranch;

                    diffEnum.MoveNext();
                    
                    //The enum is now on the "next turn" we have to take
                    var isAtTurn = !diffEnum.Current.Equals(diffToAdd) && currentBranch.Contains(diffEnum.Current);
                    
                    nextDiff = isAtTurn ? diffEnum.Current : currentBranch.Diff;
                    var nextDate = currentBranch[nextDiff] is SpineBranchNode b0 ? b0.Date : ulong.MaxValue;

                    while (nextDate < diffToAdd.Date)
                    {
                        currentBranch = (SpineBranchNode) currentBranch[nextDiff];
                        isAtTurn = !diffEnum.Current.Equals(diffToAdd) && currentBranch.Contains(diffEnum.Current);

                        if (isAtTurn)
                            diffEnum.MoveNext();
                        
                        nextDiff = isAtTurn ? diffEnum.Current : currentBranch.Diff;
                        nextDate = currentBranch[nextDiff] is SpineBranchNode b1 ? b1.Date : ulong.MaxValue;

                    }
                }

                //Current is looking at the branch node the new branch is going to be added on
                if (currentBranch.Date == diffToAdd.Date) {
                    return currentBranch.AddBranch(diffToAdd);
                }
                
                //Current is looking at the branch node that comes before the spine
                //node the new branch needs to be between. Create a new node and insert in between
                var nextBranch = currentBranch[nextDiff];
                var newBranch = new SpineBranchNode(diffToAdd.Date, nextBranch.ParentSprig, nextDiff);
                currentBranch[nextDiff] = newBranch;
                newBranch[nextDiff] = nextBranch;
                
                //Add the new branch to the branch node we just made
                return newBranch.AddBranch(diffToAdd);
            }
        }
    }
}