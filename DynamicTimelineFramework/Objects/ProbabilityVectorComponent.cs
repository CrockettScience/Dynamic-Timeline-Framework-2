using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Interfaces;

namespace DynamicTimelineFramework.Objects
{
    public struct ProbabilityVectorComponent<T> : IProbabilityVectorComponent<T> where T : DTFObject
    {
        public double Probability;
        public Position<T> Position;

        public ProbabilityVectorComponent(double probability, Position<T> position)
        {
            Position = position;
            Probability = probability;
        }
    }
}