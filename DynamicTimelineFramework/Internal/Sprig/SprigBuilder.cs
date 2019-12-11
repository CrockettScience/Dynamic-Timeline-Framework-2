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

        public Multiverse Owner;
        
        public SprigBuilder(Diff rootDiff, Universe rootUniverse)
        {
            Spine = new Spine(rootDiff, rootUniverse);
            rootUniverse.Sprig.Builder = this;
            IndexedSpace = 0;
            Owner = rootUniverse.Owner;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig = Spine.AddBranch(diff.GetDiffChain());

            newSprig.Builder = this;
            
            diff.InstallChanges(newSprig);
            
            return newSprig;
        }

        public OperativeSlice RegisterObject(DTFObject obj)
        {
            //Allocate space for every sprig in the tree
            var objDef = (DTFObjectDefinitionAttribute) obj.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            var space = objDef.PositionCount;

            var leftBound = IndexedSpace;
            IndexedSpace += space;
            
            Spine.Alloc(space, leftBound);
            
            Registry.Add(obj);
            
            return new OperativeSlice(leftBound, IndexedSpace);

        }
    }
}