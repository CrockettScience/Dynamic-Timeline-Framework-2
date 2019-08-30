using System.Collections.Generic;

namespace DynamicTimelineFramework.Core.Tools.Sprig {
    internal class SBranchNode : SpineNode
    {
        public ulong BranchIndex { get; }

        private readonly Dictionary<Diff, SpineNode> _children = new Dictionary<Diff, SpineNode>();

        public SpineNode this[Diff diff]
        {
            get { return _children[diff]; }

            set
            {
                if (_children.ContainsKey(diff))
                {
                    _children[diff].Parent = null;
                }

                _children[diff] = value;
                value.Parent = this;
            }
        }

        public SBranchNode(ulong index, Diff diff) : base(diff)
        {
            BranchIndex = index;
        }
    }
}