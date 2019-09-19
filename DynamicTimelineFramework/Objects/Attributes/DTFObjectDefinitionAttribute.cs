using System;

namespace DynamicTimelineFramework.Objects.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute {
        internal string MvID { get; }
        internal int positionSpace { get; }

        public DTFObjectDefinitionAttribute(int positionSpace, string mvId) {
            MvID = mvId;
            this.positionSpace = positionSpace;
        }
    }
}