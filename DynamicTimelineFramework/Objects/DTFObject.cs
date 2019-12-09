using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly Dictionary<string, DTFObject> _lateralDirectory;
        private readonly List<string> _lateralKeys;

        internal OperativeSlice SprigBuilderSlice { get; }

        protected DTFObject(Multiverse owner) {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Dictionary<string, DTFObject>();

            //Register object with the timeline
            SprigBuilderSlice = owner.SprigBuilder.RegisterObject(this);
        }

        protected void SetLateralObject(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");
            
            _lateralDirectory[key] = obj;
            _lateralKeys.Add(key);
            
            //Subscribe to the other objects directory
            obj._lateralDirectory[key + "-BACK_REFERENCE-" + GetHashCode()] = this;
            obj._lateralKeys.Add(key + "-BACK_REFERENCE-" + GetHashCode());
        }

        internal DTFObject GetLateralObject(string key) {
            return _lateralDirectory[key];
        }

        internal List<string> GetLateralKeys()
        {
            return _lateralKeys;
        }

        public abstract Position InitialSuperPosition();
        public abstract Position TerminalSuperPosition();
    }
}