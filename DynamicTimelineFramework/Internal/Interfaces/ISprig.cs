using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface ISprig<out T> where T : DTFObject
    {
        IPosition<T> this[ulong index]
        {
            get;
        }
    }
}