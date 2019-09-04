namespace DynamicTimelineFramework.Exception {
    public class DTFObjectDefinitionException : System.Exception {
        
        public DTFObjectDefinitionException(string message) {
            Message = message;
        }

        public override string Message { get; }
    }
}