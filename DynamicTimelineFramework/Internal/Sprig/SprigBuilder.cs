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
        
        public SprigBuilder(Diff rootDiff, Universe rootUniverse)
        {
            _spine = new Spine(rootDiff, rootUniverse);
            _indexedSpace = 0;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig =  _spine.AddBranch(diff.GetDiffChain());
            
            diff.InstallChanges(newSprig);

            newSprig.Builder = this;
            
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