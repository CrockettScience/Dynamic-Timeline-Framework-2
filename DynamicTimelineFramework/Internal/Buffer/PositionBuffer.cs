using System;
using System.Collections;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Interfaces;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal.Buffer {
    internal class PositionBuffer : BinaryPosition
    {
        public static PositionBuffer operator &(PositionBuffer left, Position right)
        {
            return (PositionBuffer) left.And(right);
        }
        
        public static PositionBuffer operator |(PositionBuffer left, Position right) {
            return (PositionBuffer) left.Or(right);
        }

        public BitArray Bits { get; }

        public int Length => Bits.Length;

        public PositionBuffer() {
            Bits = new BitArray(0);
        }
        
        public PositionBuffer(BitArray bits)
        {
            Bits = bits;
        }

        public PositionBuffer(int size, bool defaultValue)
        {
            Bits = new BitArray(size, defaultValue);
        }

        public Position Alloc(Type type, bool defaultValue = false) {
            var objDef = (DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            
            var space = objDef.PositionCount;
            var leftBound = Bits.Length;
            
            Alloc(space, defaultValue);
            
            return new Position(type, new ReferenceSlice(this, leftBound, leftBound + space));
        }

        public void Alloc(int space, bool defaultValue = false)
        {
            var targetSize = Bits.Length + space;
            
            //Increasing the count sets them to false by default
            if (defaultValue)
            {
                var startingSize = Bits.Length;
                Bits.Length = targetSize;

                for (var i = startingSize; i < targetSize; i++)
                {
                    Bits[i] = true;
                }
            }

            else
            {
                Bits.Length = targetSize;
            }
        }

        public bool this[int index] {
            get => Bits[index];
            set => Bits[index] = value;
        }

        public PositionBuffer Copy() {
            
            return new PositionBuffer((BitArray) Bits.Clone());
        }

        internal override BinaryPosition And(BinaryPosition other)
        {
            var buffer = Copy();
            
            switch (other)
            {
                case Position otherPos:
                {
                    var bufferIndex = otherPos.OperativeSlice.LeftBound;
                    for (var i = 0; i < otherPos.Length; i++, bufferIndex++)
                    {
                        buffer[bufferIndex] &= otherPos[i];
                    }

                    break;
                }
                
                case PositionBuffer otherBuff:
                {
                    buffer.Bits.And(otherBuff.Bits);
                    break;
                }
                
                default:
                    throw new InvalidOperationException();
            }

            return buffer;
        }

        internal override BinaryPosition Or(BinaryPosition other)
        {
            var buffer = Copy();
            
            switch (other)
            {
                case Position otherPos:
                {
                    var bufferIndex = otherPos.OperativeSlice.LeftBound;
                    for (var i = 0; i < otherPos.Length; i++, bufferIndex++)
                    {
                        buffer[bufferIndex] |= otherPos[i];
                    }

                    break;
                }
                
                case PositionBuffer otherBuff:
                {
                    buffer.Bits.Or(otherBuff.Bits);
                    break;
                }
                
                default:
                    throw new InvalidOperationException();
            }

            return buffer;
        }

        public Position PositionAtSlice(Type type, ISlice slice)
        {
            return new Position(type, new ReferenceSlice(this, slice.LeftBound, slice.RightBound));
        }

        public void Clear(OperativeSlice address, bool value = false) {
            for (var i = address.LeftBound; i < address.RightBound; i++) {
                this[i] = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PositionBuffer other)) return false;

            for (var i = 0; i < Bits.Count; i++) {
                if (Bits[i] ^ other.Bits[i])
                    return false;
            }

            return true;
        }
    }
}