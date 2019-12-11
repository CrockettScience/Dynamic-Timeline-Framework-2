using System;

namespace DynamicTimelineFramework.Objects.Attributes {
    /// <summary>
    /// Defines necessary metadata for the definition of objects that inherit from DTFObject.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute {
        internal int PositionCount { get; }

        /// <summary>
        /// Defines necessary metadata for the definition of objects that inherit from DTFObject.
        /// </summary>
        /// <param name="positionCount">The number of different positions defined by this object</param>
        public DTFObjectDefinitionAttribute(int positionCount) {
            PositionCount = positionCount;
        }
    }
}