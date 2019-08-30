using System;
using DynamicTimelineFramework.Multiverse;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse("TEST");
            var universe = multiverse.BaseUniverse;
            
            var galaxy = new Galaxy();
            var star = new Star(galaxy);

            var starContinuity = universe.GetContinuity(star);
            
            //Get the position at the beginning and end of the universe;
            var bigBangPosition = starContinuity[0];
            var heatDeathPosition = starContinuity[ulong.MaxValue];
            
            
        }
    }
}