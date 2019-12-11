
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Spine {
        public SpineBranchNode RootBranch { get; }

        public Spine(Diff rootDiff, Universe rootUniverse)
        {
            RootBranch = new SpineBranchNode(0, null);
            
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
            var branchDate = diffChain.Last.Value.Date;
            
            using (var diffEnum = diffChain.GetEnumerator()) 
            {
                //The diff chain includes the diff that hasn't been spliced in yet, so there's always at least two diffs
                //in the chain; rootDiff and nextDiff
                
                diffEnum.MoveNext();
                var nextDiff = diffEnum.Current;

                diffEnum.MoveNext();
                var nextTurn = diffEnum.Current;
                
                var currentBranchNode = RootBranch;

                while (currentBranchNode[nextDiff] is SpineBranchNode nextBranchNode && nextBranchNode.Date <= branchDate) {
                    
                    currentBranchNode = nextBranchNode;
                    
                    if (nextTurn != null && currentBranchNode.Contains(nextTurn)) {
                        nextDiff = nextTurn;

                        nextTurn = diffEnum.Current;
                    }
                }

                //Current is looking at the branch node the new branch is going to be added on
                if (currentBranchNode.Date == branchDate) {
                    return currentBranchNode.AddBranch(nextTurn);
                }
                
                //Current is looking at the branch node that comes before the spine
                //node the new branch needs to be between. Create a new node and insert in between
                var nextBranch = currentBranchNode[nextDiff];
                var newBranch = new SpineBranchNode(branchDate, nextBranch.ParentSprig);
                currentBranchNode[nextDiff] = newBranch;
                newBranch.AddBranch(nextBranch);
                
                //Add the new branch to the branch node we just made
                return newBranch.AddBranch(nextTurn);
            }
        }
    }
}