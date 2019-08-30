using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core.Tools.Interfaces
{
    internal interface ISprig<out T> where T : DTFObject
    {
        IPosition<T> this[ulong index]
        {
            get;
        }
    }
}