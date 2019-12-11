namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface ICopyable<out T>
    {
        T Copy();
    }
}