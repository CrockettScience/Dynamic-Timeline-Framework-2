using System;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly Map<Diff, Sprig> _sprigs;
        private readonly Map<string, DTFObject> _lateralDirectory;
        private DTFObject _parent;
        
        internal Sprig GetSprig(Diff diff)
        {
            return _sprigs[diff];
        }
            
        protected DTFObject() {
            _lateralDirectory = new Map<string, DTFObject>();
            _sprigs = new Map<Diff, Sprig>();
            _parent = null;
        }

        protected void AddObject(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");
            
            _lateralDirectory[key] = obj;
            
            //Subscribe to the other objects directory
            obj._lateralDirectory[key + ToString()] = this;
        }
        
        protected void AddParent(DTFObject obj) {
            if(_parent != null)
                throw new DTFObjectDefinitionException(GetType() + " can only have 1 parent");
            
            _parent = obj;
        }

        internal DTFObject GetLateralObject(string key) {
            return _lateralDirectory[key];
        }
        
        internal DTFObject GetParent() {
            return _parent;
        }
    }
}