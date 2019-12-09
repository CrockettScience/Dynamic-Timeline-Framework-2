using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test {
    [DTFObjectDefinition(14)]
    public class Star : DTFObject {

        private const string GALAXY_KEY = "Galaxy";
        
        [Position(1, (int) Stage.PreExistent, (int) Stage.Birth)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.PreExistent, (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.ProtoGalaxy)]
        public static readonly Position PreExistence     = Position.Alloc<Star>((int) Stage.PreExistent);

        [Position(1, (int) Stage.Proto)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.ProtoGalaxy)]
        public static readonly Position Birth            = Position.Alloc<Star>((int) Stage.Birth);
        
        [Position(3_600_000_000, (int) Stage.MainSequence, (int) Stage.Massive)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.ProtoGalaxy)]
        public static readonly Position Proto            = Position.Alloc<Star>((int) Stage.Proto);
        
        [Position(3_600_000_000, (int) Stage.RedGiant)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position MainSequence     = Position.Alloc<Star>((int) Stage.MainSequence);
        
        [Position(3_600_000_000, (int) Stage.RedSuperGiant)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position Massive          = Position.Alloc<Star>((int) Stage.Massive);
        
        [Position(3_600_000_000, (int) Stage.PlanetaryNebula)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position RedGiant         = Position.Alloc<Star>((int) Stage.RedGiant);
        
        [Position(3_600_000_000, (int) Stage.Supernova)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position RedSuperGiant    = Position.Alloc<Star>((int) Stage.RedSuperGiant);
        
        [Position(3_600_000_000, (int) Stage.Burnoff)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position PlanetaryNebula  = Position.Alloc<Star>((int) Stage.PlanetaryNebula);

        [Position(1, (int) Stage.WhiteDwarf)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.Degenerate)]
        public static readonly Position Burnoff       = Position.Alloc<Star>((int) Stage.Burnoff);
        
        [Position(1, (int) Stage.WhiteDwarf)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.Degenerate)]
        public static readonly Position WhiteDwarf       = Position.Alloc<Star>((int) Stage.WhiteDwarf);
        
        [Position(350, (int) Stage.Death)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old)]
        public static readonly Position Supernova        = Position.Alloc<Star>((int) Stage.Supernova);
        
        [Position(1, (int) Stage.Neutron, (int) Stage.BlackHole)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.Degenerate)]
        public static readonly Position Death          = Position.Alloc<Star>((int) Stage.Death);
        
        [Position(1, (int) Stage.Neutron)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.Degenerate)]
        public static readonly Position Neutron          = Position.Alloc<Star>((int) Stage.Neutron);
        
        [Position(1, (int) Stage.BlackHole)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.Young, (int) Galaxy.Stage.Middle, (int) Galaxy.Stage.Old, (int) Galaxy.Stage.Degenerate)]
        public static readonly Position BlackHole        = Position.Alloc<Star>((int) Stage.BlackHole);

        public Star(Galaxy galaxy, Multiverse owner) : base(owner){
            SetLateralObject(GALAXY_KEY, galaxy);
        }

        public override Position InitialSuperPosition() {
            return PreExistence;
        }

        public override Position TerminalSuperPosition() {
            return WhiteDwarf | Neutron | BlackHole;
        }

        public enum Stage {
            PreExistent, 
            Birth,
            Proto, 
            MainSequence, 
            Massive,
            RedGiant, 
            RedSuperGiant, 
            Burnoff,
            WhiteDwarf, 
            Neutron, 
            BlackHole, 
            PlanetaryNebula, 
            Supernova,
            Death,
        }
    }
}