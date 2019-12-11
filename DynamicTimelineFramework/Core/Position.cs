using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Position : BinaryPosition
    {

        #region STATIC

        public static Position Alloc(Type type, params int[] setBits)
        {
            //Make a position
            var pos = Alloc(type);
            
            //Set appropriate bits
            foreach (var setBit in setBits) {
                pos[setBit] = true;
            }

            return pos;
        }
        
        public static Position Alloc(Type type, bool superPosition = false)
        {
            //Make a new Buffer
            var buffer = new PositionBuffer();
            
            //Make a position out of the buffer
            var pos = buffer.Alloc(type, superPosition);

            return pos;
        }
        
        public static Position Alloc<T>(params int[] setBits) where T : DTFObject
        {
            //Make a position
            var pos = Alloc(typeof(T));
            
            //Set appropriate bits
            foreach (var setBit in setBits) {
                pos[setBit] = true;
            }

            return pos;
        }
        
        public static Position operator &(Position left, Position right)
        {
            return (Position) left.And(right);
        }
        
        public static Position operator |(Position left, Position right) {
            return (Position) left.Or(right);
        }
        
        public static Position operator ^(Position lhs, Position rhs)
        {
            var pos = Alloc(lhs.Type);
            
            if(lhs.Type != rhs.Type)
                throw new InvalidOperationException("Both positions must be for the same type of DTFObject");

            for (var i = 0; i < lhs.Length; i++)
            {
                pos[i] = lhs[i] ^ rhs[i];
            }

            return pos;
        }

        #endregion
        
        internal IOperativeSliceProvider OperativeSliceProvider { private get; set; }

        internal ReferenceSlice ReferenceSlice { get; }

        internal OperativeSlice OperativeSlice => OperativeSliceProvider.OperativeSlice;

        public override int Length => ReferenceSlice.RightBound - ReferenceSlice.LeftBound;

        public Type Type { get; }

        public int Uncertainty
        {
            get {
                var flag = ReferenceSlice.Head.Flag;

                var lb = ReferenceSlice.LeftBound;
                var rb = ReferenceSlice.RightBound;
                
                //Count the number of set bits in the bitArray
                var hWeight = 0;

                for (var i = lb; i < rb; i++) {
                    if (flag[i])
                        hWeight++;
                }
                
                //Uncertainty is the Hamming Weight of the bits minus one
                return hWeight - 1;
            }
        }

        internal Position(Type type, ReferenceSlice referenceSlice)
        {
            Type = type;
            ReferenceSlice = referenceSlice;
        }

        /// <summary>
        /// Returns an array of positions such that each position has 0 uncertainty and all positions OR'd together
        /// equals the current position. In other words:
        ///
        /// anyPosition.Equals(anyPosition.GetEigenStates().Aggregate(anyPosition, (i, p) => i | p)) is always TRUE;
        /// </summary>
        /// <returns></returns>
        public List<Position> GetEigenStates()
        {
            
            //Create a buffer to hold all the eigenvalues.
            var buffer = new PositionBuffer();
            
            //Iterate through and make positions for each set bit
            var flag = ReferenceSlice.Head.Flag;

            var lb = ReferenceSlice.LeftBound;
            var rb = ReferenceSlice.RightBound;

            var eigenVals = new List<Position>();
            
            for (var i = lb; i < rb; i++) {
                if (flag[i]) {
                    var eigenVal = buffer.Alloc(Type);
                    eigenVal[i - lb] = true;
                    eigenVals.Add(eigenVal);
                }
            }

            return eigenVals;
        }

        public override bool Equals(object obj) {
            if (!(obj is Position other)) return false;

            for (var i = 0; i < Length; i++) {
                if (this[i] ^ other[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode() {
            var hash = int.MaxValue;

            for (var i = 0; i < Length; i++) {
                hash ^= this[i] ? i * 367 : i * 397;
            }

            return hash;
        }

        public override bool this[int index] {
            get {
                if(index >= Length)
                    throw new IndexOutOfRangeException();
                
                return ReferenceSlice.Head[ReferenceSlice.LeftBound + index];
            }
            set {
                if(index >= Length)
                    throw new IndexOutOfRangeException();
                
                ReferenceSlice.Head[ReferenceSlice.LeftBound + index] = value;
            }
        }

        public override BinaryPosition Copy()
        {
            var pos = Alloc(Type);

            for (var i = 0; i < Length; i++)
            {
                pos[i] = this[i];
            }

            pos.OperativeSliceProvider = OperativeSliceProvider;

            return pos;
        }

        internal override BinaryPosition And(BinaryPosition other)
        {
            var pos = (Position) Copy();
            
            switch (other)
            {
                case Position otherPos when Type != otherPos.Type:
                    throw new InvalidOperationException("Both positions must be for the same type of DTFObject");
                
                case Position otherPos:
                {
                    for (var i = 0; i < Length; i++)
                    {
                        pos[i] &= otherPos[i];
                    }

                    break;
                }
                case PositionBuffer otherBuff:
                {
                    var bufferIndex = OperativeSlice.LeftBound;
                
                    for (var i = 0; i < Length; i++, bufferIndex++)
                    {
                        pos[i] &= otherBuff[bufferIndex];
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            return pos;
        }

        internal override BinaryPosition Or(BinaryPosition other)
        {
            var pos = (Position) Copy();
            
            switch (other)
            {
                case Position otherPos when Type != otherPos.Type:
                    throw new InvalidOperationException("Both positions must be for the same type of DTFObject");
                
                case Position otherPos:
                {
                    for (var i = 0; i < Length; i++)
                    {
                        pos[i] |= otherPos[i];
                    }

                    break;
                }
                case PositionBuffer otherBuff:
                {
                    var bufferIndex = OperativeSlice.LeftBound;
                
                    for (var i = 0; i < Length; i++, bufferIndex++)
                    {
                        pos[i] |= otherBuff[bufferIndex];
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            return pos;
        }
    }
}