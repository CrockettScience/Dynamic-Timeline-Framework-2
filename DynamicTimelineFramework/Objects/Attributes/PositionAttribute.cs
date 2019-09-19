using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PositionAttribute : Attribute
    {
        internal ulong Length { get; }
        internal int[] ForwardTransitionSetBits { get; }

        public PositionAttribute(ulong length, params int[] forwardTransitionSetBits) {
            Length = length;
            ForwardTransitionSetBits = forwardTransitionSetBits;
        }
    }
}