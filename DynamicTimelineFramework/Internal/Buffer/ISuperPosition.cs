namespace DynamicTimelineFramework.Internal.Buffer {
    public interface ISuperPosition {
        
        bool this[int index] { get; set; }
        int Length { get; }
        ISuperPosition Copy();
        
        

    }
}