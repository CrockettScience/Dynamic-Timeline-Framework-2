using System;
using System.Collections.Generic;
using System.Linq;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;

namespace DynamicTimelineFramework.Internal.Sprig
{
    internal class BufferVectorBuilder
    {
        private Node _head = new Node(null, ulong.MaxValue, null, 0);
        private readonly int _size;

        public BufferVectorBuilder(int bitCount)
        {
            _size = bitCount;
            _head.Last = new Node(null, 0, null, _size);
        }

        public void Add(PositionVector vector)
        {
            var currentPositionNode = (PositionNode) vector.Head;
            var currentBuilderNode = _head;

            while (currentPositionNode != null)
            {
                while (currentBuilderNode.Last.Index > currentPositionNode.Index)
                {
                    currentBuilderNode = currentBuilderNode.Last;
                    currentBuilderNode.Add(currentPositionNode.SuperPosition);
                }
                
                if (currentBuilderNode.Last.Index < currentPositionNode.Index)
                {
                    currentBuilderNode.Last = new Node(currentBuilderNode.Last, currentPositionNode.Index, currentPositionNode.SuperPosition, _size);
                    currentBuilderNode = currentBuilderNode.Last;
                    
                    currentBuilderNode.Add(currentBuilderNode.Last);
                }

                else if (currentBuilderNode.Last.Index == currentPositionNode.Index)
                {
                    currentBuilderNode = currentBuilderNode.Last;
                    
                    currentBuilderNode.Add(currentPositionNode.SuperPosition);
                }

                currentPositionNode = (PositionNode) currentPositionNode.Last;
            }
        }

        public BufferVector And()
        {
            var builderCurrent = _head.Last;
            
            BufferNode bufferHead;
            var bufferCurrent = bufferHead = new BufferNode(null, builderCurrent.Index, builderCurrent.And());

            builderCurrent = builderCurrent.Last;
            while (builderCurrent != null)
            {
                bufferCurrent.Last = new BufferNode(null, builderCurrent.Index, builderCurrent.And());

                bufferCurrent = (BufferNode) bufferCurrent.Last;
                builderCurrent = builderCurrent.Last;
            }

            return new BufferVector(bufferHead);
        }
        
        public BufferVector Or()
        {
            
            var builderCurrent = _head.Last;
            
            BufferNode bufferHead;
            var bufferCurrent = bufferHead = new BufferNode(null, builderCurrent.Index, builderCurrent.Or());

            builderCurrent = builderCurrent.Last;
            while (builderCurrent != null)
            {
                bufferCurrent.Last = new BufferNode(null, builderCurrent.Index, builderCurrent.Or());

                bufferCurrent = (BufferNode) bufferCurrent.Last;
                builderCurrent = builderCurrent.Last;
            }

            return new BufferVector(bufferHead);
        }

        private class Node
        {
            public Node Last { get; set; }
            
            public ulong Index { get; }

            private readonly List<Position> _positions = new List<Position>();
            
            private readonly int _size;

            public Node(Node last, ulong index, Position pos, int size)
            {
                Index = index;
                _size = size;
                Last = last;
                
                if(pos != null)
                    _positions.Add(pos);
            }


            public void Add(Position pos)
            {
                _positions.Add(pos);
            }
            
            public void Add(Node otherNode)
            {
                foreach (var pos in otherNode._positions)
                    _positions.Add(pos);
            }

            public PositionBuffer And()
            {
                var buffer = new PositionBuffer(_size, true);

                return _positions.Aggregate(buffer, (current, position) => current & position);
            }
            
            public PositionBuffer Or()
            {
                
                var buffer = new PositionBuffer(_size, false);

                return _positions.Aggregate(buffer, (current, position) => current | position);
            }
        }
    }
}