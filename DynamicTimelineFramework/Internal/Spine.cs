using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal {
    internal class Spine {
        public SpineBranchNode RootBranch { get; }

        public Spine(Diff rootDiff, Universe rootUniverse)
        {
            RootBranch = new SpineBranchNode(0, null);
            
            rootUniverse.Sprig = RootBranch.AddBranch(rootDiff);
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

                        diffEnum.MoveNext();
                        nextTurn = diffEnum.Current;
                    }
                }

                //Current is looking at the branch node the new branch is going to be added on
                if (currentBranchNode.Date == branchDate) {
                    return currentBranchNode.AddBranch(nextTurn);
                }
                
                //Current is looking at the branch node that comes before the spine
                //node the new branch needs to be between. Create a new node and insert in between
                var nextNode = currentBranchNode[nextDiff];
                var newBranch = new SpineBranchNode(branchDate, nextNode.Sprig);
                currentBranchNode[nextDiff] = newBranch;
                newBranch.AddBranch(nextNode);
                
                //Add the new branch to the branch node we just made
                return newBranch.AddBranch(nextTurn);
            }
        }

        public void RemoveBranch(LinkedList<Diff> diffChain) {
            //Use the diff chain to follow the chain of diffs to the first place where the diff breaks off

            var diffToLookFor = diffChain.Last.Value;
            using (var diffEnum = diffChain.GetEnumerator()) {
                
                diffEnum.MoveNext();
                var nextDiff = diffEnum.Current;

                diffEnum.MoveNext();
                var nextTurn = diffEnum.Current;
                
                var currentBranchNode = RootBranch;

                while (nextDiff != diffToLookFor) {
                    
                    currentBranchNode = (SpineBranchNode) currentBranchNode[nextDiff];
                    
                    if (nextTurn != null && currentBranchNode.Contains(nextTurn)) {
                        nextDiff = nextTurn;

                        nextTurn = diffEnum.MoveNext() ? diffEnum.Current : null;
                    }
                }
                
                //We're at the "graft" point of the branch
                var graftPoint = currentBranchNode;
                
                //Navigate along this diff strand and graft each other branch to the graft point
                while (graftPoint.Contains(diffToLookFor)) {
                    
                    //Remove the nextBranch and replace it with the nextBranch's next branches
                    var nextNode = graftPoint.RemoveBranch(diffToLookFor);

                    //If it's a branch node, we need to graft it's children to the graft point. Else, we can let it rot
                    if (nextNode is SpineBranchNode nextBranchNode) 
                        graftPoint.GraftBranches(nextBranchNode);

                }
            }
        }
    }
}