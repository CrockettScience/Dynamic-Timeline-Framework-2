using System.Collections.Generic;
using System.Linq;
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
        internal ulong Date { get; private set; }
        internal Universe Parent { get; private set; }
        
        private BufferVector _delta;

        private List<Diff> _children;
        
        public List<DTFObject> InstigatingObjects { get; }

        public bool IsExpired { get; internal set; }

        internal Diff(ulong date, Universe allegingParent, BufferVector delta, DTFObject instigatingObject)
        {
            Date = date;
            _delta = delta;
            _children = new List<Diff>();
            
            InstigatingObjects = new List<DTFObject> {instigatingObject};

            var parent = allegingParent;
            
            while (parent?.Diff.Date > Date)
                parent = parent.Parent;
            
            Parent = parent;

            if (parent == null) return;
            
            Parent.Multiverse.AddDiffPending(this);

            IsExpired = false;
        }

        internal void InstallChanges(Sprig newSprig) {
            //This command installs the changes given by the diff.
            var mvCompiler = Parent.Multiverse.Compiler;
            
            //Add to the parent's children
            Parent.Diff._children.Add(this);

            //Constrain the new sprig to establish the shared certainty signature
            var priorCertaintySignature = mvCompiler.GetTimelineVector(Date - 1, Parent.Sprig.GetBufferNode(Date - 1).SuperPosition, Parent.Multiverse.SprigManager.Registry);

            newSprig.And(priorCertaintySignature);
            
            //Constrain the new sprig to establish the changes defined by the diff
            newSprig.And(_delta, InstigatingObjects.ToArray());

            var affectedLatObjects = new HashSet<DTFObject>();
            foreach (var dtfObject in InstigatingObjects) {
                mvCompiler.PushLateralConstraints(dtfObject, newSprig, true, affectedLatObjects);
            }
            
            //Constrain the parent sprig to back-propagate the new certainty signature
            var newCertaintySignature = mvCompiler.GetTimelineVector(Date - 1, _delta[Date - 1], InstigatingObjects.Concat(affectedLatObjects));
            Parent.Sprig.And(newCertaintySignature, InstigatingObjects.Concat(affectedLatObjects).ToArray());

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

            if (!combined.Validate(Parent.Multiverse.SprigManager, InstigatingObjects.Concat(otherDiff.InstigatingObjects).ToArray()))
                return false;
            
            //Combine into this
            _delta = combined;
            InstigatingObjects.AddRange(otherDiff.InstigatingObjects);
            return true;
        }

        internal void Remove() {
            //Iterate through the children and merge this diff into them
            Parent.Diff._children.Remove(this);
            
            foreach (var child in _children) {
                child.Parent = Parent;
                Parent.Diff._children.Add(child);
                
                foreach (var instigatingObject in InstigatingObjects) {
                    child.InstigatingObjects.Add(instigatingObject);
                }

                child._delta &= _delta;
                child.Date = Date;
            }
        }
    }
}