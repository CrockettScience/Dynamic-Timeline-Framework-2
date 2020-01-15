using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class PositionNode
    {
        public Position SuperPosition { get; }

        public PositionNode Last { get; set; }

        public ulong Index { get; set; }

        public PositionNode(PositionNode last, ulong index, Position superPosition)
        {
            Last = last;
            Index = index;
            SuperPosition = superPosition;
        }

        public bool Validate()
        {
            if (SuperPosition.Uncertainty == -1)
                return false;

            return Last == null || ((PositionNode) Last).Validate();
        }
        
        /*
         * The AND & OR functions perform the task that can be thought of as performing the operation STRAIGHT across
         * two INode structures. All usages assume left and right are the node heads of their respective structures.
         * The index passed in is used to partially add structures, attaching the end of the resulting structure
         * to the remaining portion prior to index on left, preserving those node references.
         */
        
        public static PositionNode And(PositionNode left, PositionNode right, ulong index = 0) {
            var currentLeft = left;
            var currentRight = right;

            PositionNode head;
            var currentNew = head = new PositionNode(null, Max(currentLeft.Index, currentRight.Index, index), currentLeft.SuperPosition & currentRight.SuperPosition);

            while (currentNew.Index != index)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = new PositionNode(null, Max(currentLeft.Index, currentRight.Index, index), currentLeft.SuperPosition & currentRight.SuperPosition);

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                {
                    currentNew.Last = newNode;
                    currentNew = newNode;
                }
            }

            currentNew.Last = currentNew.Index == currentLeft.Index ? currentLeft.Last : currentLeft;

            return head;
        }

        public static PositionNode Or(PositionNode left, PositionNode right, ulong index = 0) {
            var currentLeft = left;
            var currentRight = right;

            PositionNode head;
            var currentNew = head = new PositionNode(null, Max(currentLeft.Index, currentRight.Index, index),  currentLeft.SuperPosition | currentRight.SuperPosition);

            while (currentNew.Index != index)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = new PositionNode(null, Max(currentLeft.Index, currentRight.Index, index), currentLeft.SuperPosition | currentRight.SuperPosition);

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                {
                    currentNew.Last = newNode;
                    currentNew = newNode;
                }
            }

            currentNew.Last = currentNew.Index == currentLeft.Index ? currentLeft.Last : currentLeft;

            return head;
        }

        private static ulong Max(ulong i1, ulong i2, ulong i3)
        {
            var num = 0UL;

            if (i1 > num)
            {
                num = i1;
            }

            if (i2 > num)
            {
                num = i2;
            }

            if (i3 > num)
            {
                num = i3;
            }

            return num;
        }
    }
}