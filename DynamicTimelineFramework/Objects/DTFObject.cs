using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Objects {
    
    /// <summary>
    /// The parent object used to define object behaviour on DynamicTimelineFramework
    /// </summary>
    public abstract class DTFObject {
        
        private readonly Dictionary<string, DTFObject> _lateralDirectory;
        private readonly List<string> _lateralKeys;

        internal readonly DTFOOperativeSliceProvider OperativeSliceProvider;

        internal readonly int ReferenceHash;

        internal OperativeSlice SprigManagerSlice { get; }
        private Multiverse.ObjectCompiler Compiler { get; }

        protected DTFObject(Multiverse owner) {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Dictionary<string, DTFObject>();
            
            OperativeSliceProvider = new DTFOOperativeSliceProvider(this);

            //Register object with the timeline
            SprigManagerSlice = owner.SprigManager.RegisterObject(this, out var hash);
            ReferenceHash = hash;
            Compiler = owner.Compiler;
        }

        /// <summary>
        /// Can be called during the inherited constructor to define and set lateral objects. Lateral objects cannot be
        /// redefined once set. Care should be taken when adding two or more lateral objects that already exist on the
        /// timeline but are not laterally constrained to one another, or they are constrained in a way that is
        /// incompatible with the constraints put forth by this object.
        /// </summary>
        /// <param name="key">The key that identifies the set of lateral constraints defined by LateralConstraintAttributes</param>
        /// <param name="obj">The object to set</param>
        /// <exception cref="DTFObjectDefinitionException">When attempting to set more than one object with the same key</exception>
        /// <exception cref="InvalidBridgingException">When the constraining of two or more objects that already exist
        /// on the timeline results in segments with no valid states</exception>
        protected void SetLateralObject(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");
            
            _lateralDirectory[key] = obj;
            _lateralKeys.Add(key);
            
            //Subscribe to the other object's directory and add a proxy
            var backKey = key + "-BACK_REFERENCE-" + ReferenceHash;


            for (var i = 1; obj._lateralDirectory.ContainsKey(backKey); i++)
                backKey = key + "-BACK_REFERENCE-" + (ReferenceHash ^ (i * 967));
            
            obj._lateralDirectory[backKey] = this;
            obj._lateralKeys.Add(backKey);
            Compiler.AddLateralProxy(obj.GetType(), GetType(), backKey, key);
            
            //Pull constraints from the object's existing timeline
            Compiler.PullLateralConstraints(obj, this);
            
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

            public OperativeSlice OperativeSlice => _proxyOwner.SprigManagerSlice;

            public DTFOOperativeSliceProvider(DTFObject proxyOwner)
            {
                _proxyOwner = proxyOwner;
            }
        }
    }
}