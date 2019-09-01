using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralPositionAttribute : Attribute 
    {
        public string LateralKey { get; }
        public long[] Flag;

        public LateralPositionAttribute(string lateralKey, params long[] flag) {
            LateralKey = lateralKey;
            Flag = flag;

        }
    }
}