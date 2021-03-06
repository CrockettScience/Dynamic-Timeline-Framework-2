using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;

namespace DynamicTimelineFramework.Objects {
    
    /// <summary>
    /// The parent object used to define object behaviour on DynamicTimelineFramework
    /// </summary>
    public abstract class DTFObject {
        private readonly Dictionary<string, DTFObject> _lateralDirectory;
        private readonly List<string> _lateralKeys;
        private Multiverse.ObjectCompiler Compiler { get; }
        public List<Universe> RootedUniverses { get; }

        internal readonly int ReferenceHash;
        
        public string ParentKey { get; set; }
        public DTFObject Parent => ParentKey == null ? null : _lateralDirectory[ParentKey];

        protected DTFObject(Multiverse owner) {
            _lateralKeys = new List<string>();
            _lateralDirectory = new Dictionary<string, DTFObject>();
            RootedUniverses = new List<Universe>();

            //Register object with the timeline
            owner.SprigManager.RegisterObject(this, out var hash);
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

        /// <summary>
        /// Can be called during the inherited constructor to define and set a single, special lateral object that exists
        /// as a "Parent" to this object. Parent objects don't inform child objects of changes that occur on them when
        /// they happen, but instead, child objects will pull from them when needed. This makes using parents crucial to
        /// performance when dealing with objects that can end up with many, many lateral objects.
        ///
        /// ALL LATERAL OBJECTS MUST SHARE THE SAME PARENT.
        /// </summary>
        /// <param name="key">The key that identifies the set of lateral constraints defined by LateralConstraintAttributes</param>
        /// <param name="obj">The object to set</param>
        /// <exception cref="DTFObjectDefinitionException">When the key passed is taken, or when lateral objects do not share the same parent</exception>
        protected void SetParent(string key, DTFObject obj) {
            if(_lateralDirectory.ContainsKey(key))
                throw new DTFObjectDefinitionException(GetType() + " attempted to add an object to key " + key + "twice");

            //All lateral objects must share the same parent and key, except the parent itself
            foreach (var lateralObject in _lateralDirectory.Values) {
                if(lateralObject.ParentKey != null && lateralObject._lateralDirectory[lateralObject.ParentKey] != obj)
                    throw new DTFObjectDefinitionException(GetType() + " attempted to add a parent object with key " + key + ", but lateral object " + lateralObject.GetType() + " doesn't have the same parent at the key.");
            }

            _lateralDirectory[key] = obj;
            ParentKey = key;

        }

        internal DTFObject GetLateralObject(string key) {
            return _lateralDirectory[key];
        }

        internal List<string> GetLateralKeys()
        {
            return _lateralKeys;
        }
    }
}