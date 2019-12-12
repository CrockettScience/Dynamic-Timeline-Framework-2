using DynamicTimelineFramework.Core;

namespace Test
{
    internal class Program
    {
        public static void Main(string[] args) {
            var multiverse = new Multiverse();
            var universe = multiverse.BaseUniverse;
            
            var galaxy = new Galaxy(multiverse);
            for (var i = 0; i < 1000; i++)
            {
                var star = new Star(galaxy, multiverse);

                var rootStarContinuity = universe.GetContinuity(star);

                //Test branching multiverse
                rootStarContinuity.Constrain(10_000_000_000, Star.Proto, out _);
                rootStarContinuity.Constrain(11_000_000_000, Star.MainSequence, out _);
                rootStarContinuity.Constrain(11_000_000_000, Star.Massive, out var massiveStarDiff);

                var massiveStarUniverse = new Universe(massiveStarDiff);
                var massiveStarContinuity = massiveStarUniverse.GetContinuity(star);

                massiveStarContinuity.Constrain(13_000_000_351, Star.BlackHole, out _);
                massiveStarContinuity.Constrain(13_000_000_351, Star.Neutron, out var neutronStarDiff);

                var neutronStarUniverse = new Universe(neutronStarDiff);
            }

            var a = 10;
        }
    }
}