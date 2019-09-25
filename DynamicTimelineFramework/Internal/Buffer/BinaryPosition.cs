using System;
using DynamicTimelineFramework.Internal.Interfaces;

namespace DynamicTimelineFramework.Internal.Buffer {
    
    public abstract class BinaryPosition : ICopyable<BinaryPosition> {
        
        public abstract bool this[int index] { get; set; }
        
        public abstract int Length { get; }
        
        public abstract BinaryPosition Copy();

        internal abstract BinaryPosition And(BinaryPosition other);
        
        internal abstract BinaryPosition Or(BinaryPosition other);

        internal BinaryPosition()
        {
            
        }
    }
}