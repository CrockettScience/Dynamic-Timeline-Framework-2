using System.Collections.Generic;
using System.Linq;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
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
        
        private SprigVector _delta;

        private readonly List<Diff> _children;
        
        public HashSet<DTFObject> AffectedObjects { get; }

        public bool IsExpired { get; internal set; }

        internal Diff(ulong date, Universe allegingParent, Position newPosition, DTFObject changedObject)
        {
            Date = date;
            _children = new List<Diff>();
            _delta = new SprigVector();

            var parent = allegingParent;
            
            while (parent?.Diff.Date > Date)
                parent = parent.Parent;
            
            Parent = parent;

            if (parent == null) return;
            
            Parent.Multiverse.AddDiffPending(this);

            IsExpired = false;

            var timelineVector = Parent.Multiverse.Compiler.GetTimelineVector(changedObject, date, newPosition);
            
            //Identify all affected objects and compute total delta
            AffectedObjects = GetAllAffectedLateralObjectsInNetwork(changedObject, timelineVector);
        }
        
        internal Diff()
        {
            Date = 0;
            _children = new List<Diff>();
            Parent = null;
            _delta = new SprigVector();
        }

        internal void AddToDelta(DTFObject obj, PositionVector deltaVector) {
            _delta.Add(obj, deltaVector);
        }

        internal void InstallChanges(Sprig newSprig) {
            //Add to the parent's children
            Parent.Diff._children.Add(this);
            
            //Constrain the new sprig to establish the changes defined by the diff
            newSprig.And(_delta);

            if (Parent.Multiverse.ValidateOperations)
                if(!newSprig.Validate() || !Parent.Sprig.Validate())
                    throw new InvalidBridgingException();
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

            if (!combined.Validate())
                return false;
            
            //Combine into this
            _delta = combined;
            AffectedObjects.UnionWith(otherDiff.AffectedObjects);
            return true;
        }

        internal void Remove() {
            //Iterate through the children and merge this diff into them
            Parent.Diff._children.Remove(this);
            
            foreach (var child in _children) {
                child.Parent = Parent;
                Parent.Diff._children.Add(child);
                
                foreach (var instigatingObject in AffectedObjects) {
                    child.AffectedObjects.Add(instigatingObject);
                }

                child._delta &= _delta;
                child.Date = Date;
            }
        }
        
        private HashSet<DTFObject> GetAllAffectedLateralObjectsInNetwork(DTFObject obj, PositionVector timelineVector) {
            var lats = new HashSet<DTFObject> {obj};
            _delta.Add(obj, timelineVector);

            //Add all in current network
            AddAffectedLateralObjects(obj, timelineVector, lats);
            
            //Add if needed from parent network
            if (obj.Parent != null && Parent.Multiverse.Compiler.CanConstrainLateralObject(obj, obj.Parent, obj.ParentKey, timelineVector, Parent.Sprig, out var result)) {
                lats.Add(obj.Parent);
                _delta.Add(obj.Parent, result);
                GetAllAffectedLateralObjectsInNetwork(obj.Parent, result);
            }

            return lats;
        }

        internal void AddAffectedLateralObjects(DTFObject obj, PositionVector timelineVector, HashSet<DTFObject> lats) {
            foreach (var key in obj.GetLateralKeys()) {
                var lat = obj.GetLateralObject(key);

                //Get lateral translation and see if it's compatible with the current timeline
                if (!lats.Contains(lat) && !Parent.Multiverse.Compiler.CanConstrainLateralObject(obj, lat, key, timelineVector, Parent.Sprig, out var result)) {
                        
                    lats.Add(lat);
                    _delta.Add(lat, result);
                    AddAffectedLateralObjects(lat, result, lats);
                }
            }
        }
    }
}