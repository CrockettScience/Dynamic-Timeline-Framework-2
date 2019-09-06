using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test {
    //Period Length: 10 million years
    [DTFObjectDefinition(1, "TEST")]
    public class Star : DTFObject {
        internal const long PRE_EXISTENT     = 1;
        internal const long PROTO            = 1 << 1;
        internal const long MAIN_SEQUENCE    = 1 << 2;
        internal const long MASSIVE          = 1 << 3;
        internal const long RED_GIANT        = 1 << 4;
        internal const long RED_SUPER_GIANT  = 1 << 5;
        internal const long WHITE_DWARF      = 1 << 6;
        internal const long NEUTRON          = 1 << 7;
        internal const long BLACK_HOLE       = 1 << 8;
        internal const long PLANETARY_NEBULA = 1 << 9;
        internal const long SUPERNOVA        = 1 << 10;

        private const string GALAXY_KEY = "Galaxy";
        
        [Position(3_600_000_000, PROTO | PRE_EXISTENT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.PRE_EXISTENT | Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.PROTO_GALAXY)]
        public static readonly Position PreExistence     = Position.Alloc<Star>(PRE_EXISTENT);
        
        [Position(3_600_000_000, MAIN_SEQUENCE | MASSIVE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.PROTO_GALAXY)]
        public static readonly Position Proto            = Position.Alloc<Star>(PROTO);
        
        [Position(3_600_000_000, RED_GIANT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position MainSequence     = Position.Alloc<Star>(MAIN_SEQUENCE);
        
        [Position(3_600_000_000, RED_SUPER_GIANT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position Massive          = Position.Alloc<Star>(MASSIVE);
        
        [Position(3_600_000_000, PLANETARY_NEBULA)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position RedGiant         = Position.Alloc<Star>(RED_GIANT);
        
        [Position(3_600_000_000, SUPERNOVA)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position RedSuperGiant    = Position.Alloc<Star>(RED_SUPER_GIANT);
        
        [Position(3_600_000_000, WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position PlanetaryNebula  = Position.Alloc<Star>(PLANETARY_NEBULA);
        
        [Position(3_600_000_000, WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static readonly Position WhiteDwarf       = Position.Alloc<Star>(WHITE_DWARF);
        
        [Position(350, NEUTRON | BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static readonly Position Supernova    = Position.Alloc<Star>(SUPERNOVA);
        
        [Position(3_599_999_650, NEUTRON)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static readonly Position Neutron          = Position.Alloc<Star>(NEUTRON);
        
        [Position(3_599_999_650, BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static readonly Position BlackHole        = Position.Alloc<Star>(BLACK_HOLE);

        public Star(Galaxy galaxy) {
            SetParent(GALAXY_KEY, galaxy);
        }

        public override Position InitialSuperPosition() {
            return PreExistence;
        }

        public override Position TerminalSuperPosition() {
            return WhiteDwarf | Neutron | BlackHole;
        }
    }
}