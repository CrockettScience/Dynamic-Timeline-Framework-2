using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly Map<string, DTFObject> _lateralDirectory;
        private readonly List<string> _lateralKeys;
        private string _parentKey;
            
        protected DTFObject() {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Map<string, DTFObject>();
        }

        protected void AddObject(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");
            
            _lateralDirectory[key] = obj;
            _lateralKeys.Add(key);
            
            //Subscribe to the other objects directory
            obj._lateralDirectory[key + "-BACK_REFERENCE-" + GetType().GetHashCode()] = this;
            obj._lateralKeys.Add(key + "-BACK_REFERENCE-" + GetType().GetHashCode());
        }
        
        protected void SetParent(string key, DTFObject obj) {
            if(HasParent())
                throw new DTFObjectDefinitionException(GetType() + " can only have 1 parent");
            
            _lateralDirectory[key] = obj;
            _parentKey = key;
        }
        
        internal bool HasParent() {
            return _parentKey != null;
        }

        internal DTFObject GetLateralObject(string key) {
            return _lateralDirectory[key];
        }
        
        internal string GetParentKey() {
            return _parentKey;
        }

        internal List<string> GetLateralKeys()
        {
            return _lateralKeys;
        }

        public abstract Position InitialSuperPosition();
        public abstract Position TerminalSuperPosition();
    }
}