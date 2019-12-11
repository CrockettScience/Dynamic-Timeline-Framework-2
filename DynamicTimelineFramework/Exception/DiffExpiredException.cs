
namespace DynamicTimelineFramework.Exception
{
    public class DiffExpiredException : System.Exception
    {
        public override string Message { get; }

        public DiffExpiredException()
        {
            Message = "The diff given is no longer valid for Universe construction";
        }
    }
}