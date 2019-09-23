using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigNode <T> where T : ISuperPosition {

        public T SuperPosition { get; }
        public SprigNode<T> Last { get; set; }
        public ulong Index { get; set; }
        

        public SprigNode(SprigNode<T> last, ulong index, T superPosition)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
        }

        public SprigNode<T> Copy()
        {
            return new SprigNode<T>(Last.Copy(), Index, (T) SuperPosition.Copy());
        }

#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            if (!(obj is SprigNode<T> other)) return false;

            if (Index != other.Index || !SuperPosition.Equals(other.SuperPosition)) return false;

            return Last.Equals(other.Last);
        }
    }
}