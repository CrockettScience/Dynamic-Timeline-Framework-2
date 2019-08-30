using DynamicTimelineFramework.Multiverse;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace Test
{
    [DTFObjectDefinition(1, "TEST")]
    public class Galaxy : DTFObject {

        internal const long PRE_EXISTENT = 1;
        internal const long PROTO_GALAXY = 1 << 1;
        internal const long FORMED =       1 << 2;
        internal const long DEGENERATE =   1 << 3;
        
        [ForwardPosition(PRE_EXISTENT | PROTO_GALAXY)]
        public static readonly Position<Galaxy> PreExistent = Position<Galaxy>.Alloc(PRE_EXISTENT);
        
        [ForwardPosition(FORMED)]
        public static Position<Galaxy> ProtoGalaxy = Position<Galaxy>.Alloc(PROTO_GALAXY);
        
        [ForwardPosition(DEGENERATE)]
        public static Position<Galaxy> Formed =      Position<Galaxy>.Alloc(FORMED);
        
        [ForwardPosition(DEGENERATE)]
        public static Position<Galaxy> Degenerate =  Position<Galaxy>.Alloc(DEGENERATE);
    }
}