using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Multiverse
{  
    public class Diff
    {
        internal ulong Date { get; }
        internal Universe Parent { get; }
        internal  Map<DTFObject, SprigVector> SprigVectors { get; }

        internal Diff(ulong date, Universe parent, Map<DTFObject, SprigVector> sprigVectors)
        {
            Date = date;
            Parent = parent;
            SprigVectors = sprigVectors;
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

            return true;

        }

        public override int GetHashCode()
        {
            var deltaHash = int.MaxValue;

            return (Date.GetHashCode() * 397) ^ deltaHash;
        }
    }
}