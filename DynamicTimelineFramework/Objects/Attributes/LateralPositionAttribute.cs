using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralPositionAttribute : Attribute 
    {
        internal string LateralKey { get; }
        internal long[] Flag { get; }
        internal Type Type { get; }

        public LateralPositionAttribute(string lateralKey, Type type, params long[] flag) {
            LateralKey = lateralKey;
            Type = type;
            Flag = flag;

        }
    }
}