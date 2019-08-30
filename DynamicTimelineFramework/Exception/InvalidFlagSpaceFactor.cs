using System;

namespace DynamicTimelineFramework.Exception {
    public class InvalidFlagSpaceFactorException : System.Exception{
        
        public InvalidFlagSpaceFactorException(Type t, int expected, int attempted) {
            Message = "The flag space factor for DTFObject " + t.Name + " is " + expected + ", but an allocation of " + attempted + " was attempted.";
        }

        public override string Message { get; }
    }
}