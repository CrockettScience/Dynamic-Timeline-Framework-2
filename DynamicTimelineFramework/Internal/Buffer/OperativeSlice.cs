using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Buffer
{
    public struct OperativeSlice : ISlice
    {
        public int LeftBound { get; }
        public int RightBound { get; }
        
        public OperativeSlice(int leftBound, int rightBound) {
            LeftBound = leftBound;
            RightBound = rightBound;
        }

        public override bool Equals(object obj) {
            if (!(obj is OperativeSlice other)) return false;

            return LeftBound == other.LeftBound && RightBound == other.RightBound;
        }
        
        public override int GetHashCode() {
            return LeftBound * 367 ^ LeftBound * 997;
        }
    }
}