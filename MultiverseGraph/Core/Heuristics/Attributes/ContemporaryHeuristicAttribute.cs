using System;

namespace MultiverseGraph.Core.Heuristics.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ContemporaryHeuristicAttribute : Attribute{
        public Position Heuristic { get;}
        public string Identifier { get; }

        public ContemporaryHeuristicAttribute(string identifier, params string[] values)
        {
            var csv = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                csv += "," + values[i];
            }
            
            Heuristic = new Position(csv);
            Identifier = identifier;
        }
    }
}