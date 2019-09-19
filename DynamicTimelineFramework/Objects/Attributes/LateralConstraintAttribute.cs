using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralConstraintAttribute : Attribute 
    {
        internal string LateralKey { get; }
        internal int[] ConstraintSetBits { get; }
        internal Type Type { get; }

        public LateralConstraintAttribute(string lateralKey, Type type, params int[] constraintSetBits) {
            LateralKey = lateralKey;
            Type = type;
            ConstraintSetBits = constraintSetBits;

        }
    }
}