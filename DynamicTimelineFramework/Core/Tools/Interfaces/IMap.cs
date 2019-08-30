namespace DynamicTimelineFramework.Core.Tools.Interfaces
{
    public interface IMap<in TKey, out TValue>
    {
        TValue this[TKey key]
        {
            get;
        }
    }
}