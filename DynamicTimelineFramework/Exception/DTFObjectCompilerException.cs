namespace DynamicTimelineFramework.Exception {
    public class DTFObjectCompilerException : System.Exception {
        public DTFObjectCompilerException(string message) {
            Message = message;
        }

        public override string Message { get; }
    }
}