
namespace DynamicTimelineFramework.Exception
{
    public class InvalidBridgingException : System.Exception
    {
        public override string Message { get; }

        public InvalidBridgingException()
        {
            Message = "A lateral object has incompatible constraints with another object or an already existing timeline";
        }
    }
}