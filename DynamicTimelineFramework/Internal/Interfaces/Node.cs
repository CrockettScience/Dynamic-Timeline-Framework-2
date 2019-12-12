using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal static class Node<T> where T : BinaryPosition
    {
        /*
         * The AND & OR functions perform the task that can be thought of as performing the operation STRAIGHT across
         * two INode structures. All usages assume left and right are the node heads of their respective structures.
         * The index passed in is used to partially add structures, attaching the end of the resulting structure
         * to the remaining portion prior to index on left, preserving those node references.
         */
        
        public static INode And<TOther>(INode left, Node<TOther>.INode right, ulong index = 0) where TOther : BinaryPosition{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = left.MakeNode(null, Max(currentLeft.Index, currentRight.Index, index), (T) currentLeft.SuperPosition.And(currentRight.SuperPosition));

            while (currentNew.Index != index)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = left.MakeNode(null, Max(currentLeft.Index, currentRight.Index, index), (T) currentLeft.SuperPosition.And(currentRight.SuperPosition));

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

        public static INode Or<TOther>(INode left, Node<TOther>.INode right, ulong index = 0) where TOther : BinaryPosition{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = left.MakeNode(null, Max(currentLeft.Index, currentRight.Index, index), (T) currentLeft.SuperPosition.Or(currentRight.SuperPosition));

            while (currentNew.Index != index)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = left.MakeNode(null, Max(currentLeft.Index, currentRight.Index, index), (T) currentLeft.SuperPosition.Or(currentRight.SuperPosition));

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
        
        internal interface INode : ICopyable<INode>
        {

            T SuperPosition { get; set; }

            INode Last { get; set; }

            ulong Index { get; set; }

            INode MakeNode(INode last, ulong index, T superPosition);
        }
    }
    
}