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
            
            var rootStarContinuity = universe.GetContinuity(star);
            
            //Test branching multiverse
            rootStarContinuity.Constrain(10_000_000_000, Star.Proto, out var d0);
            rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out d0);
            rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);
            
            var massiveStarUniverse = new Universe(massiveStarDiff);

            var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

            var testPosition = rootStarContinuity[10_000_000_000];
            testPosition = massiveStarContinuity[10_000_000_000];
            testPosition = rootStarContinuity[11_000_000_000];
            testPosition = massiveStarContinuity[11_000_000_000];

        }
    }
}