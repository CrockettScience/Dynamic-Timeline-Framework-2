using MultiverseGraph.Core.Heuristics.Attributes;

namespace MultiverseGraph.Core.Debug
{
    public class StarHandle : Handle
    {
        private const string _nonexistent     = "0x0000";
        private const string _proto           = "0x0004";
        private const string _mainSequence    = "0x0008";
        private const string _massive         = "0x0010";
        private const string _redGiant        = "0x0020";
        private const string _redSuperGiant   = "0x0040";
        private const string _whiteDwarf      = "0x0080";
        private const string _neutron         = "0x0100";
        private const string _blackHole       = "0x0200";
        private const string _planetaryNebula = "0x0400";
        private const string _blackDwarf      = "0x0800";

        public StarHandle(GalaxyHandle galaxy)
        {
            
        }
        
        [FutureHeuristic(_nonexistent, _proto)]
        public static readonly Position Nonexistent     = new Position(_nonexistent);
        
        [FutureHeuristic(_massive, _mainSequence)]
        public static readonly Position Proto           = new Position(_proto);
        
        [FutureHeuristic(_redGiant)]
        public static readonly Position MainSequence    = new Position(_mainSequence);
        
        [FutureHeuristic(_redSuperGiant)]
        public static readonly Position Massive         = new Position(_massive);
        
        [FutureHeuristic(_planetaryNebula)]
        public static readonly Position RedGiant        = new Position(_redGiant);
        
        [FutureHeuristic(_neutron, _blackHole)]
        public static readonly Position RedSuperGiant   = new Position(_redSuperGiant);
        
        [FutureHeuristic(_blackDwarf)]
        public static readonly Position WhiteDwarf      = new Position(_whiteDwarf);
        
        [FutureHeuristic(_neutron)]
        public static readonly Position Neutron         = new Position(_neutron);
        
        [FutureHeuristic(_blackHole)]
        public static readonly Position BlackHole       = new Position(_blackHole);
        
        [FutureHeuristic(_whiteDwarf)]
        public static readonly Position PlanetaryNebula = new Position(_planetaryNebula);
        
        [FutureHeuristic(_blackDwarf)]
        public static readonly Position BlackDwarf      = new Position(_blackDwarf);
        
        
        
        protected override Position BigBangPosition()
        {
            return Nonexistent;
        }

        protected override Position HeatDeathPosition()
        {
            return BlackHole | BlackDwarf | Neutron;
        }
    }
}