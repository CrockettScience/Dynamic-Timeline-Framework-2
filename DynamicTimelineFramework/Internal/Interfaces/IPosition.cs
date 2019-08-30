using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface IPosition<out T> where T : DTFObject
    {
        long[] Flag { get; }
        int Uncertainty { get; }
        
    }
}