using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Multiverse
{
    public struct Position<T> : IPosition<T> where T : DTFObject
    {
        public static Position<T> Alloc(params long[] flag)
        {
            
            var pos = new Position<T>
            {
                Flag = new long[((DTFObjectDefinitionAttribute) typeof(T).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor]
            };
            
            if(flag.Length != pos.Flag.Length)
                throw new InvalidFlagSpaceFactorException(typeof(T), pos.Flag.Length, flag.Length);
            
            for (var i = 0; i < pos.Flag.Length; i++) {
                pos.Flag[i] = flag[i];
            }

            return pos;
        }
        
        public static Position<T> Alloc(bool defaultValue = false)
        {
            var pos = new Position<T>
            {
                Flag = new long[((DTFObjectDefinitionAttribute) typeof(T).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor]
            };
            
            var value = defaultValue ? long.MaxValue : long.MinValue;

            for (var i = 0; i < pos.Flag.Length; i++) {
                pos.Flag[i] = value;
            }
            
            return pos;
        }
        
        public static void ReAlloc(Position<T> pos)
        {
            pos.Flag = new long[((DTFObjectDefinitionAttribute) typeof(T).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor];
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

            private set { _flag = value; }
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

        /// <summary>
        /// Returns an array of positions such that each position has 0 uncertainty and all positions OR'd together
        /// equals the current position. In other words:
        ///
        /// anyPosition.Equals(anyPosition.GetEigenValues().Aggregate(anyPosition, (i, p) => i | p) is always TRUE;
        /// </summary>
        /// <returns></returns>
        public List<Position<T>> GetEigenValues()
        {
            //Todo - Use a "sliding" mask to construct the collection
            var eigenValues = new List<Position<T>>();

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
            if (!(obj is Position<T> pos)) return false;
            
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
        
        
        public static Position<T> operator &(Position<T> lhs, Position<T> rhs)
        {
            var newPos = new Position<T>();
            
            for (var i = 0; i < lhs.Flag.Length; i++)
            {
                newPos.Flag[i] = lhs.Flag[i] & rhs.Flag[i];
            }

            return newPos;
        }
        
        public static Position<T> operator |(Position<T> lhs, Position<T> rhs)
        {
            var newPos = new Position<T>();
            
            for (var i = 0; i < lhs.Flag.Length; i++)
            {
                newPos.Flag[i] = lhs.Flag[i] | rhs.Flag[i];
            }

            return newPos;
        }
        
        public static implicit operator Position<T>(long[] rhs) {
            return Alloc(rhs);
        }
        
        public static implicit operator long[](Position<T> rhs) {
            return rhs.Flag;
        }
    }
}