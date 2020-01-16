using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Internal {
    internal class SprigManager {
        public Spine Spine { get; }
        
        public readonly List<DTFObject> Registry = new List<DTFObject>();

        public readonly Multiverse Owner;
        
        public SprigManager(Diff rootDiff, Universe rootUniverse)
        {
            Spine = new Spine(rootDiff, rootUniverse);
            rootUniverse.Sprig.Manager = this;
            Owner = rootUniverse.Multiverse;
        }

        public Sprig BuildSprig(Diff diff)
        {
            var newSprig = Spine.AddBranch(diff.GetDiffChain());

            newSprig.Manager = this;
            
            diff.InstallChanges(newSprig);
            
            return newSprig;
        }

        public void RegisterObject(DTFObject obj, out int referenceHash)
        {
            //Root the object in the root universe
            Owner.BaseUniverse.RootObject(obj);
            
            Registry.Add(obj);

            referenceHash = Registry.Count ^ 397;
            
            Owner.ClearPendingDiffs();

        }

        public void UnregisterDiff(Diff diff) {
            
            //Remove the branch from the spine
            Spine.RemoveBranch(diff.GetDiffChain());
        }
    }
}