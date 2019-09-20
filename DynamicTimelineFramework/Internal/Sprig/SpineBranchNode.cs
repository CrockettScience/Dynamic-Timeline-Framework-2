using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SpineBranchNode : SpineNode
    {
        private Map<Diff, SpineNode> _nexts = new Map<Diff, SpineNode>();
        
        public SpineNode GetNode(Diff diff) {
            return _nexts[diff];
        }

        public bool Contains(Diff diff) {
            return _nexts.ContainsKey(diff);
        }
    }
}