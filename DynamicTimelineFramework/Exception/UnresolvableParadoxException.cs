using System;

namespace DynamicTimelineFramework.Exception
{
    public class UnresolvableParadoxException : System.Exception
    {
        public override string Message { get; }

        public UnresolvableParadoxException()
        {
            Message = "A situation was encountered where no viable states were found during the operation";
        }
    }
}