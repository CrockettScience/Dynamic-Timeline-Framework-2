using System.Collections.Generic;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    /// <summary>
    /// Represents a single, non-branching timeline in the multiverse. Universes are constructed with Diff objects;
    /// objects that encode changes made to a parent universe's timeline that are incompatible with the universe's state
    /// </summary>
    public class Universe
    {
        internal readonly Diff Diff;
        internal Multiverse Owner { get; }
        internal Sprig Sprig { get; set; }
        
        private Queue<EnqueuedConstraintTasks> _constraintTasks = new Queue<EnqueuedConstraintTasks>();

        /// <summary>
        /// The other universe that this universe's timeline branches off from. This may or may NOT be the universe
        /// that is responsible for the original creation of the diff object. This depends on whether or not the
        /// diff was created before or after the date that the parent branched off from it's parent timeline.
        /// </summary>
        public Universe Parent => Diff.Parent;

        /// <summary>
        /// Constructs a Universe based on the changes encoded in the Diff
        /// </summary>
        /// <param name="diff">A diff object that encodes what changes have been made, when, and from what parent
        /// universe</param>
        /// <exception cref="DiffExpiredException">If the diff has expired</exception>
        public Universe(Diff diff)
        {
            if(diff.IsExpired)
                throw new DiffExpiredException();
            
            Diff = diff;
            Owner = diff.Parent.Owner;
            Sprig = Owner.SprigManager.BuildSprig(diff);
            Owner.AddUniverse(this);

            Owner.ClearPendingDiffs();
        }
        internal Universe(Multiverse multiverse)
        {
            Diff = new Diff(0, null, new BufferVector(0));
            Owner = multiverse;
        }
        
        /// <summary>
        /// Acquires a continuity structure representing the single timeline of a particular object in this universe.
        /// </summary>
        /// <param name="dtfObject">The object to get the continuity of.</param>
        /// <returns>The Continuity</returns>
        public Continuity GetContinuity (DTFObject dtfObject)
        {
            return new Continuity(this, dtfObject);
        }

        internal void EnqueueConstraintTask(DTFObject source, DTFObject dest, Multiverse.ObjectCompiler.TypeMetaData meta)
        {
            _constraintTasks.Enqueue(new EnqueuedConstraintTasks(source, dest, meta, this));
        }

        internal void ClearConstraintTasks()
        {
            while(_constraintTasks.Count > 0)
                _constraintTasks.Dequeue().Execute();
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

        private struct EnqueuedConstraintTasks
        {
            private DTFObject _source;
            private DTFObject _dest;
            private Multiverse.ObjectCompiler.TypeMetaData _meta;
            private Universe _universe;

            public EnqueuedConstraintTasks(DTFObject source, DTFObject dest, Multiverse.ObjectCompiler.TypeMetaData meta, Universe universe)
            {
                _source = source;
                _dest = dest;
                _meta = meta;
                _universe = universe;
            }

            public void Execute()
            {
                //Get the key that maps to dest
                foreach (var key in _source.GetLateralKeys())
                {
                    if (_source.GetLateralObject(key) == _dest)
                    {
                        //If the constraint results in a change, validate the state of the universe's timeline and
                        //make sure a no bridging errors occured
                        if (_universe.Owner.Compiler.ConstrainLateralObject(_source, key, _meta, _universe))
                        {
                            //Push lateral constraints and validate timeline
                            _universe.Owner.Compiler.PushLateralConstraints(_dest, _universe);
                                
                            if(!_universe.Sprig.Validate())
                                throw new InvalidBridgingException();
                        }

                        break;
                    }
                }
            }
        }
    }
}