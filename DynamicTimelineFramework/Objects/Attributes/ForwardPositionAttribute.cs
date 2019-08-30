using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ForwardPositionAttribute : Attribute
    {
        public long[] Flag;

        public ForwardPositionAttribute(params long[] flag) {
            Flag = flag;

        }
    }
}