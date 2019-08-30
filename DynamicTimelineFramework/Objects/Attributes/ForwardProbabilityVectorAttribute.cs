using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Interfaces;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ForwardProbabilityVectorAttribute : Attribute
    {
        public IProbabilityVectorComponent<DTFObject>[] Components;

        public ForwardProbabilityVectorAttribute (params IProbabilityVectorComponent<DTFObject>[] components)
        {
            Components = components;
        }
    }
}