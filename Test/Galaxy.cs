using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    [DTFObjectDefinition(8)]
    public class Galaxy : DTFObject {
        
        [Position(1, (int) Stage.PreExistent, (int) Stage.Birth)]
        public static readonly Position PreExistent = Position.Alloc<Galaxy>((int) Stage.PreExistent);
        
        [Position(1, (int) Stage.ProtoGalaxy)]
        public static readonly Position Birth = Position.Alloc<Galaxy>((int) Stage.Birth);
        
        [Position(100, (int) Stage.Young)]
        public static readonly Position ProtoGalaxy = Position.Alloc<Galaxy>((int) Stage.ProtoGalaxy);
        
        [Position(100, (int) Stage.Middle)]
        public static readonly Position Young =      Position.Alloc<Galaxy>((int) Stage.Young);
        
        [Position(100, (int) Stage.Old)]
        public static readonly Position Middle =      Position.Alloc<Galaxy>((int) Stage.Middle);
        
        [Position(100, (int) Stage.Death)]
        public static readonly Position Old =      Position.Alloc<Galaxy>((int) Stage.Old);
        
        [Position(1, (int) Stage.Degenerate)]
        public static readonly Position Death =  Position.Alloc<Galaxy>((int) Stage.Death);
        
        [Position(1, (int) Stage.Degenerate)]
        public static readonly Position Degenerate =  Position.Alloc<Galaxy>((int) Stage.Degenerate);

        public Galaxy(Multiverse owner) : base(owner)
        {}


        public enum Stage {
            PreExistent,
            Birth,
            ProtoGalaxy,
            Young,
            Middle,      
            Old,         
            Death,
            Degenerate
        }
    }
}