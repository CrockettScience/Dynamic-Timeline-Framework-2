using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{  
    public class Diff
    {
        internal ulong Date { get; }
        internal Universe Parent { get; }
        private readonly SprigBufferVector _delta;

        internal Diff(ulong date, Universe allegingParent, SprigBufferVector delta)
        {
            Date = date;
            _delta = delta;

            var parent = allegingParent;
            
            while (parent?.Diff.Date <= Date)
                parent = parent.Parent;
            
            Parent = parent;
        }

        internal void InstallChanges(Sprig newSprig) {
            //This command installs the changes given by the diff.
            var mvCompiler = Parent.Owner.Compiler;
            var sprigBuilder = Parent.Owner.SprigBuilder;

            //Constrain the new sprig to establish the shared certainty signature
            var priorCertaintySignature = mvCompiler.GetTimelineVector(sprigBuilder, Date - 1,
                Parent.Sprig.GetBufferNode(Date - 1).SuperPosition);

            newSprig.And(priorCertaintySignature);
            
            //Constrain the new sprig to establish the changes defined by the diff
            newSprig.And(_delta);
            
            //Constrain the parent sprig to back-propagate the new certainty signature
            var newCertaintySignature = mvCompiler.GetTimelineVector(sprigBuilder, Date - 1, _delta[Date - 1]);
            Parent.Sprig.And(newCertaintySignature);

        }

        public LinkedList<Diff> GetDiffChain() {
            var diffChain = new LinkedList<Diff>();
            var current = this;

            while (current != null) {
                diffChain.AddFirst(current);
                current = current.Parent.Diff;
            }

            return diffChain;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Diff other)) return false;
            
            //Two diffs are equal if:
            //The same positions have the same deltas
            //The deltas happened on the same date
            //The deltas happened in the same universe (parent)
                
            if (Date != other.Date)
                return false;
                
            //We check the parent universe by reference as well; The Multiverse creates the universes by Diff definition
            if (Parent != other.Parent)
                return false;

            return _delta.Equals(other._delta);

        }

        public override int GetHashCode()
        {

            return Date.GetHashCode() ^ _delta.Head.SuperPosition.GetHashCode() * 397;
        }
    }
}