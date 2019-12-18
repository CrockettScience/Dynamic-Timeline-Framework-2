using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    /// <summary>
    /// A position represents are particular state or superstate. 
    /// </summary>
    public class Position : BinaryPosition
    {

        #region STATIC
        internal static Position Alloc(Type type, bool superPosition = false)
        {
            //Make a new Buffer
            var buffer = new PositionBuffer();
            
            //Make a position out of the buffer
            var pos = buffer.Alloc(type, superPosition);

            return pos;
        }

        /// <summary>
        /// Used to construct a position.
        /// </summary>
        /// <param name="type">The Type of DTFObject to make a position of</param>
        /// <param name="setBits">Indices to set to true. Each set bit represents a single eigenstate</param>
        /// <returns>The position that represents the state or superstate articulated by the set bits</returns>
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
        
        /// <summary>
        /// Used to construct a position.
        /// </summary>
        /// <param name="setBits">Indices to set to true. Each set bit represents a single eigenstate</param>
        /// <typeparam name="T">The Type of DTFObject to make a position of</typeparam>
        /// <returns>The position that represents the state or superstate articulated by the set bits</returns>
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
        
        private readonly bool _readOnly;
        
        internal IOperativeSliceProvider OperativeSliceProvider { private get; set; }

        internal ReferenceSlice ReferenceSlice { get; }

        internal OperativeSlice OperativeSlice => OperativeSliceProvider.OperativeSlice;

        /// <summary>
        /// Total number of bits in the position. Represents the maximum number of eigenstates
        /// </summary>
        public int Length => ReferenceSlice.RightBound - ReferenceSlice.LeftBound;

        /// <summary>
        /// The DTFObject type that the position represent a state or superstate of.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// A measure of the amount of uncertainty encoded in the Position. If there is only one eigenstate, the
        /// uncertainty will be 0. A value of -1 means there are no eigenstates.
        /// </summary>
        public int Uncertainty
        {
            get {
                var flag = ReferenceSlice.Head.Bits;

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

        internal Position(Type type, ReferenceSlice referenceSlice, bool readOnly = false)
        {
            _readOnly = readOnly;
            Type = type;
            ReferenceSlice = referenceSlice;
        }

        /// <summary>
        /// Retrieves a List of positions such that each position has 0 uncertainty and all positions OR'd together
        /// equals the current position. In other words:
        ///
        /// anyPosition.Equals(anyPosition.GetEigenstates().Aggregate(anyPosition, (i, p) => i | p)) is always TRUE;
        /// </summary>
        /// <returns></returns>
        public List<Position> GetEigenstates()
        {
            
            //Create a buffer to hold all the eigenvalues.
            var buffer = new PositionBuffer();
            
            //Iterate through and make positions for each set bit
            var flag = ReferenceSlice.Head.Bits;

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

        /// <summary>
        /// Accesses the value of the bit at index
        /// </summary>
        /// <param name="index">The index of the bit to access</param>
        /// <exception cref="IndexOutOfRangeException"> If the index is greater than Length</exception>
        public bool this[int index] {
            get {
                if(index >= Length)
                    throw new IndexOutOfRangeException();
                
                return ReferenceSlice.Head[ReferenceSlice.LeftBound + index];
            }
            set {
                if(_readOnly)
                    throw new ReadOnlyException();
                    
                if(index >= Length)
                    throw new IndexOutOfRangeException();

                ReferenceSlice.Head[ReferenceSlice.LeftBound + index] = value;
            }
        }

        /// <summary>
        /// Duplicates the object
        /// </summary>
        /// <returns>A copy of the position</returns>
        public Position Copy() {
            var newReference = ReferenceSlice.Head.Copy();

            var pos = new Position(Type, new ReferenceSlice(newReference, ReferenceSlice.LeftBound, ReferenceSlice.RightBound)) {
                OperativeSliceProvider = OperativeSliceProvider
            };

            return pos;
        }

        public override string ToString() {
            var stringOut = "Type: " + Type.Name + ", Value: ";

            if (Uncertainty == -1)
                return stringOut + "IN PARADOX";

            var firstField = true;
            
            foreach (var field in Type.GetFields()) {
                //If it has a position attribute
                if (field.GetCustomAttribute(typeof(PositionAttribute)) is PositionAttribute) {
                    
                    //Add it's name if it's part of the superposition
                    if (((Position) field.GetValue(null) & this).Uncertainty != -1) {
                        stringOut += (firstField ? "" : "/") + field.Name;
                        firstField = false;
                    }
                }
            }

            return stringOut;
        }

        internal override BinaryPosition And(BinaryPosition other)
        {
            var pos = Copy();
            
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
            var pos = Copy();
            
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