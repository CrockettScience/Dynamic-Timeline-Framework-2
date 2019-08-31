using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Multiverse
{
    public class Multiverse
    {
        private readonly Dictionary<Diff, Universe> _multiverse;
        
        internal ObjectCompiler Compiler { get; }
        
        public Universe BaseUniverse { get; }

        /// <summary>
        /// Instantiates a new multiverse with a blank base universe and compiles the system of objects
        /// the share the identifier key in the DTFObjectDefinition attribute
        /// </summary>
        /// <param name="identifierKey">The key to identify objects the object compiler will look for</param>
        public Multiverse(string identifierKey)
        {
            // Set up the big bang date node
            BaseUniverse = new Universe(this);

            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
            //Run the Compiler
            Compiler = new ObjectCompiler(identifierKey, Assembly.GetCallingAssembly());
            
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
        
        internal class ObjectCompiler
        {
            private static readonly Type DTFObjectType = typeof(DTFObject);
            
            private static readonly Type DTFObjectDefAttr = typeof(DTFObjectDefinitionAttribute);
            private static readonly Type ForwardPositionAttr = typeof(ForwardPositionAttribute);
            private static readonly Type LateralObjectAttr = typeof(LateralObjectAttribute);
            private static readonly Type LateralPositionAttr = typeof(LateralPositionAttribute);
            
            public ObjectCompiler(string mvIdentifierKey, Assembly callingAssembly)
            {
                var types = callingAssembly.GetTypes();
                
                //Go through the types and only work with the types we care about
                foreach (var type in types)
                {
                    if (!type.IsAssignableFrom(typeof(DTFObject))) continue;

                    if (!(type.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute definitionAttr))
                        throw new DTFObjectCompilerException(type.Name + " is assignable from " + DTFObjectType.Name + " but does not have a " + DTFObjectDefAttr.Name + " defined.");

                    if (definitionAttr.MvIdentifierKey != mvIdentifierKey) continue;
                    
                    //This is a type we are looking for. Now iterate though each field and build the database
                    
                    //This position type represents the run time type of a position from this DTFObject
                    var positionType = typeof(Position<>).MakeGenericType(type);
                    foreach (var field in type.GetFields())
                    {
                        var forwardPositionAttribute = field.GetCustomAttribute(ForwardPositionAttr) as ForwardPositionAttribute;
                        var lateralPositionAttributes = field.GetCustomAttributes(LateralPositionAttr) as LateralPositionAttribute[];
                        var lateralObjectAttribute = field.GetCustomAttribute(LateralObjectAttr) as LateralObjectAttribute;

                        //Perform a series of run time checks, then add them to some preliminary structures
                        var fieldType = field.FieldType;
                        
                        if (lateralObjectAttribute != null)
                        {
                            //The field must not be static
                            if (field.IsStatic)
                                throw new DTFObjectCompilerException(fieldType.Name + " references a lateral object that is in a static field");
                            
                            //A lateral object must be assignable from DTFObject
                            if (!fieldType.IsAssignableFrom(DTFObjectType))
                                throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + DTFObjectType.Name + ", and cannot be attributed as a lateral object by " + type.Name);
                            
                            //The class must also have a DTFObjectDefinitionAttribute
                            if (!(fieldType.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute lateralAttr))
                                throw new DTFObjectCompilerException(fieldType.Name + " is assignable from " + DTFObjectType.Name + " but does not have a " + DTFObjectDefAttr.Name + " defined");

                            //The object must share the same mvIdentifierKey
                            if(lateralAttr.MvIdentifierKey != mvIdentifierKey)
                                throw new DTFObjectCompilerException(type.Name + " references a " + fieldType.Name + " as a lateral object, but identifier key \"" + lateralAttr.MvIdentifierKey + "\" does not math identifier key \"" + mvIdentifierKey + "\"");
                            
                            //Todo - key in the object
                            
                        }

                        else if (lateralPositionAttributes != null)
                        {
                            //The field must be static
                            if (!field.IsStatic)
                                throw new DTFObjectCompilerException("All of " + fieldType.Name + "\'s attributed positions must be static");
                            
                            //The field must be assignable from Position<Type>
                            if (!fieldType.IsAssignableFrom(positionType))
                                throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);

                            //If it has a lateralPositionAttribute, then it must have a forwardPositionAttribute as well
                            if (forwardPositionAttribute == null)
                                throw new DTFObjectCompilerException(field.Name + " of type " + type.Name + " does not have a ForwardPositionAttribute");

                            //Todo - key in the Position to both the associated lateral object and the forward object
                        }
                        
                        else if (forwardPositionAttribute != null)
                        {
                            //The field must be static
                            if (!field.IsStatic)
                                throw new DTFObjectCompilerException("All of " + fieldType.Name + "\'s attributed positions must be static");
                            
                            //The field must be assignable from Position<Type>
                            if (!fieldType.IsAssignableFrom(positionType))
                                throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);
                            
                            //Todo - key in the position to the forward object
                        }
                    }
                }
            }

            internal SprigVector<T> GetNormalizedVector<T>(Position<T> position, ulong date) where T : DTFObject
            {
                //Todo - Get normalized vector
                return null;
            }

            internal Position<T> GetPosition<T>(T dtfObject, ulong date) where T : DTFObject
            {
                //Todo - Recursively retrieve parent state to constrain position, if one exists
                return null;
            }
        }
    }
}