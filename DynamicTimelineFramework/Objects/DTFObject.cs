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

        internal readonly DTFOOperativeSliceProvider OperativeSliceProvider;

        internal OperativeSlice SprigBuilderSlice { get; }
        private Multiverse.ObjectCompiler Compiler { get; }

        protected DTFObject(Multiverse owner) {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Dictionary<string, DTFObject>();
            
            OperativeSliceProvider = new DTFOOperativeSliceProvider(this);

            //Register object with the timeline
            SprigBuilderSlice = owner.SprigBuilder.RegisterObject(this);
            Compiler = owner.Compiler;
        }

        protected void SetLateralObject(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");
            
            _lateralDirectory[key] = obj;
            _lateralKeys.Add(key);
            
            //Subscribe to the other object's directory and add a proxy
            var backKey = key + "-BACK_REFERENCE" + GetHashCode();
            obj._lateralDirectory[backKey] = this;
            obj._lateralKeys.Add(backKey);
            Compiler.AddLateralProxy(obj.GetType(), GetType(), backKey, key);
            
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