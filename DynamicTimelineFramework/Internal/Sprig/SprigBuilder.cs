using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigBuilder {
        public Spine Spine { get; }
        public int IndexedSpace { get; private set; }
        
        public readonly List<DTFObject> Registry = new List<DTFObject>();
        
        public SprigBuilder(Diff rootDiff, Universe rootUniverse)
        {
            Spine = new Spine(rootDiff, rootUniverse);
            rootUniverse.Sprig.Builder = this;
            IndexedSpace = 0;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig = Spine.AddBranch(diff.GetDiffChain());
            
            diff.InstallChanges(newSprig);

            newSprig.Builder = this;
            
            return newSprig;
        }

        public Slice RegisterObject(DTFObject obj)
        {
            //Allocate space for every sprig in the tree
            var objDef = (DTFObjectDefinitionAttribute) obj.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            var space = objDef.PositionCount;

            var leftBound = IndexedSpace;
            IndexedSpace += space;
            
            Spine.Alloc(space, leftBound);
            
            Registry.Add(obj);
            
            return new Slice(leftBound, IndexedSpace);

        }
    }
}