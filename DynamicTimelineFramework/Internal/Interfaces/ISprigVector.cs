using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface ISprigVector<T> : ICopyable<ISprigVector<T>> where T : BinaryPosition
    {
        Node<T>.INode Head { get; }

        void ShiftForward(ulong amount);

        void ShiftBackward(ulong amount);
    }
}