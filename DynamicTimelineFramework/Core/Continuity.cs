using System;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Continuity
    {
        private readonly Universe _universe;
        private readonly DTFObject _dtfObject;

        public Position this[ulong date] => _universe.Sprig[date, _dtfObject];

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
        }

        /// <summary>
        /// Attempts to Constrain the position at date to pos. If the constraint results in a paradox
        /// (that is, the resultant position's Uncertainty property is less than pos), but is transitionable
        /// (Every eigenvalue in pos is in the set of eigenvalues in the forward transition of the superposition
        /// at date - 1, and the required previous states end there as well), It will create and assign a Diff
        /// object to outDiff that can be used to instantiate a universe branch where the object's position at date is
        /// collapsed to Pos.
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="pos">The positions to collapse to</param>
        /// <param name="outDiff">The resultant diff if the attempted collapse should return a paradox</param>
        /// <exception cref="UnresolvableParadoxException">If the given position results in a paradox that cannot be transitioned to</exception>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public bool Constrain(ulong date, Position pos, out Diff outDiff)
        {
            //Create a copy of the position with the correct operative slice
            var deltaPosition = new Position(_dtfObject.GetType(), pos.ReferenceSlice)
            {
                OperativeSliceProvider = _dtfObject.OperativeSliceProvider
            };

            //Get timeline position vector
            var timelineVector = _universe.Owner.Compiler.GetTimelineVector(_dtfObject, date, deltaPosition);

            //Check if it's possible to constrain the position
            if ((this[date] & deltaPosition).Uncertainty < deltaPosition.Uncertainty)
            {
                //Not possible to constrain, check if it's transitionable
                if(date != 0 && (this[date - 1] & timelineVector[date - 1]).Uncertainty < timelineVector[date - 1].Uncertainty)
                {
                    //Not transitionable
                    throw new UnresolvableParadoxException();
                }
                
                //Validate vector to verify the lengths can line up
                var existingVector = _universe.Sprig.ToPositionVector(_dtfObject);

                while (existingVector.Head.Index >= date)
                    existingVector.Head = existingVector.Head.Last;
                    
                existingVector.Head = new PositionNode(existingVector.Head, date, Position.Alloc(_dtfObject.GetType(), true));

                if (!((PositionNode) (existingVector & timelineVector).Head).Validate())
                    //Not transitionable
                    throw new UnresolvableParadoxException();
                
                outDiff = new Diff(date, _universe, new SprigBufferVector(_universe.Owner.SprigBuilder.IndexedSpace, true) & timelineVector);
                return false;
            }
            
            //Constrain the position
            _universe.Sprig.And(timelineVector);
            _universe.Owner.Compiler.PushLateralConstraints(_dtfObject, _universe);
            
            outDiff = null;
            return true;
        }
    }
}