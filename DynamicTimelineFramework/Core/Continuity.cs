using System;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Continuity
    {
        private readonly Universe _universe;
        private readonly DTFObject _dtfObject;

        public Position this[ulong date] => _universe.Sprig.GetPosition(date, _dtfObject);

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
        }

        /// <summary>
        /// Attempts to Constrain the position at date to pos. If the constraint results in a paradox
        /// (that is, the resultant position's Uncertainty property returns -1), but is transitionable
        /// (the position at date - 1 has at least 1 eigenvalue that defines pos as an eigenvalue or
        /// set of eigenvalues in it's forward transition), It will create and assign a Diff object
        /// to outDiff that can be used to instantiate a universe branch where the object's position
        /// at date is collapsed to Pos.
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="pos">The position to collapse to</param>
        /// <param name="outDiff">The resultant diff if the attempted collapse should return a paradox</param>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public bool Constrain(ulong date, Position pos, out Diff outDiff) 
        {
            throw new NotImplementedException();
        }
    }
}