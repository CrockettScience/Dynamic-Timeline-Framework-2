using System.Collections;
using System.Collections.Generic;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Universe
    {
        internal readonly Diff Diff;

        public Universe Parent
        {
            get { return Diff.Parent; }
        }

        internal Universe(Diff diff)
        {
            Diff = diff;
            
            // Now, force the positions given in the dictionary onto the new branched handles
            
        }

        /// <summary>
        /// Only used once during initial universe creation
        /// </summary>
        internal Universe()
        {
            Diff = new Diff(0, null);
        }
        
        public Continuity<T> GetContinuity<T> (T dtfObject) where T : DTFObject
        {
            return new Continuity<T>(this, dtfObject);
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