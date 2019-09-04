using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute
    {
        internal string MvIdentifierKey { get; }
        internal int BitSpaceFactor { get; }
        internal ulong DatePeriodLength { get; }

        public DTFObjectDefinitionAttribute(int bitSpaceFactor, ulong datePeriodLength, string mvIdentifierKey) {
            MvIdentifierKey = mvIdentifierKey;
            BitSpaceFactor = bitSpaceFactor;
            DatePeriodLength = datePeriodLength;
        }
    }
}