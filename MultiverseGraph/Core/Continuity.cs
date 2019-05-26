using System.Collections;

namespace MultiverseGraph.Core
{
    public class Continuity
    {
        private Universe _universe;
        private Handle _handle;

        internal Continuity(Universe universe, Handle handle)
        {
            _universe = universe;
            _handle = handle;
        }

        /// <summary>
        /// Get the position of the handle at a given date
        /// </summary>
        /// <param name="date"></param>
        public Position this[ulong date]
        {
            get
            {
                //TODO
                return new Position();
            }
        }
        
    }
}