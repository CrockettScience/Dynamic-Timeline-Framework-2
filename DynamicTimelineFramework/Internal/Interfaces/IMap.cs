namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface IMap<in TKey, out TValue>
    {
        TValue this[TKey key]
        {
            get;
        }
    }
}