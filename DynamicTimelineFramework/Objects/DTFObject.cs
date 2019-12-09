using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly Dictionary<string, DTFObject> _lateralDirectory;
        private readonly List<string> _lateralKeys;

        internal DTFOOperativeSliceProvider _operativeSliceProvider;

        internal OperativeSlice SprigBuilderSlice { get; }

        protected DTFObject(Multiverse owner) {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Dictionary<string, DTFObject>();
            
            _operativeSliceProvider = new DTFOOperativeSliceProvider(this);

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

        internal class DTFOOperativeSliceProvider : IOperativeSliceProvider
        {
            private readonly DTFObject _proxyOwner;

            public OperativeSlice OperativeSlice => _proxyOwner.SprigBuilderSlice;

            public DTFOOperativeSliceProvider(DTFObject proxyOwner)
            {
                _proxyOwner = proxyOwner;
            }
        }
    }
}