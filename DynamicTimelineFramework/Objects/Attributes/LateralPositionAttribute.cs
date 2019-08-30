using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralPositionAttribute : Attribute 
    {
        public string PositionKey { get; }
        public long[] Flag;

        public LateralPositionAttribute(string positionKey, params long[] flag) {
            PositionKey = positionKey;
            Flag = flag;

        }
    }
}