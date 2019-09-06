using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Multiverse
{
    public class Position
    {
        public Type Type { get; }

        public static Position Alloc(Type type, params long[] flag)
        {
            
            var pos = new Position(((DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor, type);
            
            if(flag.Length != pos.Flag.Length)
                throw new InvalidFlagSpaceFactorException(type, pos.Flag.Length, flag.Length);
            
            for (var i = 0; i < pos.Flag.Length; i++) {
                pos.Flag[i] = flag[i];
            }

            return pos;
        }
        
        public static Position Alloc(Type type, bool defaultValue = false)
        {
            var pos = new Position(((DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor, type);
            
            var value = defaultValue ? long.MaxValue : long.MinValue;

            for (var i = 0; i < pos.Flag.Length; i++) {
                pos._flag[i] = value;
            }
            
            return pos;
        }
        
        public static Position Alloc<T>(params long[] flag) where T : DTFObject
        {
            
            var pos = new Position(((DTFObjectDefinitionAttribute) typeof(T).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor, typeof(T));
            
            if(flag.Length != pos.Flag.Length)
                throw new InvalidFlagSpaceFactorException(typeof(T), pos.Flag.Length, flag.Length);
            
            for (var i = 0; i < pos.Flag.Length; i++) {
                pos.Flag[i] = flag[i];
            }

            return pos;
        }
        
        public static Position Alloc<T>(bool defaultValue = false) where T : DTFObject
        {
            var pos = new Position(((DTFObjectDefinitionAttribute) typeof(T).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor, typeof(T));
            
            var value = defaultValue ? long.MaxValue : long.MinValue;

            for (var i = 0; i < pos.Flag.Length; i++) {
                pos._flag[i] = value;
            }
            
            return pos;
        }
        
        public static void ReAlloc(Position pos)
        {
            pos._flag = new long[pos._flag.Length];
        }

        private long[] _flag;

        public long[] Flag
        {
            get
            {
                //Make a copy and return. This will prevent modification of the struct from outside it's scope
                var copy = new long[_flag.Length];
                
                Array.Copy(_flag, copy, copy.Length);

                return copy;
            }
        }

        public int Uncertainty
        {
            get
            {
                var hammingWeight = 0;

                foreach (var i in Flag)
                {
                    hammingWeight += BitHacks.NumberOfSetBits(i);
                }

                return hammingWeight - 1;
            }
        }

        private Position(int length, Type type)
        {
            Type = type;
            _flag = new long[length];
        }

        private Position(long[] flag, Type type)
        {
            Type = type;
            _flag = flag;
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
            //Use a "sliding" mask to construct the collection
            var eigenValues = new List<Position>();

            //Get the first flag
            var flag = new long[_flag.Length];
            flag[0] = 1;
            
            var mask = new Position(flag, Type);

            for (var i = 0; i < flag.Length; i++)
            {
                //Mask and add a position if a value exists
                var eigenValue = mask & this;
                
                if(eigenValue.Uncertainty == 0)
                    eigenValues.Add(eigenValue);

                mask.ShiftLeftByOne();

            }

            return eigenValues;
        }

        public override string ToString()
        {
            var str = "";

            foreach (var i in Flag)
            {
                str += Convert.ToString(i, 2);
            }

            return str;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position pos)) return false;

            if (_flag.Length != pos._flag.Length) return false;
            
            for (var i = 0; i < Flag.Length; i++)
            {
                if (Flag[i] != pos.Flag[i])
                    return false;
            }

            return true;

        }

        public override int GetHashCode()
        {
            var hash = int.MaxValue;

            foreach (var i in Flag)
            {
                hash ^= (int) i * 367;
            }

            return hash;
        }
        
        
        public static Position operator &(Position lhs, Position rhs)
        {
            if(lhs._flag.Length != rhs._flag.Length)
                throw new InvalidOperationException("Positions given are made for two different types");
            
            var newPos = new Position(lhs._flag.Length, lhs.Type);
            
            for (var i = 0; i < lhs.Flag.Length; i++)
            {
                newPos.Flag[i] = lhs.Flag[i] & rhs.Flag[i];
            }

            return newPos;
        }
        
        public static Position operator |(Position lhs, Position rhs)
        {
            if(lhs._flag.Length != rhs._flag.Length)
                throw new InvalidOperationException("Positions given are made for two different types");
            
            var newPos = new Position(lhs._flag.Length, lhs.Type);
            
            for (var i = 0; i < lhs.Flag.Length; i++)
            {
                newPos.Flag[i] = lhs.Flag[i] | rhs.Flag[i];
            }

            return newPos;
        }
        
        public static Position operator ^(Position lhs, Position rhs)
        {
            if(lhs._flag.Length != rhs._flag.Length)
                throw new InvalidOperationException("Positions given are made for two different types");
            
            var newPos = new Position(lhs._flag.Length, lhs.Type);
            
            for (var i = 0; i < lhs.Flag.Length; i++)
            {
                newPos.Flag[i] = lhs.Flag[i] ^ rhs.Flag[i];
            }

            return newPos;
        }
        
        public static implicit operator long[](Position position) {
            return position._flag;
        }
        
        private void ShiftLeftByOne()
        {
            var curFlag = Flag;
            
            for (var i = _flag.Length - 1; i >= 0; i--)
            {
                //Shift the flag
                _flag[i] = curFlag[i] << 1;

                if (i > 0)
                {
                    //Compute the carryover
                    _flag[i - 1] |= curFlag[i - 1] >> 63;
                }
            }
        }
    }
}