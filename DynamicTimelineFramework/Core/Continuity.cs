using System.Collections;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Continuity
    {
        private Universe _universe;
        private DTFObject _dtfObject;

        /// <summary>
        /// Get the position of the handle at a given date
        /// </summary>
        /// <param name="date"></param>
        public Position this[ulong date]
        {
            get {
                var position = _dtfObject[_universe.Diff][date];

                if (position.Uncertainty > 0) {
                    Collapse(date);
                }
                
                return _dtfObject[_universe.Diff][date];
            }
        }

        private void Collapse(ulong date) {
            throw new System.NotImplementedException();
        }

        internal Continuity(Universe universe, DTFObject dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
        }

        
        
    }
}