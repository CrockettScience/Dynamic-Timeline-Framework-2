using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineBranchNode : SpineNode
    {
        private Dictionary<Diff, SpineNode> _diffMap = new Dictionary<Diff, SpineNode>();
        
        public SpineNode GetNode(Diff diff) {
            return _diffMap[diff];
        }

        public bool Contains(Diff diff) {
            return _diffMap.ContainsKey(diff);
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