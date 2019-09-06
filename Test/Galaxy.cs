using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    //Lets say the period length is about 3.5 billion years
    [DTFObjectDefinition(1, "TEST")]
    public class Galaxy : DTFObject {

        internal const long PRE_EXISTENT =     1;
        internal const long PROTO_GALAXY =     1 << 1;
        internal const long YOUNG =            1 << 2;
        internal const long MIDDLE =           1 << 3;
        internal const long OLD =              1 << 4;
        internal const long DEGENERATE =       1 << 31;
        
        [Position(1_250_000_000_000, PRE_EXISTENT | PROTO_GALAXY)]
        public static readonly Position PreExistent = Position.Alloc<Galaxy>(PRE_EXISTENT);
        
        [Position(1_250_000_000_000, YOUNG)]
        public static readonly Position ProtoGalaxy = Position.Alloc<Galaxy>(PROTO_GALAXY);
        
        [Position(1_250_000_000_000, MIDDLE)]
        public static readonly Position Young =      Position.Alloc<Galaxy>(YOUNG);
        
        [Position(1_250_000_000_000, OLD)]
        public static readonly Position Middle =      Position.Alloc<Galaxy>(MIDDLE);
        
        [Position(1_250_000_000_000, DEGENERATE)]
        public static readonly Position Old =      Position.Alloc<Galaxy>(OLD);
        
        [Position(1_250_000_000_000, DEGENERATE)]
        public static readonly Position Degenerate =  Position.Alloc<Galaxy>(DEGENERATE);

        public override Position InitialSuperPosition() {
            return PreExistent;
        }

        public override Position TerminalSuperPosition() {
            return Degenerate;
        }
    }
}