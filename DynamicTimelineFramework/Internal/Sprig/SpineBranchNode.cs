using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineBranchNode : SpineNode
    {
        private readonly Dictionary<Diff, SpineNode> _diffMap;
        public ulong Date { get; }
        
        public SpineNode this[Diff diff] {
            get => _diffMap[diff];

            set => _diffMap[diff] = value;
        }

        public SpineBranchNode(ulong date) {
            _diffMap = new Dictionary<Diff, SpineNode>();
            Date = date;
        }

        public bool Contains(Diff diff) {
            return _diffMap.ContainsKey(diff);
        }

        public Sprig AddBranch(Diff diff) {
            var newHead = new SpineHeadNode();
            _diffMap[diff] = newHead;
            
            return newHead.Sprig;
        }

        public override void Alloc(int space, int startIndex)
        {
            var nexts = _diffMap.Values;
            
            foreach (var next in nexts)
            {
                next.Alloc(space, startIndex);
            }
        }
    }
}