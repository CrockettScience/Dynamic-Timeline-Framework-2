using System.Collections.Generic;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{  
    /// <summary>
    /// A Diff encodes the specific changes that are made to the timeline. Diffs are created when attempting to
    /// constrain the timeline to a state that isn't available, and can can be passed into the constructor of a
    /// universe. Once a Universe is constructed, Diffs are used as a key to access the universe from the
    /// Multiverse's indexer. It is recommended that Diffs are maintained or saved after constructing a universe. If
    /// allowed to go out of scope after Universe construction, the Universe will become inaccessible but remain as a
    /// permanent part of the Multiverse "tree"
    ///
    /// The creation and existence of a Diff has no effect on the timeline until it's passed into a Universe's
    /// constructor
    /// </summary>
    public class Diff
    {
        internal ulong Date { get; }
        internal Universe Parent { get; }
        
        private BufferVector _delta;
        
        public List<DTFObject> InstigatingObjects { get; }

        public bool IsExpired { get; internal set; }

        internal Diff(ulong date, Universe allegingParent, BufferVector delta, DTFObject instigatingObject)
        {
            Date = date;
            _delta = delta;
            
            InstigatingObjects = new List<DTFObject> {instigatingObject};

            var parent = allegingParent;
            
            while (parent?.Diff.Date > Date)
                parent = parent.Parent;
            
            Parent = parent;

            if (parent == null) return;
            
            Parent.Owner.AddDiffPending(this);

            IsExpired = false;
        }

        internal void InstallChanges(Sprig newSprig) {
            //This command installs the changes given by the diff.
            var mvCompiler = Parent.Owner.Compiler;

            //Constrain the new sprig to establish the shared certainty signature
            var priorCertaintySignature = mvCompiler.GetTimelineVector(Date - 1, Parent.Sprig.GetBufferNode(Date - 1).SuperPosition, Parent.Owner.SprigManager.Registry);

            newSprig.And(priorCertaintySignature);
            
            //Constrain the new sprig to establish the changes defined by the diff
            newSprig.And(_delta, InstigatingObjects.ToArray());
            
            //Constrain the parent sprig to back-propagate the new certainty signature
            var newCertaintySignature = mvCompiler.GetTimelineVector(Date - 1, _delta[Date - 1], InstigatingObjects);
            Parent.Sprig.And(newCertaintySignature, InstigatingObjects.ToArray());

        }

        /// <summary>
        /// Gets the chain of Diffs that, together, encode all the changes made since the root
        /// universe
        /// </summary>
        /// <returns>The chain of Diffs that, together, encode all the changes made since the root universe</returns>
        public LinkedList<Diff> GetDiffChain() {
            var diffChain = new LinkedList<Diff>();
            var current = this;

            while (current != null) {
                diffChain.AddFirst(current);
                current = current.Parent?.Diff;
            }

            return diffChain;
        }

        /// <summary>
        /// Attempt to combine the delta from the given diff into the calling diff. If successful, otherDiff will be
        /// rendered expired and you can use the calling diff to manifest it's changes
        /// </summary>
        /// <param name="otherDiff">The diff to combine. Diff will become expired if successful</param>
        /// <exception cref="DiffExpiredException">If either diff has expired</exception>
        /// <returns>True if the diffs are compatible and have been combined into the calling diff, false if not</returns>
        public bool Combine(Diff otherDiff)
        {
            if (IsExpired || otherDiff.IsExpired)
                throw new DiffExpiredException();
            
            //Check trivial disqualifications
            if (Date != otherDiff.Date || Parent != otherDiff.Parent)
                return false;

            //Combine the delta's and validate
            var combined = _delta & otherDiff._delta;

            if (!combined.Validate(Parent.Owner.SprigManager))
                return false;
            
            //Combine into this
            _delta = combined;
            InstigatingObjects.AddRange(otherDiff.InstigatingObjects);
            return true;
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