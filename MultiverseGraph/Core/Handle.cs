using System;
using System.Collections.Generic;

namespace MultiverseGraph.Core
{
    public abstract class Handle
    {
        private Dictionary<Diff, PositionRange> HeatDeaths;

        protected Handle()
        {
            
        }

        protected abstract Position BigBangPosition();
        protected abstract Position HeatDeathPosition();

        internal void Collapse(ulong date, Random rand)
        {
            
        }

        private class PositionRange
        {
            public ulong StartDate;
            public Position Position;

            public PositionRange Previous;
        }
    }
}