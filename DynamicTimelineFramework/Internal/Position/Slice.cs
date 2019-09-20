namespace DynamicTimelineFramework.Internal.Position {
    internal struct Slice {
        public readonly PositionBuffer Head;
        public readonly int LeftBound;
        public readonly int RightBound;
            
        public Slice(PositionBuffer head, int leftBound, int rightBound) {
            Head = head;
            LeftBound = leftBound;
            RightBound = rightBound;
        }
        
        public Slice(int leftBound, int rightBound) {
            Head = null;
            LeftBound = leftBound;
            RightBound = rightBound;
        }

        public override bool Equals(object obj) {
            if (!(obj is Slice other)) return false;

            if (other == this) return true;
            
            //All headless slices are assumed to not be equal to any other slice
            if (Head == null || other.Head == null) return false;

            var space = RightBound - LeftBound;

            if (space != other.RightBound - other.LeftBound) return false;

            //Returns true if the flags are the same
            for (var i = LeftBound; i < RightBound; i++) {
                if (Head[i] ^ other.Head[i])
                    return false;
            }

            return true;
        }
        
        public override int GetHashCode() {
            var hash = 0;
            
            for (var i = LeftBound; i < RightBound; i++) {
                hash ^= Head[i] ? i * 367 : i * 997;

            }

            return hash;
        }

        public static bool operator ==(Slice lhs, Slice rhs) {
            return lhs.Head == rhs.Head && lhs.LeftBound == rhs.LeftBound;
        }

        public static bool operator !=(Slice lhs, Slice rhs) {
            return !(lhs == rhs);
        }
    }
}