using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Interfaces
{
    public interface IPosition<out T> where T : DTFObject
    {
        long[] Flag { get; }
        int Uncertainty { get; }
        
    }
}