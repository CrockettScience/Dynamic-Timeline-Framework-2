using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ForwardPositionAttribute : Attribute
    {
        internal long[] Flag { get; }

        public ForwardPositionAttribute(params long[] flag) {
            Flag = flag;

        }
    }
}