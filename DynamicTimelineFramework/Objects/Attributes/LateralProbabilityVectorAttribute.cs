using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Interfaces;

namespace DynamicTimelineFramework.Objects.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LateralProbabilityVectorAttribute : Attribute 
    {
        public string LateralKey { get; }
        
        public IProbabilityVectorComponent<DTFObject>[] Components { get; }

        public LateralProbabilityVectorAttribute(string lateralKey, params IProbabilityVectorComponent<DTFObject>[] components) {
            LateralKey = lateralKey;
            Components = components;
        }
    }
}