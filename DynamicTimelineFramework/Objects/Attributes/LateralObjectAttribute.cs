using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LateralObjectAttribute : Attribute 
    {
        public string LateralKey { get; }

        public LateralObjectAttribute(string lateralKey) {
            LateralKey = lateralKey;
        }
    }
}