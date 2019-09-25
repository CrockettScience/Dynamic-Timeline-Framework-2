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

        internal Diff(ulong date, Universe parent, List<SprigPositionVector> delta)
        {
            Date = date;
            Parent = parent;
            Delta = delta;
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

            return (Date.GetHashCode() ^ Delta[0].Head.SuperPosition.GetHashCode() * 397);
        }
    }
}