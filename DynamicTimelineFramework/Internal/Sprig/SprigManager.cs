using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal.Sprig {
    internal class SprigManager {
        public Spine Spine { get; }
        public int BitCount { get; private set; }
        
        public readonly List<DTFObject> Registry = new List<DTFObject>();

        public Multiverse Owner;
        
        public SprigManager(Diff rootDiff, Universe rootUniverse)
        {
            Spine = new Spine(rootDiff, rootUniverse);
            rootUniverse.Sprig.Manager = this;
            BitCount = 0;
            Owner = rootUniverse.Owner;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig = Spine.AddBranch(diff.GetDiffChain());

            newSprig.Manager = this;
            
            diff.InstallChanges(newSprig);
            
            return newSprig;
        }

        public OperativeSlice RegisterObject(DTFObject obj, out int referenceHash)
        {
            //Allocate space for every sprig in the tree
            var objDef = (DTFObjectDefinitionAttribute) obj.GetType().GetCustomAttribute(typeof(DTFObjectDefinitionAttribute));
            var space = objDef.PositionCount;

            var leftBound = BitCount;
            BitCount += space;
            
            Spine.Alloc(space, leftBound);
            
            Registry.Add(obj);

            referenceHash = Registry.Count ^ 397;
            
            Owner.ClearPendingDiffs();

            return new OperativeSlice(leftBound, BitCount);

        }
    }
}