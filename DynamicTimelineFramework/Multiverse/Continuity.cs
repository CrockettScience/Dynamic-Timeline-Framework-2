using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Multiverse
{
    public class Continuity<T> where T : DTFObject
    {
        private Universe _universe;
        private T _dtfObject;
        private readonly Sprig<T> _sprig;

        public Position<T> this[ulong date] {
            get {
                //TODO - For now, just return the raw position
                //until a method is made that will check the 
                //object's parent state.
                return (Position<T>) _sprig[date];
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
        public bool Collapse(ulong date, Position<T> pos, out Diff outDiff) {
            var newPosition = this[date] & pos;

            if (newPosition.Uncertainty < 0) {
                //Paradox has occured
                var diffVector = new Map<DTFObject, SprigVector<T>>();
                //Todo - we need to be able to create a sprig vector from a position and a date.
                //Suggestion: Maybe the DTFObject compiler can pre-compute a MASTER SprigVector
                //for every position defined for every DTFObject defined and keep a database.
                //We only need to pull them, copy them, and them together and "shift" the collapsed
                //band" left or right, based on the date it needs to line up with.
                
                outDiff = new Diff(date, _universe, diffVector);
                return false;
            }
            
            //todo - furnish a more valid sprig vector, see above
            var sprigVector = new SprigVector<T>(null);

            _sprig.And(sprigVector);
            outDiff = null;

            return true;
        }

        internal Continuity(Universe universe, T dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
            _sprig = dtfObject.GetSprig<T>(universe.Diff);
        }
    }
}