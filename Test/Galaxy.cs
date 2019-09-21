using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    //Lets say the period length is about 3.5 billion years
    [DTFObjectDefinition(6)]
    public class Galaxy : DTFObject {
        
        [Position(1_250_000_000_000, (int) Stage.PRE_EXISTENT, (int) Stage.PROTO_GALAXY)]
        public static readonly Position PreExistent = Position.Alloc<Galaxy>((int) Stage.PRE_EXISTENT);
        
        [Position(1_250_000_000_000, (int) Stage.YOUNG)]
        public static readonly Position ProtoGalaxy = Position.Alloc<Galaxy>((int) Stage.PROTO_GALAXY);
        
        [Position(1_250_000_000_000, (int) Stage.MIDDLE)]
        public static readonly Position Young =      Position.Alloc<Galaxy>((int) Stage.YOUNG);
        
        [Position(1_250_000_000_000, (int) Stage.OLD)]
        public static readonly Position Middle =      Position.Alloc<Galaxy>((int) Stage.MIDDLE);
        
        [Position(1_250_000_000_000, (int) Stage.DEGENERATE)]
        public static readonly Position Old =      Position.Alloc<Galaxy>((int) Stage.OLD);
        
        [Position(1_250_000_000_000, (int) Stage.DEGENERATE)]
        public static readonly Position Degenerate =  Position.Alloc<Galaxy>((int) Stage.DEGENERATE);

        public Galaxy(Multiverse owner) : base(owner)
        {}

        public override Position InitialSuperPosition() {
            return PreExistent;
        }

        public override Position TerminalSuperPosition() {
            return Degenerate;
        }


        public enum Stage {
            PRE_EXISTENT,
            PROTO_GALAXY,
            YOUNG,
            MIDDLE,      
            OLD,         
            DEGENERATE
        }
    }
}