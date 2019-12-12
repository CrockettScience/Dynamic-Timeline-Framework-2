using System;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Sprig;
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
                //Dequeue constraint Tasks
                _universe.ClearConstraintTasks();
                
                return _universe.Sprig[date, _dtfObject];
            }
        } 

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
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
            var multiverse = _universe.Owner;
            var compiler = multiverse.Compiler;
            
            //Dequeue constraint Tasks
            _universe.ClearConstraintTasks();
            
            //Pull parent constraints
            compiler.PullParentConstraints(_dtfObject, _universe);
            
            //Create a copy of the position with the correct operative slice
            var deltaPosition = new Position(_dtfObject.GetType(), pos.ReferenceSlice)
            {
                OperativeSliceProvider = _dtfObject.OperativeSliceProvider
            };

            //Get timeline position vector
            var timelineVector = _universe.Owner.Compiler.GetTimelineVector(_dtfObject, date, deltaPosition);

            //Check if it's possible to constrain the position
            var existingVector = _universe.Sprig.ToPositionVector(_dtfObject);

            if (!((PositionNode) (existingVector & timelineVector).Head).Validate()) {
                //Not possible to constrain, check if it's transitionable

                while (existingVector.Head.Index >= date)
                    existingVector.Head = existingVector.Head.Last;

                existingVector.Head = new PositionNode(existingVector.Head, date, Position.Alloc(_dtfObject.GetType(), true));

                if (!((PositionNode) (existingVector & timelineVector).Head).Validate())

                    //Not transitionable
                    throw new UnresolvableParadoxException();

                outDiff = new Diff(date, _universe, new BufferVector(_universe.Owner.SprigManager.BitCount, true) & timelineVector);
                return false;
            }

            //Constrain the position
            _universe.Sprig.And(timelineVector);
            _universe.Owner.Compiler.PushLateralConstraints(_dtfObject, _universe, true);
            
            //Clear pending diffs
            _universe.Owner.ClearPendingDiffs();
            
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
            //The only data seeding the selection is the date and a reference hashed that's serialized (coming soon)
            //with the object reference. If that's not available, iterate through the indices until one is.
            var rand = new Random(((int) date * 967) ^ _dtfObject.ReferenceHash);
            var objDef = (DTFObjectDefinitionAttribute) _dtfObject.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));

            var selectedIndex = rand.Next(objDef.PositionCount);
            
            while(!this[date][selectedIndex])
                selectedIndex = (selectedIndex + 1) % objDef.PositionCount;

            var selectedPosition = Position.Alloc(_dtfObject.GetType());
            selectedPosition[selectedIndex] = true;

            Constrain(date, selectedPosition, out _);
        }
    }
}