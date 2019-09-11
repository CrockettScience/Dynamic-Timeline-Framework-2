using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute
    {
        internal string MvID { get; }
        internal int BitSpaceFactor { get; }

        public DTFObjectDefinitionAttribute(int bitSpaceFactor, string mvId) {
            MvID = mvId;
            BitSpaceFactor = bitSpaceFactor;
        }
    }
}