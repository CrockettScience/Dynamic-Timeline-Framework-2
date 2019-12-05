using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PositionAttribute : Attribute
    {
        internal ulong Length { get; }
        internal int[] ForwardTransitionSetBits { get; }

        /// <summary>
        /// PositionAttributes are necessary to define the various states a DTFObject can have
        /// </summary>
        /// <param name="length">The amount of time that this position lasts</param>
        /// <param name="forwardTransitionSetBits">The Set Bit positions indicating the potential states that can follow this state</param>
        public PositionAttribute(ulong length, params int[] forwardTransitionSetBits) {
            Length = length;
            ForwardTransitionSetBits = forwardTransitionSetBits;
        }
    }
}