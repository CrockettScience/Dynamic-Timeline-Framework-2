using System.Collections.Generic;
using DynamicTimelineFramework.Core;

namespace DynamicTimelineFramework.Objects {
    public class LateralRuleset
    {
        private readonly Dictionary<Position, Position> _ruleset;

        public LateralRuleset(Dictionary<Position, Position> ruleset)
        {
            _ruleset = ruleset;
        }

        internal Position Translate(Position input)
        {
            return _ruleset[input];
        }
    }
}