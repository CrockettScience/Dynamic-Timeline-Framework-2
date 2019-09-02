using System;

namespace DynamicTimelineFramework.Exception
{
    public class ParadoxException : System.Exception
    {
        public override string Message { get; }

        public ParadoxException()
        {
            Message = "A situation was encountered where no viable states were found during an operation";
        }
    }
}