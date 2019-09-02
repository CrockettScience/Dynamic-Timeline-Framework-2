using System;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly Map<Diff, Sprig> _sprigs;
        
        internal Sprig GetSprig(Diff diff)
        {
            return _sprigs[diff];
        }
            
        protected DTFObject()
        {
            _sprigs = new Map<Diff, Sprig>();
        }
        
    }
}