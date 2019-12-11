namespace DynamicTimelineFramework.Internal.Buffer {
    
    /// <summary>
    /// The parent class of Position. Only used internally.
    /// </summary>
    public abstract class BinaryPosition{

        internal abstract BinaryPosition And(BinaryPosition other);
        
        internal abstract BinaryPosition Or(BinaryPosition other);

        internal BinaryPosition()
        {
            
        }
    }
}