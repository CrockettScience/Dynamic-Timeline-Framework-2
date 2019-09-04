using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test {
    //Period Length: 10 million years
    [DTFObjectDefinition(1, 3_600_000_000, "TEST")]
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

        private const string GALAXY_KEY = "Galaxy";
        
        [ForwardPosition(PROTO | PRE_EXISTENT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.PRE_EXISTENT | Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.PROTO_GALAXY)]
        public static Position PreExistence     = Position.Alloc<Star>(PRE_EXISTENT);
        
        [ForwardPosition(MAIN_SEQUENCE | MASSIVE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.PROTO_GALAXY)]
        public static Position Proto            = Position.Alloc<Star>(PROTO);
        
        [ForwardPosition(RED_GIANT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static Position MainSequence     = Position.Alloc<Star>(MAIN_SEQUENCE);
        
        [ForwardPosition(RED_SUPER_GIANT)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static Position Massive          = Position.Alloc<Star>(MASSIVE);
        
        [ForwardPosition(PLANETARY_NEBULA)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static Position RedGiant         = Position.Alloc<Star>(RED_GIANT);
        
        [ForwardPosition(NEUTRON | BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static Position RedSuperGiant    = Position.Alloc<Star>(RED_SUPER_GIANT);
        
        [ForwardPosition(WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD)]
        public static Position PlanetaryNebula  = Position.Alloc<Star>(WHITE_DWARF);
        
        [ForwardPosition(WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static Position WhiteDwarf       = Position.Alloc<Star>(NEUTRON);
        
        [ForwardPosition(NEUTRON)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static Position Neutron          = Position.Alloc<Star>(BLACK_HOLE);
        
        [ForwardPosition(BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, typeof(Galaxy), Galaxy.YOUNG | Galaxy.MIDDLE | Galaxy.OLD | Galaxy.DEGENERATE)]
        public static Position BlackHole        = Position.Alloc<Star>(PLANETARY_NEBULA);

        public Star(Galaxy galaxy) {
            SetParent(GALAXY_KEY, galaxy);
        }
    }
}