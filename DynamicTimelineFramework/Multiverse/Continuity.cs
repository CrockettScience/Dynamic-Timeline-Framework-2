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

        public Position<T> this[ulong date]
        {
            get
            {
                //Pull proper constraints
                var objectCompiler = _universe.Owner.Compiler;
                
                objectCompiler.PullConstraints(_dtfObject, _universe.Diff);
                
                //Now we can return the correct position
                return (Position<T>) _dtfObject.GetSprig<T>(_universe.Diff)[date];
            }
        }

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
        public bool Collapse(ulong date, Position<T> pos, out Diff outDiff) 
        {
            //First, pull constraints to get the correct value
            var compiler = _universe.Owner.Compiler;
            compiler.PullConstraints(_dtfObject, _universe.Diff);
            
            //Proceed with the mask
            var newPosition = this[date] & pos;
            var sprigVector = _universe.Owner.Compiler.GetNormalizedVector(pos, date);

            //Check for a paradox
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
            
            //Push the constraints to the keyed objects
            compiler.PushConstraints(_dtfObject, _universe.Diff);
            
            outDiff = null;

            return true;
        }

        /// <summary>
        /// Collapses the position for the date to a random available eigen value.
        /// </summary>
        /// <param name="date">The date to collapse to the position at</param>
        /// <param name="random">Random object to seed pick</param>
        /// <returns>True if the collapse was successful; false if the resultant position was a paradox</returns>
        public void Collapse(ulong date, Random random)
        {
            //First, pull constraints to get the correct value
            var compiler = _universe.Owner.Compiler;
            compiler.PullConstraints(_dtfObject, _universe.Diff);
            
            //Proceed with the eigenvalue selection
            var position = this[date];
            var eigenValues = position.GetEigenValues();

            //The out diff will never be used in this call because we picked the state from an available eigen value
            Collapse(date, eigenValues[random.Next(eigenValues.Count)], out _);
        }

        internal Continuity(Universe universe, T dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
            _sprig = dtfObject.GetSprig<T>(universe.Diff);
        }
    }
}