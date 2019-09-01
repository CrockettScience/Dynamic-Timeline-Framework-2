using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute
    {
        public string MvIdentifierKey { get; }
        internal int BitSpaceFactor { get; }
        public ulong DatePeriodLength { get; }

        public DTFObjectDefinitionAttribute(int bitSpaceFactor, ulong datePeriodLength, string mvIdentifierKey) {
            MvIdentifierKey = mvIdentifierKey;
            BitSpaceFactor = bitSpaceFactor;
            DatePeriodLength = datePeriodLength;
        }
    }
}