using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Multiverse;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        
        private readonly IMap<Diff, ISprig<DTFObject>> _sprigs;
        
        internal Sprig<T> GetSprig<T>(Diff diff) where T : DTFObject
        {
            return (Sprig<T>) _sprigs[diff];
        }
            
        protected DTFObject()
        {
            //We need to make sure we instantiate a sprig structure with the correct derived type parameter
            var mapType = typeof(Map< , >);
            var sprigType = typeof(Sprig< >);
            
            var genericSprigType = sprigType.MakeGenericType(GetType());
            var genericMapType = mapType.MakeGenericType(typeof(Diff), genericSprigType);

            _sprigs = (IMap<Diff, ISprig<DTFObject>>) Activator.CreateInstance(genericMapType);
        }
        
    }
}