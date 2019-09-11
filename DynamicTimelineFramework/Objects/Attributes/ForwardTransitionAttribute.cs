using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ForwardTransitionAttribute : Attribute
    {
        internal ulong Length { get; }
        internal long[] ForwardPosition { get; }

        public ForwardTransitionAttribute(ulong length, params long[] forwardPosition) {
            Length = length;
            ForwardPosition = forwardPosition;
        }
    }
}