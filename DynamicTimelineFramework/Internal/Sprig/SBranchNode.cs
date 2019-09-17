using System.Collections.Generic;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SBranchNode : SpineNode
    {
        public ulong BranchIndex { get; }
        
        //Todo - There needs to be some sort of maintenence procedure that updates the crumple lengths as certainty increases
        public ulong[] CrumpleLengths { get; set; }

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

        public SBranchNode(ulong index, Diff diff, params ulong[] crumpleLengths) : base(diff) {
            BranchIndex = index;
            CrumpleLengths = crumpleLengths;
        }
    }
}