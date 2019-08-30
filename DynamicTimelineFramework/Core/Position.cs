using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core.Tools;
using DynamicTimelineFramework.Core.Tools.Interfaces;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    public struct Position<T> : IPosition<T> where T : DTFObject
    {
        public static Position<TAlloc> Alloc<TAlloc>() where TAlloc : DTFObject
        {
            var pos = new Position<TAlloc>
            {
                Flag = new long[((DTFObjectDefinitionAttribute) typeof(TAlloc).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor]
            };

            return pos;
        }
        
        public static void ReAlloc<TAlloc>(Position<TAlloc> pos) where TAlloc : DTFObject
        {
            pos.Flag = new long[((DTFObjectDefinitionAttribute) typeof(TAlloc).GetCustomAttribute(typeof(DTFObjectDefinitionAttribute))).BitSpaceFactor];
        }
        
        public long[] Flag { get; private set; }
        
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
    }
}