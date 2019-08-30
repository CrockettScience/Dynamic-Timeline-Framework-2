using System.Collections;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Continuity<T> where T : DTFObject
    {
        private Universe _universe;
        private T _dtfObject;

        private void Collapse(ulong date) {
            throw new System.NotImplementedException();
        }

        internal Continuity(Universe universe, T dtfObject)
        {
            _universe = universe;
            _dtfObject = dtfObject;
        }

        
        
    }
}