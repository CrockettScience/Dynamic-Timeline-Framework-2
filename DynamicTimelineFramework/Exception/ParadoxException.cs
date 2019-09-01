using System;

namespace DynamicTimelineFramework.Exception
{
    public class ParadoxException : System.Exception
    {
        public override string Message { get; }

        public ParadoxException(Type t)
        {
            Message = "A situation was encountered where no viable states were found for type " + t.Name;
        }
    }
}