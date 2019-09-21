using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Core
{
    public class Position : ISuperPosition
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
        
        public static Position Alloc(Type type, bool defaultValue = false)
        {
            //Make a new Buffer
            var buffer = new PositionBuffer();
            
            //Make a position out of the buffer
            var pos = buffer.Alloc(type, defaultValue);

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

        public static Position operator &(Position lhs, Position rhs)
        {
            var pos = Alloc(lhs.Type);
            
            if(lhs.Type != rhs.Type)
                throw new InvalidOperationException("Both positions must be for the same type of DTFObject");

            for (var i = 0; i < lhs.Length; i++)
            {
                pos[i] = lhs[i] && rhs[i];
            }

            return pos;
        }
        
        public static Position operator |(Position lhs, Position rhs)
        {
            var pos = Alloc(lhs.Type);
            
            if(lhs.Type != rhs.Type)
                throw new InvalidOperationException("Both positions must be for the same type of DTFObject");

            for (var i = 0; i < lhs.Length; i++)
            {
                pos[i] = lhs[i] || rhs[i];
            }

            return pos;
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

        internal Slice Slice { get; }

        public int Length => Slice.RightBound - Slice.LeftBound;

        public Type Type { get; }

        public int Uncertainty
        {
            get {
                var flag = Slice.Head.Flag;

                var lb = Slice.LeftBound;
                var rb = Slice.RightBound;
                
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

        internal Position(Type type, Slice slice)
        {
            Type = type;
            Slice = slice;
        }

        /// <summary>
        /// Returns an array of positions such that each position has 0 uncertainty and all positions OR'd together
        /// equals the current position. In other words:
        ///
        /// anyPosition.Equals(anyPosition.GetEigenValues().Aggregate(anyPosition, (i, p) => i | p) is always TRUE;
        /// </summary>
        /// <returns></returns>
        public List<Position> GetEigenValues()
        {
            
            //Create a buffer to hold all the eigenvalues.
            var buffer = new PositionBuffer();
            
            //Iterate through and make positions for each set bit
            var flag = Slice.Head.Flag;

            var lb = Slice.LeftBound;
            var rb = Slice.RightBound;

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

            return other.Slice.Equals(Slice);
        }

        public override int GetHashCode() {
            return Slice.GetHashCode();
        }

        public bool this[int index] {
            get {
                if(index >= Length)
                    throw new IndexOutOfRangeException();
                
                return Slice.Head[Slice.LeftBound + index];
            }
            set {
                if(index >= Length)
                    throw new IndexOutOfRangeException();
                
                Slice.Head[Slice.LeftBound + index] = value;
            }
        }

        public ISuperPosition Copy()
        {
            var pos = Alloc(Type);

            for (var i = 0; i < Length; i++)
            {
                pos[i] = this[i];
            }

            return pos;
        }
    }
}