
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Spine {
        private SpineNode _root;
        private Diff _rootDiff;

        public Spine(Diff rootDiff)
        {
            _root = new SpineHeadNode();
            _rootDiff = rootDiff;
        }

        public void Alloc(int space, int startIndex)
        {
            //Though somewhat recursive, basically amounts to a Depth-First iteration of the spine tree
            _root.Alloc(space, startIndex);
        }
    }
}