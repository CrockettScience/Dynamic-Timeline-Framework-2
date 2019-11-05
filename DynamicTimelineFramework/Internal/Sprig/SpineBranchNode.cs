using System;
using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineBranchNode : SpineNode, IEnumerable<Diff>
    {
        private readonly Dictionary<Diff, Branch> _branchMap;
        public ulong Date { get; }
        public override Sprig RootSprig { get; }
        
        public SpineNode this[Diff diff] {
            get => _branchMap[diff].SNode;

            set => _branchMap[diff].SNode = value;
        }

        public SpineBranchNode(ulong date, Sprig rootSprig) {
            _branchMap = new Dictionary<Diff, Branch>();
            Date = date;
            RootSprig = rootSprig;
        }

        public bool Contains(Diff diff) {
            return _branchMap.ContainsKey(diff);
        }

        public Sprig AddBranch(Diff diff) {
            var newHead = new SpineHeadNode(RootSprig, Date, diff);
            
            _branchMap[diff] = new Branch(newHead, newHead.RootSprig.Head);
            return newHead.RootSprig;
        }

        public override void Alloc(int space, int startIndex)
        {
            var nexts = _branchMap.Values;
            
            foreach (var next in nexts)
            {
                next.SNode.Alloc(space, startIndex);
            }
        }

        public Branch GetBranch(Diff diff) {
            return _branchMap[diff];
        }

        public class Branch {
            public SpineNode SNode { get; set; }
            public BufferNode BNode { get; set; }
            
            public Branch(SpineNode sNode, BufferNode bNode) {
                SNode = sNode;
                BNode = bNode;
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