using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.State;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Multiverse
{
    public class Position : SuperPosition
    {

        #region STATIC

        public static Position Alloc(Type type, params int[] setBits)
        {
            //Make a new Buffer
            var buffer = new PositionBuffer();
            
            //Make a position out of the buffer
            var pos = buffer.AllocatePosition(type, false);
            
            //Set appropriate bits
            foreach (var setBit in setBits) {
                pos[setBit] = true;
            }

            return pos;
        }
        
        public static Position Alloc(Type type, bool defaultValue = false)
        {
            throw new NotImplementedException();
        }
        
        public static Position Alloc<T>(params int[] flag) where T : DTFObject
        {
            throw new NotImplementedException();
        }

        public static Position operator &(Position lhs, Position rhs)
        {
            throw new NotImplementedException();
        }
        
        public static Position operator |(Position lhs, Position rhs)
        {
            throw new NotImplementedException();
        }
        
        public static Position operator ^(Position lhs, Position rhs)
        {
            throw new NotImplementedException();
        }
        
        public static implicit operator long[](Position position) {
            
            throw new NotImplementedException();
        }

        #endregion

        private readonly Stake _stake;

        public int Space => _stake.RightBound - _stake.LeftBound;

        public Type Type { get; }

        public int Uncertainty
        {
            get {
                var flag = _stake.Buffer.Flag;

                var lb = _stake.LeftBound;
                var rb = _stake.RightBound;
                
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

        internal Position(Type type, Stake stake)
        {
            Type = type;
            _stake = stake;
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
            var flag = _stake.Buffer.Flag;

            var lb = _stake.LeftBound;
            var rb = _stake.RightBound;

            var eigenVals = new List<Position>();
            
            for (var i = lb; i < rb; i++) {
                if (flag[i]) {
                    var eigenVal = buffer.AllocatePosition(Type, false);
                    eigenVal[i - lb] = true;
                    eigenVals.Add(eigenVal);
                }
            }

            return eigenVals;
        }

        public override bool Equals(object obj) {
            if (!(obj is Position other)) return false;

            return other._stake.Equals(_stake);
        }

        public override int GetHashCode() {
            return _stake.GetHashCode();
        }

        public bool this[int index] {
            get {
                if(index >= Space)
                    throw new IndexOutOfRangeException();
                
                return _stake.Buffer[_stake.LeftBound + index];
            }
            set {
                if(index >= Space)
                    throw new IndexOutOfRangeException();
                
                _stake.Buffer[_stake.LeftBound + index] = value;
            }
        }
    }
}