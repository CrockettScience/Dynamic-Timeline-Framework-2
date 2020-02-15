using System;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    /// <summary>
    /// A Continuity represents a single timeline for an object for a specific Universe. This class is the primary way
    /// to interact with the timeline
    /// </summary>
    public class Continuity
    {
        private readonly Universe _universe;
        private readonly DTFObject _dtfObject;
        private readonly DTFObjectDefinitionAttribute _attr;

        /// <summary>
        /// Gets the current read-only position at date. This position MAY be in SuperPosition
        /// (have multiple positions encoded inside, also know as the position's eigenvalues). Use
        /// Position.GetEigenstates() to break the superposition up into it's singular components
        /// </summary>
        /// <param name="date">The date to get the position of</param>
        public Position this[ulong date]
        {
            get
            {
                if(_universe.IsDisposed)
                    throw new ObjectDisposedException("Universe");
                
                //Dequeue constraint Tasks
                _universe.ClearConstraintTasks();
                
                //Pull parent constraints.
                _universe.Multiverse.Compiler.PullParentInformation(_dtfObject, _universe);
                
                return _universe.Sprig[date, _dtfObject];
            }
        } 

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
            _attr = (DTFObjectDefinitionAttribute) _dtfObject.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));

            //Ask sprig if object is rooted to trigger rooting if needed
            while (!_universe.Sprig.CheckRooted(_dtfObject))
            {
                _universe = _universe.Parent;
            }
        }

        /// <summary>
        /// Attempts to Constrain the position at date to pos. If the constraint results in a
        /// paradox (that is, the resultant position's Uncertainty property is less than pos), but
        /// is transitionable (Every eigenstate in pos is in the set of eigenstates in the forward
        /// transition of the superposition at date - 1, and the lengths of the eigenstates permit
        /// the states to end there as well), It will create and assign a Diff object to outDiff
        /// that can be used to instantiate a universe branch where the object's position at date
        /// is collapsed to Pos.
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="pos">The positions to collapse to</param>
        /// <param name="outDiff">The resultant diff if the attempted collapse should return a paradox</param>
        /// <exception cref="UnresolvableParadoxException">If the given position results in a paradox that cannot be transitioned to</exception>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public bool Constrain(ulong date, Position pos, out Diff outDiff) {
            if(_universe.IsDisposed)
                throw new ObjectDisposedException("Universe");
            
            //Check if the position will result in net change
            var currentPos = _universe.Sprig[date, _dtfObject];
            if ((currentPos & pos).Equals(currentPos)) {
                //Constraining this position will not result in net change, no work needed
                outDiff = null;
                return true;
            }

            var multiverse = _universe.Multiverse;
            var compiler = multiverse.Compiler;
            
            //Pull parent constraints.
            compiler.PullParentInformation(_dtfObject, _universe);

            //Get timeline position vector
            var timelineVector = _universe.Multiverse.Compiler.GetTimelineVector(_dtfObject, date, pos);

            //Check if it's possible to constrain the position
            if ((_universe.Sprig[date, _dtfObject] & pos).Uncertainty == -1) {
                //Not possible to constrain, check if it's transitionable

                var existingVector = _universe.Sprig.ToPositionVector(_dtfObject);

                while (existingVector.Head.Index >= date)
                    existingVector.Head = (PositionNode) existingVector.Head.Last;

                existingVector.Head = new PositionNode(existingVector.Head, date, Position.Alloc(_dtfObject.GetType(), true));

                if (!(existingVector & timelineVector).Validate())

                    //Not transitionable
                    throw new UnresolvableParadoxException();

                outDiff = new Diff(date, _universe, pos, _dtfObject);
                return false;
            }

            //Constrain the position
            _universe.Sprig.And(timelineVector, _dtfObject);
            _universe.Multiverse.Compiler.PushLateralConstraints(_dtfObject, _universe.Sprig, true);
            
            //Clear pending diffs
            _universe.Multiverse.ClearPendingDiffs();
            
            outDiff = null;
            return true;
        }

        /// <summary>
        /// Select a random, available eigenstate to constrain the superposition at date to.
        ///
        /// Select() will "try" to select the same states for the same objects for the same dates, across all universes.
        /// As a result, differences in the results of this selection across separate universes are a direct result of
        /// the change in the timeline that defines those universes and make those states impossible for this specific
        /// universe, even if the object itself isn't directly responsible for the construction of the diff. 
        /// </summary>
        /// <param name="date">The date to select the eigenstate at.</param>
        public void Select(ulong date)
        {
            if(_universe.IsDisposed)
                throw new ObjectDisposedException("Universe");
            
            var selectedIndex = (_dtfObject.ReferenceHash ^ (int) date) % _attr.PositionCount;
            
            while(!this[date][selectedIndex])
                selectedIndex = (selectedIndex + 1) % _attr.PositionCount;

            var selectedPosition = Position.Alloc(_dtfObject.GetType());
            selectedPosition[selectedIndex] = true;

            Constrain(date, selectedPosition, out _);
        }
    }
}