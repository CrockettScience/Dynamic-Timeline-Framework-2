using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    /// <summary>
    /// A position represents are particular state or superstate. 
    /// </summary>
    public class Position
    {

        #region STATIC
        internal static Position Alloc(Type type, bool defaultValue = false)
        {
            var pos = new Position(type);

            if (defaultValue) {
                var i = 0;
                for (; i < pos.bits.Length - 1; i++)
                    pos.bits[i] = ~0;

                var leftoverLength = pos.Length % 32;

                pos.bits[i] = (1 << leftoverLength) - 1;
            }

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
                if(setBit >= pos.Length)
                    throw new IndexOutOfRangeException("Set bit " + setBit + 
                                                       " is not within range for position defined for DTFObject type " + 
                                                       pos.Type.Name + ", which only has " + pos.Length + " states to encode.");
                pos[setBit] = true;
            }

            return pos;
        }
        
        public static Position operator &(Position left, Position right) {
            var pos = Alloc(left.Type);

            for (var i = 0; i < left.bits.Length; i++) {
                pos.bits[i] = left.bits[i] & right.bits[i];
            }

            return pos;
        }
        public static Position operator |(Position left, Position right) {
            var pos = Alloc(left.Type);

            for (var i = 0; i < left.bits.Length; i++) {
                pos.bits[i] = left.bits[i] | right.bits[i];
            }

            return pos;
        }

        #endregion
        
        private readonly bool _readOnly;

        /// <summary>
        /// Total number of bits in the position. Represents the maximum number of eigenstates
        /// </summary>
        public readonly int Length;

        private int[] bits;

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
                
                //Count the number of set bits. AKA the "Hamming Weight"
                var hWeight = 0;

                foreach (var i in bits) {
                    var value = i;
                    
                    var count = 0;
                    while (value != 0)
                    {
                        count++;
                        value &= value - 1;
                    }
                    
                    hWeight += count;
                }
                
                //Uncertainty is the Hamming Weight of the bits minus one
                return hWeight - 1;
            }
        }

        internal Position(Type type, bool readOnly = false)
        {
            var objDef = (DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            Length = objDef.PositionCount;
            
            bits = new int[(Length - 1) / 32 + 1];
            _readOnly = readOnly;
            Type = type;
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
            var eigenVals = new List<Position>();

            for (var i = 0; i < bits.Length; i++) {
                var num = 1;
                
                do {
                    if ((num & bits[i]) != 0) {
                        var pos = Alloc(Type);
                        pos.bits[i] = num;
                        eigenVals.Add(pos);
                    }

                    num <<= 1;
                } 
                while (num != 0);
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
                
                var valueIndex = index / 32;
                var modIndex = index % 32;

                return ((1 << modIndex) & bits[valueIndex]) != 0;
            }
            
            set {
                if(_readOnly)
                    throw new ReadOnlyException();
                    
                if(index >= Length)
                    throw new IndexOutOfRangeException();

                var valueIndex = index / 32;
                var modIndex = index % 32;

                bits[valueIndex] = (1 << modIndex) | bits[valueIndex];
            }
        }

        /// <summary>
        /// Duplicates the object
        /// </summary>
        /// <returns>A copy of the position</returns>
        public Position Copy() {
            var pos = Alloc(Type);
            Array.Copy(bits, pos.bits, bits.Length);
            
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
    }
}