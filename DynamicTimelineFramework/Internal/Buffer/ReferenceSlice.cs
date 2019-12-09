using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Buffer {
    internal struct ReferenceSlice : ISlice {
        public readonly PositionBuffer Head;
        public int LeftBound { get; }
        public int RightBound { get; }
            
        public ReferenceSlice(PositionBuffer head, int leftBound, int rightBound) {
            Head = head;
            LeftBound = leftBound;
            RightBound = rightBound;
        }

        public override bool Equals(object obj) {
            if (!(obj is ReferenceSlice other)) return false;

            if (other == this) return true;
            
            //All headless slices are assumed to not be equal to any other slice
            if (Head == null || other.Head == null) return false;

            var space = RightBound - LeftBound;

            if (space != other.RightBound - other.LeftBound) return false;

            //Returns true if the flags are the same
            for (var i = LeftBound; i < RightBound; i++) {
                if (Head[i] ^ other.Head[i + (other.LeftBound - LeftBound)])
                    return false;
            }

            return true;
        }
        
        public override int GetHashCode() {
            var hash = 0;
            
            for (var i = LeftBound; i < RightBound; i++) {
                hash ^= Head[i] ? (i - LeftBound) * 367 : (i - LeftBound) * 997;

            }

            return hash;
        }

        public static bool operator ==(ReferenceSlice lhs, ReferenceSlice rhs) {
            return lhs.Head == rhs.Head && lhs.LeftBound == rhs.LeftBound;
        }

        public static bool operator !=(ReferenceSlice lhs, ReferenceSlice rhs) {
            return !(lhs == rhs);
        }
    }
}