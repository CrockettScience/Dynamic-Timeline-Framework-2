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
            var star = new Star(galaxy, multiverse);
            
            var rootStarContinuity = universe.GetContinuity(star);
            rootStarContinuity.Constrain(1001, Star.Birth, out _);

            
            var timer = new Stopwatch();

            for (ulong i = 100; i > 0; i--)
            {
                timer.Start();

                //Test branching multiverse
                rootStarContinuity.Constrain(i, Star.Birth, out var massiveStarDiff);

                var massiveStarUniverse = new Universe(massiveStarDiff);
                
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }
    }
}