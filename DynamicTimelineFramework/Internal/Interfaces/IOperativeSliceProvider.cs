using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface IOperativeSliceProvider
    {
        Slice OperativeSlice { get; }
    }
}