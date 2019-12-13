using System;
using System.Collections.Generic;
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
            
            //Test branching multiverse
            rootStarContinuity.Constrain(10_000_000_000, Star.Proto, out var d0);
            rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out d0);
            rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);
            
            var massiveStarUniverse = new Universe(massiveStarDiff);
            var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

            massiveStarContinuity.Constrain(13_000_000_351, Star.BlackHole, out d0);
            massiveStarContinuity.Constrain(13_000_000_351, Star.Neutron, out var neutronStarDiff);
            
            var neutronStarUniverse = new Universe(neutronStarDiff);
            var neutronStarContinuity = neutronStarUniverse.GetContinuity(star);

            var testPosition = rootStarContinuity[13_000_000_351];   //White Dwarf
            testPosition = massiveStarContinuity[13_000_000_351];    //Black Hole
            testPosition = neutronStarContinuity[13_000_000_351];    //Neutron Star
        }
    }
}