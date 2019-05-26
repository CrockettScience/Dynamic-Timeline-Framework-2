using MultiverseGraph.Core.Heuristics.Attributes;

namespace MultiverseGraph.Core.Debug
{
    public class GalaxyHandle : Handle
    {
        private const string _nonexistent  = "0x1";
        private const string _elliptical   = "0x2";
        private const string _spiral       = "0x4";
        private const string _barredSpiral = "0x8";
        private const string _degenerate   = "0x10";

        public GalaxyHandle()
        {
            
        }
        
        [FutureHeuristic(_nonexistent, _elliptical)]
        public static readonly Position Nonexistent     = new Position(_nonexistent);
        
        [FutureHeuristic(_spiral, _barredSpiral)]
        public static readonly Position Elliptical      = new Position(_elliptical);
        
        [FutureHeuristic(_degenerate)]
        public static readonly Position Spiral          = new Position(_spiral);
        
        [FutureHeuristic(_degenerate)]
        public static readonly Position BarredSpiral    = new Position(_barredSpiral);
        
        [FutureHeuristic(_degenerate)]
        public static readonly Position Degenerate      = new Position(_degenerate);
        
        protected override Position BigBangPosition()
        {
            return Nonexistent;
        }

        protected override Position HeatDeathPosition()
        {
            return Degenerate;
        }
    }
}