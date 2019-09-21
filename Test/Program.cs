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

            var starContinuity = universe.GetContinuity(star);
            
            //Get the position at the beginning and end of the universe;
            var bigBangPosition = starContinuity[0];
            var heatDeathPosition = starContinuity[ulong.MaxValue];
            
            
        }
    }
}