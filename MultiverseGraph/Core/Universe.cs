using System.Collections;
using System.Collections.Generic;

namespace MultiverseGraph.Core
{
    public class Universe
    {
        internal readonly Diff Diff;

        public Universe Parent => Diff.Parent;

        internal Universe(Diff diff)
        {
            diff.Universe = this;
            Diff = diff;
            
            // Now, force the positions given in the dictionary onto the new branched handles
            
        }

        /// <summary>
        /// Only used once during initial universe creation
        /// </summary>
        internal Universe()
        {
            Diff = new Diff(null, 0, null);
        }
        
        public Continuity this[Handle handle] => new Continuity(this, handle);

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