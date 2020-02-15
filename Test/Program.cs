using System;
using System.Diagnostics;
using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse(true);
            
            var galaxy = new Galaxy(multiverse);
            
            var rootGalaxyContinuity = multiverse.BaseUniverse.GetContinuity(galaxy);

            Console.WindowWidth = 256;
            
            //Galaxy in full superposition
            Console.WriteLine("Before Star Constraints:");
            Console.WriteLine("Time 200 - " + rootGalaxyContinuity[200]);
            
            //Collapse Galaxy
            rootGalaxyContinuity.Constrain(10, Galaxy.Birth, out _);
            rootGalaxyContinuity.Constrain(5, Galaxy.Birth, out var earlyBirthDiff);
            var ebUniverse = new Universe(earlyBirthDiff);
            
            var star = new Star(galaxy, multiverse);
            var ebStarContinuity = ebUniverse.GetContinuity(star);

            //Test branching multiverse
            ebStarContinuity.Constrain(200, Star.MainSequence, out _);
            ebStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
            var massiveStarUniverse = new Universe(massiveStarDiff);
            var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

            massiveStarContinuity.Constrain(220, Star.Supernova, out _);
            massiveStarContinuity.Constrain(226, Star.BlackHole, out _);
            massiveStarContinuity.Constrain(226, Star.Neutron, out var neutronStarDiff);
            
            var neutronStarUniverse = new Universe(neutronStarDiff);
            var neutronStarContinuity = neutronStarUniverse.GetContinuity(star);
            
            
            Console.WriteLine();
            Console.WriteLine("After Star Constraints:");
            Console.WriteLine("Time 200 - " + rootGalaxyContinuity[200]);
            Console.WriteLine("Time 200 - " + ebStarContinuity[200]);       //Main Sequence
            Console.WriteLine("Time 220 - " + ebStarContinuity[220]);       //Planetary Nebula
            Console.WriteLine("Time 240 - " + ebStarContinuity[240]);       //White Dwarf
            
            Console.WriteLine();
            Console.WriteLine("Parallel Universe where the star became a massive star, and went supernova instead of burning off in a nebula, and ended up as a black hole:");
            Console.WriteLine("Time 200 - " + massiveStarContinuity[200]);    //Massive
            Console.WriteLine("Time 220 - " + massiveStarContinuity[220]);    //Supernova
            Console.WriteLine("Time 230 - " + massiveStarContinuity[230]);    //Black Hole
            
            Console.WriteLine();
            Console.WriteLine("Parallel Universe where the star also became a massive star and went supernova, but instead devolved into a Neutron Star:");
            Console.WriteLine("Time 200 - " + neutronStarContinuity[200]);    //Massive
            Console.WriteLine("Time 220 - " + neutronStarContinuity[220]);    //Supernova
            Console.WriteLine("Time 230 - " + neutronStarContinuity[230]);    //Neutron Star

        }

        public static void Constrain_X_ObjectCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            
            
            for (var i = 1; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                timer.Restart();
                rootStarContinuity.Select(200);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }

        public static void Constrain_X_UniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            var star = new Star(galaxy, multiverse);
            var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
            
            for (var i = 0; i < count; i++) {
                
                timer.Restart();
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                rootStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
                new Universe(massiveStarDiff);
            }
        }

        public static void Constrain_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                timer.Restart();
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                rootStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
                new Universe(massiveStarDiff);
            }
        }

        public static void CreateObject_X_ObjectCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            
            
            for (var i = 1; i < count; i++) {
                timer.Restart();
                var star = new Star(galaxy, multiverse);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
            }
        }

        public static void CreateObject_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                timer.Restart();
                var star = new Star(galaxy, multiverse);
                Console.WriteLine(timer.ElapsedMilliseconds);
                
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
                rootStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
                var massiveStarUniverse = new Universe(massiveStarDiff);
            }
        }

        public static void CreateUniverse_X_UniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            var star = new Star(galaxy, multiverse);
            var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
            
            for (var i = 0; i < count; i++) {
                
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
                rootStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
                timer.Restart();
                var a = new Universe(massiveStarDiff);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }

        public static void CreateUniverse_X_ObjectAndUniverseCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {

            for (var i = 0; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                rootStarContinuity.Constrain(200, Star.MainSequence, out _);
                rootStarContinuity.Constrain(200, Star.Massive, out var massiveStarDiff);
            
                timer.Restart();
                new Universe(massiveStarDiff);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }
    }
}