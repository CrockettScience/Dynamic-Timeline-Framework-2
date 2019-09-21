using System;

namespace DynamicTimelineFramework.Objects.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute {
        internal int PositionCount { get; }

        public DTFObjectDefinitionAttribute(int positionCount) {
            PositionCount = positionCount;
        }
    }
}