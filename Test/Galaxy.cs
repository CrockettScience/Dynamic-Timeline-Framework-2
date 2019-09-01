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
        public static readonly Position<Galaxy> PreExistent = Position<Galaxy>.Alloc(PRE_EXISTENT);
        
        [ForwardPosition(YOUNG)]
        public static Position<Galaxy> ProtoGalaxy = Position<Galaxy>.Alloc(PROTO_GALAXY);
        
        [ForwardPosition(MIDDLE)]
        public static Position<Galaxy> Young =      Position<Galaxy>.Alloc(YOUNG);
        
        [ForwardPosition(OLD)]
        public static Position<Galaxy> Middle =      Position<Galaxy>.Alloc(MIDDLE);
        
        [ForwardPosition(DEGENERATE)]
        public static Position<Galaxy> Old =      Position<Galaxy>.Alloc(OLD);
        
        [ForwardPosition(DEGENERATE)]
        public static Position<Galaxy> Degenerate =  Position<Galaxy>.Alloc(DEGENERATE);
    }
}