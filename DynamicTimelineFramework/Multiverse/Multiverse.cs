using System.Collections.Generic;

namespace DynamicTimelineFramework.Multiverse
{
    public class Multiverse
    {
        private readonly Dictionary<Diff, Universe> _multiverse;
        
        internal MultiverseObjectCompiler Compiler { get; }
        
        public Universe BaseUniverse { get; }

        /// <summary>
        /// Instantiates a new multiverse with a blank base universe and compiles the system of objects
        /// the share the identifier key in the DTFObjectDefinition attribute
        /// </summary>
        /// <param name="identifierKey">The key to identify objects the object compiler will look for</param>
        public Multiverse(string identifierKey)
        {
            // Set up the big bang date node
            BaseUniverse = new Universe();

            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
            //Run the Compiler
            Compiler = new MultiverseObjectCompiler(identifierKey);
            
        }

        public Universe this[Diff diff]
        {
            get
            {
                //The indexer works as an always-valid accessor; it either gets whats in the dictionary or creates the entry
                if(!_multiverse.ContainsKey(diff))
                {
                    _multiverse[diff] = new Universe(diff);
                }

                return _multiverse[diff];
            }
        }
        
        internal class MultiverseObjectCompiler {
            //Todo - make the object compiler
            
            public MultiverseObjectCompiler(string mvIdentifierKey) {
            
            }
        }
    }
}