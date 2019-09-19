using System;
using System.Collections;
using System.Reflection;
using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal.State {
    internal class PositionBuffer : SuperPosition{

        public BitArray Flag { get; }

        public PositionBuffer() {
            Flag = new BitArray(0);
        }

        public Position AllocatePosition(Type type, bool defaultValue) {

            var objDef = (DTFObjectDefinitionAttribute) type.GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            
            var space = objDef.positionSpace;
            var leftBound = Flag.Length;
            var targetSize = Flag.Length + space;

            while (Flag.Length < targetSize) {
                Flag.Length++;
                Flag[Flag.Length - 1] = defaultValue;
            }
            
            return new Position(type, new Stake(this, leftBound, targetSize));
        }

        public bool this[int index] {
            get => Flag[index];
            set => Flag[index] = value;
        }
    }
}