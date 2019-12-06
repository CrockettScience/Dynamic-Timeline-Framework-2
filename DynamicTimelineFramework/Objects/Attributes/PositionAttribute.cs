using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PositionAttribute : Attribute
    {
        internal long Length { get; }
        internal int[] ForwardTransitionSetBits { get; }

        /// <summary>
        /// PositionAttributes are necessary to define the various states a DTFObject can have
        /// </summary>
        /// <param name="length">The amount of time that this position lasts. Must be positive</param>
        /// <param name="forwardTransitionSetBits">The Set Bit positions indicating the potential states that can follow this state</param>
        public PositionAttribute(long length, params int[] forwardTransitionSetBits) {
            Length = length;
            ForwardTransitionSetBits = forwardTransitionSetBits;
        }
    }
}