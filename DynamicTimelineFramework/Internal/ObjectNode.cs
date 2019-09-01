using System.Collections.Generic;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal
{
    internal class ObjectNode
    {
        private readonly Map<string, ObjectNode> _edges = new Map<string, ObjectNode>();

        public DTFObject DTFObject { get; }

        public ObjectNode(DTFObject dtfObject)
        {
            DTFObject = dtfObject;
        }

        public ObjectNode this[string key]
        {
            get => _edges[key];

            set => _edges[key] = value;
        }
        
        
    }
}