using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    //Lets say the period length is about 3.5 billion years
    [DTFObjectDefinition(1, 1_250_000_000_000, "TEST")]
    public class Galaxy : DTFObject {

        internal const long PRE_EXISTENT =     1;
        internal const long PROTO_GALAXY =     1 << 1;
        internal const long YOUNG =            1 << 2;
        internal const long MIDDLE =           1 << 3;
        internal const long OLD =              1 << 4;
        internal const long DEGENERATE =       1 << 31;
        
        [ForwardPosition(PRE_EXISTENT | PROTO_GALAXY)]
        public static readonly Position PreExistent = Position.Alloc<Galaxy>(PRE_EXISTENT);
        
        [ForwardPosition(YOUNG)]
        public static readonly Position ProtoGalaxy = Position.Alloc<Galaxy>(PROTO_GALAXY);
        
        [ForwardPosition(MIDDLE)]
        public static readonly Position Young =      Position.Alloc<Galaxy>(YOUNG);
        
        [ForwardPosition(OLD)]
        public static readonly Position Middle =      Position.Alloc<Galaxy>(MIDDLE);
        
        [ForwardPosition(DEGENERATE)]
        public static readonly Position Old =      Position.Alloc<Galaxy>(OLD);
        
        [ForwardPosition(DEGENERATE)]
        public static readonly Position Degenerate =  Position.Alloc<Galaxy>(DEGENERATE);
    }
}