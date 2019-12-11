using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    /// <summary>
    /// Optional attribute that defines the possible state or set of states that exist at the same time the position this is defined over.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralConstraintAttribute : Attribute 
    {
        internal string LateralKey { get; }
        internal int[] ConstraintSetBits { get; }
        internal Type Type { get; }

        /// <summary>
        /// Optional attribute that defines the possible state or set of states that exist at the same time the position this is defined over.
        /// </summary>
        /// <param name="lateralKey">A unique key that identifies the lateral object</param>
        /// <param name="type">The lateral object's type. Must inherit from DTFObject</param>
        /// <param name="constraintSetBits">The set bit indices that constitute the possible eigenstates of the
        /// lateral object that can exist at the same time as this position</param>
        public LateralConstraintAttribute(string lateralKey, Type type, params int[] constraintSetBits) {
            LateralKey = lateralKey;
            Type = type;
            ConstraintSetBits = constraintSetBits;

        }
    }
}