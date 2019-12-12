using System;
using System.Diagnostics;
using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse();
            var universe = multiverse.BaseUniverse;
            
            var galaxy = new Galaxy(multiverse);
            var timer = new Stopwatch();

            for (var i = 0; i < 30; i++) {
                timer.Restart();

                var star = new Star(galaxy, multiverse);

                var rootStarContinuity = universe.GetContinuity(star);

                //Test branching multiverse
                rootStarContinuity.Constrain(10_000_000_000, Star.Proto, out var d0);
                rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out d0);
                rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);

                var massiveStarUniverse = new Universe(massiveStarDiff);
                
                Console.WriteLine(timer.ElapsedMilliseconds);
            }


        }
    }
}