using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
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
            
            _branchMap[diff] = new Branch(newHead, newHead.Sprig.Head);
            return newHead.Sprig;
        }
        
        public void AddBranch(SpineNode next) {
            var diff = next.Sprig.Diff;
            _branchMap[diff] = new Branch(next, next.Sprig.GetBufferNode(Date));
        }

        public override void Alloc(int space, int startIndex)
        {
            var nexts = _branchMap.Values;
            
            foreach (var next in nexts)
            {
                next.Next.Alloc(space, startIndex);
            }
        }

        public Branch GetBranch(Diff diff) {
            return _branchMap[diff];
        }

        public class Branch
        {

            public BufferNode First { get; set; }
            
            public SpineNode Next { get; set; }

            public BufferNode FirstNodeOnBranch
            {
                get => First;
                
                set
                {
                    if (First == null || First == Next.Sprig.Head)
                        First = value;

                    else
                    {
                        var second = Next.Sprig.Head;

                        while (second.Last != First)
                            second = (BufferNode) second.Last;

                        second.Last = First = value;
                    }
                }
            }

            public Branch(SpineNode next, BufferNode firstNodeOnBranch) {
                Next = next;
                FirstNodeOnBranch = firstNodeOnBranch;
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