using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineBranchNode : SpineNode, IEnumerable<Diff>
    {
        private readonly Dictionary<Diff, Branch> _branchMap;
        public ulong Date { get; }
        public override Sprig ParentSprig { get; }
        
        public SpineNode this[Diff diff] {
            get => _branchMap[diff].Next;

            set => _branchMap[diff].Next = value;
        }

        public SpineBranchNode(ulong date, Sprig parentSprig) {
            _branchMap = new Dictionary<Diff, Branch>();
            Date = date;
            ParentSprig = parentSprig;
        }

        public bool Contains(Diff diff) {
            return _branchMap.ContainsKey(diff);
        }

        public Sprig AddBranch(Diff diff) {
            var newHead = new SpineHeadNode(ParentSprig, Date, diff);
            
            _branchMap[diff] = new Branch(newHead, newHead.ParentSprig.Head);
            return newHead.ParentSprig;
        }
        
        public void AddBranch(SpineNode next) {
            var diff = next.ParentSprig.Diff;
            _branchMap[diff] = new Branch(next, next.ParentSprig.GetBufferNode(Date));
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

        public class Branch {
            public SpineNode Next { get; set; }
            public BufferNode FirstNodeOnBranch { get; set; }
            
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