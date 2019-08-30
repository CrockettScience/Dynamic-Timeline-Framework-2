using System;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DTFObjectDefinitionAttribute : Attribute
    {
        internal int BitSpaceFactor;

        public DTFObjectDefinitionAttribute(int bitSpaceFactor)
        {
            BitSpaceFactor = bitSpaceFactor;
        }
    }
}