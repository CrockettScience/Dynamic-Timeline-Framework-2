using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Sprig;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        private Dictionary<Diff, Sprig> _sprigs;

        private Dictionary<Type, LateralRuleset> _lateralRulesets;

        private Dictionary<Position, Position> _forwardRuleset;

        internal Sprig this[Diff diff] => _sprigs[diff]; 
        
    }
}