namespace DynamicTimelineFramework.Internal.State {
    internal struct Stake {
        public readonly PositionBuffer Buffer;
        public readonly int LeftBound;
        public readonly int RightBound;
            
        public Stake(PositionBuffer buffer, int leftBound, int rightBound) {
            Buffer = buffer;
            LeftBound = leftBound;
            RightBound = rightBound;
        }

        public override bool Equals(object obj) {
            if (!(obj is Stake other)) return false;

            return other.Buffer == Buffer && other.LeftBound == LeftBound;
        }
        
        public override int GetHashCode() {
            return Buffer.GetHashCode() ^ LeftBound * 367;
        }
    }
}