using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PositionAttribute : Attribute
    {
        internal ulong Length { get; }
        internal long[] ForwardPosition { get; }

        public PositionAttribute(ulong length, params long[] forwardPosition) {
            Length = length;
            ForwardPosition = forwardPosition;
        }
    }
}