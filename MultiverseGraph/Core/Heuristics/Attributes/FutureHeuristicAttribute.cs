using System;

namespace MultiverseGraph.Core.Heuristics.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FutureHeuristicAttribute : Attribute
    {
        public Position Heuristic { get;}

        public FutureHeuristicAttribute(params string[] values)
        {
            var csv = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                csv += "," + values[i];
            }
            
            Heuristic = new Position(csv);
        }
    }
}