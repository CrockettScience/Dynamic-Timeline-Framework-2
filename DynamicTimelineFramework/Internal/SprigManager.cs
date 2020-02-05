using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal {
    internal class SprigManager {
        public Spine Spine { get; }

        public int ObjectCount { get; private set; }
        public int UniverseCount { get; private set; }

        public readonly Multiverse Owner;
        
        public SprigManager(Diff rootDiff, Universe rootUniverse)
        {
            Spine = new Spine(rootDiff, rootUniverse);
            rootUniverse.Sprig.Manager = this;
            Owner = rootUniverse.Multiverse;
            UniverseCount++;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig = Spine.AddBranch(diff.GetDiffChain());

            newSprig.Manager = this;
            
            diff.InstallChanges(newSprig);

            UniverseCount++;
            
            return newSprig;
        }

        public void RegisterObject(DTFObject obj, out int referenceHash)
        {
            //Root the object in the root universe
            Owner.BaseUniverse.RootObject(obj);
            
            ObjectCount++;

            referenceHash = ObjectCount ^ 397;
            
            Owner.ClearPendingDiffs();

        }

        public void UnregisterDiff(Diff diff) {
            
            //Remove the branch from the spine
            Spine.RemoveBranch(diff.GetDiffChain());

            UniverseCount--;
        }
    }
}