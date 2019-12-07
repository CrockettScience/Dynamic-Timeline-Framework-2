using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{  
    public class Diff
    {
        internal ulong Date { get; }
        internal Universe Parent { get; }
        internal List<SprigPositionVector> Delta { get; }

        internal Diff(ulong date, Universe allegingParent, List<SprigPositionVector> delta)
        {
            Date = date;
            Delta = delta;

            var parent = allegingParent;
            
            while (parent?.Diff.Date <= Date)
                parent = parent.Parent;
            
            Parent = parent;
        }

        internal void InstallChanges(Sprig sprig) {
            //Todo - Called by the sprig builder after being registered. 
            //This command constrains the sprig, given by the sprig builder,
            //by the proper mask just prior to date.
            //Once on the given sprig, then on the parent universe's sprig
            
            
            
            
        }

        public LinkedList<Diff> GetDiffChain() {
            var diffChain = new LinkedList<Diff>();
            var current = this;

            while (current != null) {
                diffChain.AddFirst(current);
                current = current.Parent?.Diff;
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

            for (var i = 0; i < Delta.Count; i++)
            {
                if (!Delta[i].Equals(other.Delta[i]))
                    return false;
            }

            return true;

        }

        public override int GetHashCode()
        {

            return Date.GetHashCode() ^ (Delta.Count > 0 ? Delta[0].Head.SuperPosition.GetHashCode() * 397 : 397);
        }
    }
}