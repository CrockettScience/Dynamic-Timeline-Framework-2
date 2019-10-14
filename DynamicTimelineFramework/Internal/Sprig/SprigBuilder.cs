using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigBuilder {
        private readonly Spine _spine;
        private int _indexedSpace;
        public SprigBuilder(Diff rootDiff)
        {
            _spine = new Spine(rootDiff);
            _indexedSpace = 0;
        }

        public Sprig GetSprig(Diff diff)
        {
            //Get Diff Chain
            var diffChain = new LinkedList<Diff>();
            var current = diff;

            while (current != null) {
                diffChain.AddFirst(current);
                current = current.Parent.Diff;
            }

            var newSprig =  _spine.AddBranch(diffChain);
            
            diff.InstallChanges(newSprig);

            return newSprig;
        }

        public Slice RegisterObject(DTFObject obj)
        {
            //Allocate space for every sprig in the tree
            var objDef = (DTFObjectDefinitionAttribute) obj.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            var space = objDef.PositionCount;

            var leftBound = _indexedSpace;
            _indexedSpace += space;
            
            _spine.Alloc(space, leftBound);
            
            return new Slice(leftBound, _indexedSpace);

        }
    }
}