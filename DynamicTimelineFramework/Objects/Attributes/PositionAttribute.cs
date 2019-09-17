using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PositionAttribute : Attribute
    {
        internal ulong Length { get; }
        internal long[] ForwardTransition { get; }

        public PositionAttribute(ulong length, params long[] forwardTransition) {
            Length = length;
            ForwardTransition = forwardTransition;
        }
    }
}