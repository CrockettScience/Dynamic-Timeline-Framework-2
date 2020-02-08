using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal {
    internal class SpineBranchNode : SpineNode, IEnumerable<Diff>
    {
        private readonly Dictionary<Diff, Branch> _branchMap;
        public ulong Date { get; }
        public override Sprig Sprig { get; }
        
        public SpineNode this[Diff diff] {
            get => _branchMap[diff].Next;

            set => _branchMap[diff].Next = value;
        }

        public SpineBranchNode(ulong date, Sprig parentSprig) {
            _branchMap = new Dictionary<Diff, Branch>();
            Date = date;
            Sprig = parentSprig;
        }

        public bool Contains(Diff diff) {
            return _branchMap.ContainsKey(diff);
        }

        public Sprig AddBranch(Diff diff) {
            var newHead = new SpineHeadNode(Sprig, Date, diff);
            
            _branchMap[diff] = new Branch(newHead, this);
            return newHead.Sprig;
        }
        
        public void AddBranch(SpineNode next) {
            var diff = next.Sprig.Diff;
            _branchMap[diff] = new Branch(next, this);
        }

        public SpineNode RemoveBranch(Diff diff) {
            var branch = _branchMap[diff].Next;
            _branchMap.Remove(diff);
            return branch;
        }

        public void GraftBranches(SpineBranchNode graftFrom) {
            foreach (var branch in graftFrom._branchMap.Values) {
                AddBranch(branch.Next);
            }
        }

        public Branch GetBranch(Diff diff) {
            return _branchMap[diff];
        }

        public class Branch
        {
            private readonly SpineBranchNode _owner;

            public SpineNode Next { get; set; }

            public SprigNode FirstNodeOnBranch
            {
                get => Next.Sprig.Head.GetSprigNode(_owner.Date);
                
                set {
                    var first = FirstNodeOnBranch;
                    
                    if(first != Next.Sprig.Head){

                        var second = Next.Sprig.Head;

                        while (second.Last != first)
                            second = (SprigNode) second.Last;

                        second.Last = value;
                    }

                    else {
                        first.Last = value;
                    }
                }
            }

            public Branch(SpineNode next, SpineBranchNode owner) {
                Next = next;
                _owner = owner;
            }
        }

        public IEnumerator<Diff> GetEnumerator() {
            return _branchMap.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}