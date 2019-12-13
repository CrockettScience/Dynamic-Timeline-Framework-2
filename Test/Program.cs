using System;
using System.Collections.Generic;
using System.Diagnostics;
using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse(false);
            var universe = multiverse.BaseUniverse;
            
            var galaxy = new Galaxy(multiverse);

            /*Test branching multiverse
            rootStarContinuity.Constrain(10_000_000_000, Star.Proto, out _);
            rootStarContinuity.Constrain(10_999_999_999, Star.Proto, out _);
            rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out _);
            rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);
            
            var massiveStarUniverse = new Universe(massiveStarDiff);
            var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

            massiveStarContinuity.Constrain(13_000_000_351, Star.BlackHole, out _);
            massiveStarContinuity.Constrain(13_000_000_351, Star.Neutron, out var neutronStarDiff);
            
            var neutronStarUniverse = new Universe(neutronStarDiff);
            var neutronStarContinuity = neutronStarUniverse.GetContinuity(star);

            var testPosition = rootStarContinuity[14_000_000_001];   //White Dwarf
            testPosition = rootStarContinuity[13_000_000_351];       //Planetary Nebula
            testPosition = massiveStarContinuity[13_000_000_351];    //Black Hole
            testPosition = neutronStarContinuity[13_000_000_351];    //Neutron Star
            */
        }

        public static void Constrain_X_ObjectCount(int count, Stopwatch timer, Galaxy galaxy, Multiverse multiverse) {
            
            
            for (var i = 1; i < count; i++) {
                var star = new Star(galaxy, multiverse);
                var rootStarContinuity = multiverse.BaseUniverse.GetContinuity(star);
                
                timer.Restart();
                rootStarContinuity.Constrain(10_000_000_000, Star.MainSequence, out _);
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
            
                var massiveStarUniverse = new Universe(massiveStarDiff);
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
            
                var massiveStarUniverse = new Universe(massiveStarDiff);
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
                var massiveStarUniverse = new Universe(massiveStarDiff);
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
                var massiveStarUniverse = new Universe(massiveStarDiff);
                Console.WriteLine(timer.ElapsedMilliseconds);
            }
        }
    }
}