using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LateralObjectAttribute : Attribute 
    {
        public string LateralKey { get; }

        public LateralObjectAttribute(string lateralKey, bool parent = false) {
            LateralKey = lateralKey;
        }
    }
}