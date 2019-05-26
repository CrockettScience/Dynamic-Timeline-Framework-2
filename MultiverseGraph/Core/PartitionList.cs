using System;
using System.Collections.Generic;

namespace MultiverseGraph.Core
{
    internal class PartitionList
    {
        private PartitionNode _head;

        public Position this[ulong index]
        {
            get
            {
                var current = _head;
                
                //Iterate back until the node range covers the index
                while (current.StartIndex > index)
                {
                    current = current.Previous;
                }

                return current.Position;
            }
        }

        public PartitionList()
        {
            _head = new PartitionNode(new Position(), 0);
        }
        
        public PartitionList(Position position)
        {
            _head = new PartitionNode(position, 0);
        }

        private PartitionList(PartitionNode head)
        {
            _head = head;
        }

        public void And(PartitionList other)
        {
            
        }

        public static PartitionList operator &(PartitionList left, PartitionList right)
        {
            var currentLeft = left._head;
            var currentRight = right._head;
            
            var currentNew = new PartitionNode(currentLeft.Position & currentRight.Position, currentLeft.StartIndex >= currentRight.StartIndex ? currentLeft.StartIndex : currentRight.StartIndex);
            var newPartition = new PartitionList(currentNew);
            
            if (currentLeft.StartIndex >= currentRight.StartIndex)
                currentLeft = currentLeft.Previous;
            
            if (currentRight.StartIndex >= currentLeft.StartIndex)
                currentRight = currentRight.Previous;

            while (currentNew.StartIndex == 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new PartitionNode(currentLeft.Position & currentRight.Position, currentLeft.StartIndex >= currentRight.StartIndex ? currentLeft.StartIndex : currentRight.StartIndex);
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.Position == currentNew.Position)
                    currentNew.StartIndex = newNode.StartIndex;

                else
                    currentNew.Previous = newNode;
            
                if (currentLeft.StartIndex >= currentRight.StartIndex)
                    currentLeft = currentLeft.Previous;
            
                if (currentRight.StartIndex >= currentLeft.StartIndex)
                    currentRight = currentRight.Previous;
            }

            return newPartition;


        }
        
        private class PartitionNode
        {
            internal Position Position;
            internal ulong StartIndex;
            internal PartitionNode Previous;

            internal LinkedList<PartitionNode> SeverenceSubscribers;

            internal PartitionNode(Position position, ulong startIndex)
            {
                Position = position;
                StartIndex = startIndex;
            }
        }
    }
}