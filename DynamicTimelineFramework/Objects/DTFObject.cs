using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Sprig;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        #region COMPILER

        static DTFObject() {
            var typesForCompile  = Assembly.GetAssembly()
        }
        
        #endregion
        
        private Dictionary<Diff, Sprig> _sprigs;
        internal Sprig this[Diff diff] => _sprigs[diff];



        #region ATTRIBUTES
        
        [AttributeUsage(AttributeTargets.Field)]
        public class ForwardRuleAttribute : Attribute {
            public Position Range { get; }

            public ForwardRuleAttribute(Position range) {
                Range = range;
            }
        }
        
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class LateralRuleAttribute : Attribute {
            public string LateralKey { get; }
            public Position Range { get; }

            public LateralRuleAttribute(string lateralKey, Position range) {
                LateralKey = lateralKey;
                Range = range;
            }
        }

        #endregion
    }
}