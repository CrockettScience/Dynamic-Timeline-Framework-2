using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Universe
    {
        internal readonly Diff Diff;
        public Multiverse Owner { get; }
        public Universe Parent => Diff.Parent;
        internal Sprig Sprig { get; set; }

        public Universe(Diff diff)
        {
            Diff = diff;
            Owner = diff.Parent.Owner;
            Sprig = Owner.SprigBuilder.BuildSprig(diff);
        }

        /// <summary>
        /// Only used once during initial universe creation
        /// </summary>
        internal Universe(Multiverse multiverse)
        {
            Diff = new Diff(0, null, new SprigBufferVector(0));
            Owner = multiverse;
        }
        
        public Continuity GetContinuity (DTFObject dtfObject)
        {
            return new Continuity(this, dtfObject);
        }

        public override bool Equals(object obj)
        {
            if (obj is Universe other)
            {
                return Diff.Equals(other.Diff);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Diff.GetHashCode();
        }
    }
}