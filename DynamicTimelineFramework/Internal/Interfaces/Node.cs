using System;
using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal static class Node<T> where T : BinaryPosition
    {
        public static INode And<TOther>(INode left, Node<TOther>.INode right) where TOther : BinaryPosition{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = left.MakeNode(null, Math.Max(currentLeft.Index, currentRight.Index), (T) currentLeft.SuperPosition.And(currentRight.SuperPosition));

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = left.MakeNode(null, Math.Max(currentLeft.Index, currentRight.Index), (T) currentLeft.SuperPosition.And(currentRight.SuperPosition));

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
            }

            return head;
        }
        
        public static INode Or<TOther>(INode left, Node<TOther>.INode right) where TOther : BinaryPosition{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = left.MakeNode(null, Math.Max(currentLeft.Index, currentRight.Index), (T) currentLeft.SuperPosition.Or(currentRight.SuperPosition));

            var leftGreaterPlaceholder = currentLeft.Index;
            
            if (currentLeft.Index >= currentRight.Index)
                currentLeft = currentLeft.Last;
            
            if (currentRight.Index >= leftGreaterPlaceholder)
                currentRight = currentRight.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = left.MakeNode(null, Math.Max(currentLeft.Index, currentRight.Index), (T) currentLeft.SuperPosition.Or(currentRight.SuperPosition));

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = currentRight.Last;
            }

            return head;
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