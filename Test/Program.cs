using System;
using System.Diagnostics;
using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse(false);
            
            var galaxy = new Galaxy(multiverse);
            var timer = new Stopwatch();
            
            //Constrain_X_ObjectAndUniverseCount(50, timer, galaxy, multiverse);
            var star = new Star(galaxy, multiverse);
            var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
            var rootGalaxyContinuity = multiverse.BaseUniverse.GetContinuity(galaxy);

            Console.WindowWidth = 256;
            
            //Galaxy in full superposition
            Console.WriteLine("Before Star Constraints:");
            Console.WriteLine("Year 11,000,000,000 - " + rootGalaxyContinuity[11_000_000_000]);

            //Test branching multiverse
            rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out _);
            rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);
            
            var massiveStarUniverse = new Universe(massiveStarDiff);
            var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

            massiveStarContinuity.Constrain(13_000_000_000, Star.Supernova, out _);
            massiveStarContinuity.Constrain(13_000_000_351, Star.BlackHole, out _);
            massiveStarContinuity.Constrain(13_000_000_351, Star.Neutron, out var neutronStarDiff);
            
            var neutronStarUniverse = new Universe(neutronStarDiff);
            var neutronStarContinuity = neutronStarUniverse.GetContinuity(star);
            
            //Even though the galaxy was never directly constrained, there WAS a change in
            //certainty; We know it exists now and has not degenerated, because at least 1 star
            //exist in it
            Console.WriteLine();
            Console.WriteLine("After Star Constraints:");
            Console.WriteLine("Year 11,000,000,000 - " + rootGalaxyContinuity[11_000_000_000]);
            Console.WriteLine("Year 11,000,000,000 - " + rootStarContinuity[11_000_000_000]);       //Main Sequence
            Console.WriteLine("Year 13,000,000,000 - " + rootStarContinuity[13_000_000_000]);       //Planetary Nebula
            Console.WriteLine("Year 15,000,000,000 - " + rootStarContinuity[15_000_000_000]);       //White Dwarf
            
            Console.WriteLine();
            Console.WriteLine("Parallel Universe where the star became a massive star, and went supernova instead of burning off in a nebula, and ended up as a black hole:");
            Console.WriteLine("Year 11,000,000,000 - " + massiveStarContinuity[11_000_000_000]);    //Massive
            Console.WriteLine("Year 13,000,000,000 - " + massiveStarContinuity[13_000_000_000]);    //Supernova
            Console.WriteLine("Year 14,000,000,000 - " + massiveStarContinuity[14_000_000_000]);    //Black Hole
            
            Console.WriteLine();
            Console.WriteLine("Parallel Universe where the star also became a massive star and went supernova, but instead devolved into a Neutron Star:");
            Console.WriteLine("Year 11,000,000,000 - " + neutronStarContinuity[11_000_000_000]);    //Massive
            Console.WriteLine("Year 13,000,000,000 - " + neutronStarContinuity[13_000_000_000]);    //Supernova
            Console.WriteLine("Year 14,000,000,000 - " + neutronStarContinuity[14_000_000_000]);    //Neutron Star
            
        }

        public static void Constrain_X_ObjectCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            
            
            for (var i = 1; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                timer.Restart();
                rootStarContinuity.Select(10_000_000_000);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }

        public static void Constrain_X_UniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            var star = new Star(galaxy, multiverse);
            var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
            
            for (var i = 0; i < count; i++) {
                
                timer.Restart();
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                rootStarContinuity.Constrain(10_000_000_000, Star.Massive, out var massiveStarDiff);
            
                new Universe(massiveStarDiff);
            }
        }

        public static void Constrain_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                timer.Restart();
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                rootStarContinuity.Constrain(10_000_000_000, Star.Massive, out var massiveStarDiff);
            
                new Universe(massiveStarDiff);
            }
        }

        public static void CreateObject_X_ObjectCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            
            
            for (var i = 1; i < count; i++) {
                timer.Restart();
                var star = new Star(galaxy, multiverse);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
            }
        }

        public static void CreateObject_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                timer.Restart();
                var star = new Star(galaxy, multiverse);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
                rootStarContinuity.Constrain(10_000_000_000, Star.Massive, out var massiveStarDiff);
            
                var massiveStarUniverse = new Universe(massiveStarDiff);
            }
        }

        public static void CreateUniverse_X_UniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            var star = new Star(galaxy, multiverse);
            var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
            
            for (var i = 0; i < count; i++) {
                
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
                rootStarContinuity.Constrain(10_000_000_000, Star.Massive, out var massiveStarDiff);
            
                timer.Restart();
                var a = new Universe(massiveStarDiff);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }

        public static void CreateUniverse_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
                rootStarContinuity.Constrain(10_000_000_000, Star.Massive, out var massiveStarDiff);
            
                timer.Restart();
                new Universe(massiveStarDiff);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }
    }
}