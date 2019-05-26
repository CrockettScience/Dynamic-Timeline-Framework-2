
using System.Collections;
using System.Collections.Generic;

namespace MultiverseGraph.Core
{
    public class Multiverse
    {
        private readonly Dictionary<Diff, Universe> _multiverse;
        public Universe BaseUniverse { get; }

        public Multiverse()
        {
            // Set up the big bang date node
            BaseUniverse = new Universe();

            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
        }

        public Universe this[Diff diff]
        {
            get
            {
                //The indexer works as an always-true accessor; it either gets whats in the dictionary or creates the entry
                if(!_multiverse.ContainsKey(diff))
                {
                    _multiverse[diff] = new Universe(diff);
                }

                return _multiverse[diff];
            }
        }
    }
}