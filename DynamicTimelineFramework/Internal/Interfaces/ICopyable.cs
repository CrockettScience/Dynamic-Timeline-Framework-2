namespace DynamicTimelineFramework.Internal.Interfaces
{
    public interface ICopyable<out T>
    {
        T Copy();
    }
}