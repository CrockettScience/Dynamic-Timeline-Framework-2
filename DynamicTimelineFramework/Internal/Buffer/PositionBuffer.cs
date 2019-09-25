using System;
using System.Collections;
using System.Reflection;
using DynamicTimelineFramework.Core;
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

        public BitArray Flag { get; }

        public override int Length => Flag.Length;

        public PositionBuffer() {
            Flag = new BitArray(0);
        }
        
        public PositionBuffer(BitArray flag)
        {
            Flag = flag;
        }

        public PositionBuffer(int size, bool defaultValue)
        {
            Flag = new BitArray(size, defaultValue);
        }

        public Position Alloc(Type type, bool defaultValue = false) {
            var objDef = (DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            
            var space = objDef.PositionCount;
            var leftBound = Flag.Length;
            
            Alloc(space, defaultValue);
            
            return new Position(type, new Slice(this, leftBound, Flag.Length + space));
        }

        public void Alloc(int space, bool defaultValue = false)
        {
            var targetSize = Flag.Length + space;
            
            //Increasing the count sets them to false by default
            if (defaultValue)
            {
                var startingSize = Flag.Length;
                Flag.Length = targetSize;

                for (var i = startingSize; i < targetSize; i++)
                {
                    Flag[i] = true;
                }
            }

            else
            {
                Flag.Length = targetSize;
            }
        }

        public override bool this[int index] {
            get => Flag[index];
            set => Flag[index] = value;
        }

        public override BinaryPosition Copy()
        {
            var flag = new BitArray(Flag.Length);

            for (var i = 0; i > flag.Length; i++)
            {
                flag[i] = Flag[i];
            }
            
            return new PositionBuffer(flag);
        }

        internal override BinaryPosition And(BinaryPosition other)
        {
            var buffer = (PositionBuffer) Copy();
            
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
                    for (var i = 0; i < Length; i++)
                    {
                        buffer[i] &= otherBuff[i];
                    }

                    break;
                }
                
                default:
                    throw new InvalidOperationException();
            }

            return buffer;
        }

        internal override BinaryPosition Or(BinaryPosition other)
        {
            var buffer = (PositionBuffer) Copy();
            
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
                    for (var i = 0; i < Length; i++)
                    {
                        buffer[i] |= otherBuff[i];
                    }

                    break;
                }
                
                default:
                    throw new InvalidOperationException();
            }

            return buffer;
        }

        public int UncertaintyAtSlice(Slice slice)
        {
            var lb = slice.LeftBound;
            var rb = slice.RightBound;
                
            //Count the number of set bits in the bitArray
            var hWeight = 0;

            for (var i = lb; i < rb; i++) {
                if (Flag[i])
                    hWeight++;
            }
                
            //Uncertainty is the Hamming Weight of the bits minus one
            return hWeight - 1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PositionBuffer other)) return false;

            return Flag.Equals(other.Flag);
        }
    }
}