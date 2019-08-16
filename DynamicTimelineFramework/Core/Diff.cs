using System;
using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{  
    public class Diff
    {
        internal Dictionary<DTFObject, Position> PositionDelta { get; }
        internal ulong Date { get; }
        internal Universe Parent { get; }

        public Diff(Dictionary<DTFObject, Position> positionDelta, ulong date, Universe parent)
        {
            PositionDelta = positionDelta;
            Date = date;
            Parent = parent;
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

            //Unlikely, bt might be able to rule it out before the real work needs to happen
            if (PositionDelta == other.PositionDelta) 
                return true;
                
            //Check if they have the same number of deltas
            if (PositionDelta.Count != other.PositionDelta.Count)
                return false;

            //Check if they are equal
            foreach (var pair in PositionDelta)
            {
                if (!other.PositionDelta.ContainsKey(pair.Key) ||
                    !other.PositionDelta[pair.Key].Equals(pair.Value))
                    return false;
            }

            return true;

        }

        public override int GetHashCode()
        {
            var deltaHash = int.MaxValue;

            foreach (var pair in PositionDelta)
            {
                deltaHash ^= pair.Value.GetHashCode();
            }

            return (Date.GetHashCode() * 397) ^ deltaHash;
        }
    }
}