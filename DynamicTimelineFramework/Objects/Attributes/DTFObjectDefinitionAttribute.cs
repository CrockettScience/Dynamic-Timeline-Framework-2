using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute
    {
        internal string MvIdentifierKey { get; }
        internal int BitSpaceFactor { get; }

        public DTFObjectDefinitionAttribute(int bitSpaceFactor, string mvIdentifierKey) {
            MvIdentifierKey = mvIdentifierKey;
            BitSpaceFactor = bitSpaceFactor;
        }
    }
}