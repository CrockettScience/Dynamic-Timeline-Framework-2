using System;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Multiverse
{
    public class Continuity
    {
        private readonly Universe _universe;
        private readonly DTFObject _dtfObject;

        public Position this[ulong date]
        {
            get
            {
                throw new NotImplementedException();
            }
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
            //First, pull constraints to get the correct value
            var compiler = _universe.Owner.Compiler;
            compiler.PullConstraints(_dtfObject, _universe.Diff);
            
            //Proceed with the mask
            var newPosition = this[date] & pos;
            var sprigVector = _universe.Owner.Compiler.GetNormalizedVector(_dtfObject.GetType(), pos, date);

            //Check for a paradox
            if (newPosition.Uncertainty < 0) {
                //Paradox has occured
                
                var diffVector = new Map<DTFObject, SprigVector>
                {
                    [_dtfObject] = sprigVector
                };
                
                outDiff = new Diff(date, _universe, diffVector);
                return false;
            }

            //Todo - Use the spine structure to correctly crumple the sprigVector
            //_sprig.And(sprigVector);
            
            //Push the constraints to the keyed objects
            compiler.PushConstraints(_dtfObject, _universe.Diff);
            
            outDiff = null;

            return true;
        }

        public Position GetSuperposition(ulong date) {
            //Pull proper constraints
            var objectCompiler = _universe.Owner.Compiler;
                
            objectCompiler.PullConstraints(_dtfObject, _universe.Diff);
                
            //Todo - Now we can return the correct position
            throw new NotImplementedException();
            //return _dtfObject.GetSprig(_universe.Diff)[date];
        }

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
            
            throw new NotImplementedException();
            
            //_sprig = dtfObject.GetSprig(universe.Diff);
        }
    }
}