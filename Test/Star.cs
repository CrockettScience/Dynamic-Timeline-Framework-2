using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test {
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

        private const string GALAXY_KEY = "Galaxy";

        [LateralObject(GALAXY_KEY, true)]
        public Galaxy Galaxy { get; }
        
        [ForwardPosition(PROTO | PRE_EXISTENT)]
        [LateralPosition(GALAXY_KEY, Galaxy.PRE_EXISTENT | Galaxy.FORMED | Galaxy.PROTO_GALAXY)]
        public static Position<Star> PreExistence     = Position<Star>.Alloc(PRE_EXISTENT    );
        
        [ForwardPosition(MAIN_SEQUENCE | MASSIVE)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED | Galaxy.PROTO_GALAXY)]
        public static Position<Star> Proto            = Position<Star>.Alloc(PROTO           );
        
        [ForwardPosition(RED_GIANT)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED)]
        public static Position<Star> MainSequence     = Position<Star>.Alloc(MAIN_SEQUENCE   );
        
        [ForwardPosition(RED_SUPER_GIANT)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED)]
        public static Position<Star> Massive          = Position<Star>.Alloc(MASSIVE         );
        
        [ForwardPosition(PLANETARY_NEBULA)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED)]
        public static Position<Star> RedGiant         = Position<Star>.Alloc(RED_GIANT       );
        
        [ForwardPosition(NEUTRON | BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED)]
        public static Position<Star> RedSuperGiant    = Position<Star>.Alloc(RED_SUPER_GIANT );
        
        [ForwardPosition(WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED)]
        public static Position<Star> PlanetaryNebula  = Position<Star>.Alloc(WHITE_DWARF     );
        
        [ForwardPosition(WHITE_DWARF)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED | Galaxy.DEGENERATE)]
        public static Position<Star> WhiteDwarf       = Position<Star>.Alloc(NEUTRON         );
        
        [ForwardPosition(NEUTRON)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED | Galaxy.DEGENERATE)]
        public static Position<Star> Neutron          = Position<Star>.Alloc(BLACK_HOLE      );
        
        [ForwardPosition(BLACK_HOLE)]
        [LateralPosition(GALAXY_KEY, Galaxy.FORMED | Galaxy.DEGENERATE)]
        public static Position<Star> BlackHole        = Position<Star>.Alloc(PLANETARY_NEBULA);

        public Star(Galaxy galaxy) {
            Galaxy = galaxy;
        }
    }
}