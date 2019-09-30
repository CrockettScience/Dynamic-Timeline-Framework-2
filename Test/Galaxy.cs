using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    //Lets say the period length is about 3.5 billion years
    [DTFObjectDefinition(6)]
    public class Galaxy : DTFObject {
        
        [Position(1_250_000_000_000, (int) Stage.PreExistent, (int) Stage.ProtoGalaxy)]
        public static readonly Position PreExistent = Position.Alloc<Galaxy>((int) Stage.PreExistent);
        
        [Position(1_250_000_000_000, (int) Stage.Young)]
        public static readonly Position ProtoGalaxy = Position.Alloc<Galaxy>((int) Stage.ProtoGalaxy);
        
        [Position(1_250_000_000_000, (int) Stage.Middle)]
        public static readonly Position Young =      Position.Alloc<Galaxy>((int) Stage.Young);
        
        [Position(1_250_000_000_000, (int) Stage.Old)]
        public static readonly Position Middle =      Position.Alloc<Galaxy>((int) Stage.Middle);
        
        [Position(1_250_000_000_000, (int) Stage.Degenerate)]
        public static readonly Position Old =      Position.Alloc<Galaxy>((int) Stage.Old);
        
        [Position(1_250_000_000_000, (int) Stage.Degenerate)]
        public static readonly Position Degenerate =  Position.Alloc<Galaxy>((int) Stage.Degenerate);

        public Galaxy(Multiverse owner) : base(owner)
        {}

        public override Position InitialSuperPosition() {
            return PreExistent;
        }

        public override Position TerminalSuperPosition() {
            return Degenerate;
        }


        public enum Stage {
            PreExistent,
            ProtoGalaxy,
            Young,
            Middle,      
            Old,         
            Degenerate
        }
    }
}