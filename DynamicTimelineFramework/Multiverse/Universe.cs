using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Multiverse
{
    public class Universe
    {
        internal readonly Diff Diff;
        
        public Multiverse Owner { get; }

        public Universe Parent => Diff.Parent;

        internal Universe(Diff diff)
        {
            Diff = diff;
            Owner = diff.Parent.Owner;

            // Todo - force the positions given in the dictionary onto new branched sprigs

        }

        /// <summary>
        /// Only used once during initial universe creation
        /// </summary>
        internal Universe(Multiverse multiverse)
        {
            Diff = new Diff(0, null, null);
            Owner = multiverse;
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