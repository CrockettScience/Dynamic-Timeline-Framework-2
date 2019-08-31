using System;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Multiverse
{
    public class Continuity<T> where T : DTFObject
    {
        private readonly Universe _universe;
        private readonly T _dtfObject;
        private readonly Sprig<T> _sprig;

        public Position<T> this[ulong date] => _universe.Owner.Compiler.GetPosition(_dtfObject, date);

        /// <summary>
        /// Attempts to Collapse the position at date to pos. If the collapse results in a paradox
        /// (that is, the resultant position's Uncertainty property returns -1), It will create and
        /// assign a Diff object to outDiff that can be used to instantiate a universe branch where
        /// the object's position at date is collapsed to Pos.
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="pos">The position to collapse to</param>
        /// <param name="outDiff">The resultant diff if the attempted collapse should return a paradox</param>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public bool Collapse(ulong date, Position<T> pos, out Diff outDiff) {
            var newPosition = this[date] & pos;
            var sprigVector = _universe.Owner.Compiler.GetNormalizedVector(pos, date);

            if (newPosition.Uncertainty < 0) {
                //Paradox has occured
                
                var diffVector = new Map<DTFObject, SprigVector<T>>
                {
                    [_dtfObject] = sprigVector
                };
                
                outDiff = new Diff(date, _universe, diffVector);
                return false;
            }

            _sprig.And(sprigVector);
            outDiff = null;

            return true;
        }

        /// <summary>
        /// Collapses the position for the date to a random available set bit
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="random">Random object to seed pick</param>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public void Collapse(ulong date, Random random) {
            //Todo - Collapse the position for the date to a random available set bit
        }

        internal Continuity(Universe universe, T dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
            _sprig = dtfObject.GetSprig<T>(universe.Diff);
        }
    }
}