using System;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class Sprig
    {
        private SprigNode<PositionBuffer> _head;
        
        public SprigNode<PositionBuffer> Head
        {
            get => _head;

            set
            {
                //Todo - We need to inform the spine that a new head is assigned, which will inform the neighboring branches
            }
        }

        public Sprig()
        {
            Head = new SprigNode<PositionBuffer>(null, 0, new PositionBuffer());
        }

        public void Alloc(int space, int startIndex)
        {
            var current = Head;
            var buffer = (PositionBuffer) current.SuperPosition;
            
            //Don't allocate if the buffer is already allocated
            while (buffer != null && buffer.Length == startIndex)
            {
                buffer.Alloc(space);
                
                //Perform the same on the last
                current = current.Last;
                buffer = (PositionBuffer) current?.SuperPosition;
            }
        }

        public Position GetPosition(ulong date, DTFObject obj)
        {
            var current = Head;

            while (current.Index > date)
            {
                current = current.Last;
            }

            var slice = new Slice(current.SuperPosition, obj.HeadlessSlice.LeftBound, obj.HeadlessSlice.RightBound);
            
            return new Position(obj.GetType(), slice);
        }
        
        public void And(SprigVector other)
        {
            var currentSprig = Head;
            var currentVector = other.Head;
            
            var currentNew = new SprigNode<PositionBuffer>(null, Math.Max(currentSprig.Index, currentVector.Index), currentSprig.SuperPosition & currentVector.SuperPosition);
            var newHead = currentNew;
            
            if(currentNew.SuperPosition.UncertaintyAtSlice(other.Slice) == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentSprig.Index;
            
            if (currentSprig.Index >= currentVector.Index)
                currentSprig = currentSprig.Last;
            
            if (currentVector.Index >= leftGreaterPlaceholder)
                currentVector = currentVector.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode<PositionBuffer>(null, Math.Max(currentSprig.Index, currentVector.Index), currentSprig.SuperPosition & currentVector.SuperPosition);
                
                if(newNode.SuperPosition.UncertaintyAtSlice(other.Slice) == -1)
                    throw new ParadoxException();
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentSprig.Index;
            
                if (currentSprig.Index >= currentVector.Index)
                    currentSprig = currentSprig.Last;
            
                if (currentVector.Index >= leftGreaterPlaceholder)
                    currentVector = currentVector.Last;
            }
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = newHead;
        }
        
        public void Or(SprigVector other) {
            var currentSprig = Head;
            var currentVector = other.Head;
            
            var currentNew = new SprigNode<PositionBuffer>(null, Math.Max(currentSprig.Index, currentVector.Index), currentSprig.SuperPosition & currentVector.SuperPosition);
            var newHead = currentNew;
            
            if(currentNew.SuperPosition.UncertaintyAtSlice(other.Slice) == -1)
                throw new ParadoxException();

            var leftGreaterPlaceholder = currentSprig.Index;
            
            if (currentSprig.Index >= currentVector.Index)
                currentSprig = currentSprig.Last;
            
            if (currentVector.Index >= leftGreaterPlaceholder)
                currentVector = currentVector.Last;

            while (currentNew.Index != 0)
            {
                //AND the positions and set the index to the greater of the nodes
                var newNode = new SprigNode<PositionBuffer>(null, Math.Max(currentSprig.Index, currentVector.Index), currentSprig.SuperPosition & currentVector.SuperPosition);
                
                if(newNode.SuperPosition.UncertaintyAtSlice(other.Slice) == -1)
                    throw new ParadoxException();
                
                //If the new position is equal to the current node in the new list, then just update the start position of the new list
                if (newNode.SuperPosition.Equals(currentNew.SuperPosition))
                    currentNew.Index = newNode.Index;

                else
                    currentNew.Last = newNode;
                
                leftGreaterPlaceholder = currentSprig.Index;
            
                if (currentSprig.Index >= currentVector.Index)
                    currentSprig = currentSprig.Last;
            
                if (currentVector.Index >= leftGreaterPlaceholder)
                    currentVector = currentVector.Last;
            }
            
            //We only should assign the new head if any changes were made
            if (!Head.Equals(newHead))
                Head = newHead;
        }
    }
}