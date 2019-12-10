using System;
using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse();
            var universe = multiverse.BaseUniverse;
            
            var galaxy = new Galaxy(multiverse);
            var star = new Star(galaxy, multiverse);

            var galContinuity = universe.GetContinuity(galaxy);
            var starContinuity = universe.GetContinuity(star);
            
            //Get the position at the beginning and end of the universe;
            var bigBangPosition = starContinuity[0];
            var heatDeathPosition = starContinuity[ulong.MaxValue];
            
            //Collapse the position
            starContinuity.Constrain(0, Star.PreExistence, out var d0);
            bigBangPosition = starContinuity[0];
            
            starContinuity.Constrain(ulong.MaxValue, Star.BlackHole | Star.Neutron | Star.WhiteDwarf, out var d1);
            heatDeathPosition = starContinuity[ulong.MaxValue];
            
            var galBigBangPosition = galContinuity[0];
            var galHeatDeathPosition = galContinuity[0];
            
            

        }
    }
}