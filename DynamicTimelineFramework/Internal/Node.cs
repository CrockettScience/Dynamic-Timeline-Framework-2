using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Internal {
    internal class Node {
        /*
         * The AND & OR functions perform the task that can be thought of as performing the operation STRAIGHT across
         * two INode structures. All usages assume left and right are the node heads of their respective structures.
         * The index passed in is used to partially add structures, attaching the end of the resulting structure
         * to the remaining portion prior to index on left, preserving those node references.
         */
        
        public static T And<T>(T left, T right) where T : INode{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = (T) left.AndFactory(currentLeft, currentRight);

            while (currentNew.Index != 0)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = (T) currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = (T) currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = (T) left.AndFactory(currentLeft, currentRight);

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.IsSamePosition(currentNew))
                    currentNew.Index = newNode.Index;

                else
                {
                    currentNew.Last = newNode;
                    currentNew = newNode;
                }
            }

            return (T) head;
        }

        public static T Or<T>(T left, T right) where T : INode{
            var currentLeft = left;
            var currentRight = right;

            INode head;
            var currentNew = head = (T) left.OrFactory(currentLeft, currentRight);

            while (currentNew.Index != 0)
            {

                var leftGreaterPlaceholder = currentLeft.Index;
            
                if (currentLeft.Index >= currentRight.Index)
                    currentLeft = (T) currentLeft.Last;
            
                if (currentRight.Index >= leftGreaterPlaceholder)
                    currentRight = (T) currentRight.Last;
                
                //AND the positions and set the index to the greater of the nodes
                var newNode = (T) left.OrFactory(currentLeft, currentRight);

                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.IsSamePosition(currentNew))
                    currentNew.Index = newNode.Index;

                else
                {
                    currentNew.Last = newNode;
                    currentNew = newNode;
                }
            }

            return (T) head;
        }

        internal interface INode
        {
            INode Last { get; set; }

            ulong Index { get; set; }

            bool Validate();

            INode AndFactory(INode left, INode right);
            
            INode OrFactory(INode left, INode right);

            bool IsSamePosition(INode other);
        }
    }
}