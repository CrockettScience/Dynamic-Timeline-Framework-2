using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test {
    [DTFObjectDefinition(11, "TEST")]
    public class Star : DTFObject {

        private const string GALAXY_KEY = "Galaxy";
        
        [Position(3_600_000_000, (int) Stage.Proto, (int) Stage.PreExistent)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.PRE_EXISTENT, (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD, (int) Galaxy.Stage.PROTO_GALAXY)]
        public static readonly Position PreExistence     = Position.Alloc<Star>((int) Stage.PreExistent);
        
        [Position(3_600_000_000, (int) Stage.MainSequence, (int) Stage.Massive)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD, (int) Galaxy.Stage.PROTO_GALAXY)]
        public static readonly Position Proto            = Position.Alloc<Star>((int) Stage.Proto);
        
        [Position(3_600_000_000, (int) Stage.RedGiant)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position MainSequence     = Position.Alloc<Star>((int) Stage.MainSequence);
        
        [Position(3_600_000_000, (int) Stage.RedSuperGiant)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position Massive          = Position.Alloc<Star>((int) Stage.Massive);
        
        [Position(3_600_000_000, (int) Stage.PlanetaryNebula)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position RedGiant         = Position.Alloc<Star>((int) Stage.RedGiant);
        
        [Position(3_600_000_000, (int) Stage.Supernova)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position RedSuperGiant    = Position.Alloc<Star>((int) Stage.RedSuperGiant);
        
        [Position(3_600_000_000, (int) Stage.WhiteDwarf)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position PlanetaryNebula  = Position.Alloc<Star>((int) Stage.PlanetaryNebula);
        
        [Position(3_600_000_000, (int) Stage.WhiteDwarf)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD, (int) Galaxy.Stage.DEGENERATE)]
        public static readonly Position WhiteDwarf       = Position.Alloc<Star>((int) Stage.WhiteDwarf);
        
        [Position(350, (int) Stage.Neutron, (int) Stage.BlackHole)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD)]
        public static readonly Position Supernova    = Position.Alloc<Star>((int) Stage.Supernova);
        
        [Position(3_599_999_650, (int) Stage.Neutron)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD, (int) Galaxy.Stage.DEGENERATE)]
        public static readonly Position Neutron          = Position.Alloc<Star>((int) Stage.Neutron);
        
        [Position(3_599_999_650, (int) Stage.BlackHole)]
        [LateralConstraint(GALAXY_KEY, typeof(Galaxy), (int) Galaxy.Stage.YOUNG, (int) Galaxy.Stage.MIDDLE, (int) Galaxy.Stage.OLD, (int) Galaxy.Stage.DEGENERATE)]
        public static readonly Position BlackHole        = Position.Alloc<Star>((int) Stage.BlackHole);

        public Star(Galaxy galaxy) {
            SetParent(GALAXY_KEY, galaxy);
        }

        public override Position InitialSuperPosition() {
            return PreExistence;
        }

        public override Position TerminalSuperPosition() {
            return WhiteDwarf | Neutron | BlackHole;
        }

        public enum Stage {
            PreExistent,
            Proto,
            MainSequence,
            Massive,
            RedGiant,
            RedSuperGiant,
            WhiteDwarf,
            Neutron,
            BlackHole,
            PlanetaryNebula,
            Supernova       
            
            
        }
    }
}