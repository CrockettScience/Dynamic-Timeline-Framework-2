using System;
using System.Collections.Generic;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Sprig;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        private Dictionary<Diff, Sprig> _sprigs;

        internal Sprig this[Diff diff] => _sprigs[diff];
    }
}