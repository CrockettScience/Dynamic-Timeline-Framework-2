using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
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
            private static readonly Type LateralPositionAttr = typeof(LateralPositionAttribute);
            
            //2^63
            public const ulong SPRIG_CENTER = 9_223_372_036_854_775_808;

            private readonly Map<Type, MetaData> _objectMetaData = new Map<Type, MetaData>();
            
            public ObjectCompiler(string mvIdentifierKey, Assembly callingAssembly)
            {
                var firstPassTypes = callingAssembly.GetTypes();
                var secondPassTypes = new List<Type>();

                string parentKey = null;
                
                //Go through the types and create their metadata
                foreach (var type in firstPassTypes)
                {
                    
                    if (!type.IsAssignableFrom(typeof(DTFObject))) continue;

                    if (!(type.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute definitionAttr))
                        throw new DTFObjectCompilerException(type.Name + " is assignable from " + DTFObjectType.Name + " but does not have a " + DTFObjectDefAttr.Name + " defined.");
                    
                    if (definitionAttr.MvIdentifierKey != mvIdentifierKey) continue;

                    parentKey = CompileObjectTypeFirstPass(mvIdentifierKey, type);
                    
                    //Set up for second pass
                    secondPassTypes.Add(type);
                }
                
                //Modify and link the metadata
                foreach (var type in secondPassTypes)
                {

                    CompileObjectTypeSecondPass(type, parentKey);
                }
            }
            
            private string CompileObjectTypeFirstPass(string mvIdentifierKey, Type type) 
            {
                    string parentKey = null;
                    
                    //This is a type we are looking for. Now iterate though each field and build the metadata
                    var lateralFields = new Map<string, FieldInfo>();
                    var lateralKeys = new HashSet<string>();
                    var lateralTranslation = new Map<string, Map<Position, Position>>();
                    var preComputedSprigs = new Map<Position, SprigVector>();
                    
                    //This position type represents the run time type of a position from this DTFObject
                    var positionType = typeof(Position);
                    
                    foreach (var field in type.GetFields())
                    {
                        //Todo - Refactor to account new refactored way to access lateral objects
                        
                        var forwardPositionAttribute = field.GetCustomAttribute(ForwardPositionAttr) as ForwardPositionAttribute;

                        //Perform a series of run time checks, and add to some preliminary structures
                        var fieldType = field.FieldType;
                        
                        if (lateralObjectAttribute != null)
                        {
                            #region CHECKS
                            
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

                            #endregion

                            string key;
                            
                            //Add a key for the object
                            if (lateralObjectAttribute.Parent)
                            {
                                key = PARENT_KEY;
                                parentKey = lateralObjectAttribute.LateralKey;
                            }
                            
                            else
                                key = lateralObjectAttribute.LateralKey;

                            lateralKeys.Add(key);
                            
                            //Add the fieldInfo for the type
                            lateralFields[key] = field;

                        }
                        
                        if (forwardPositionAttribute != null)
                        {
                            #region CHECKS
                            
                            //The field must be static
                            if (!field.IsStatic)
                                throw new DTFObjectCompilerException("All of " + fieldType.Name + "\'s attributed positions must be static");
                            
                            //The field must be assignable from Position<Type>
                            if (!fieldType.IsAssignableFrom(positionType))
                                throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);
                            
                            #endregion
                            
                            //Todo - Use the forward mapping to compute the sprigVector for the field position as an eigenvalue
                        }
                    }
                    
                    _objectMetaData[type] = new MetaData(lateralFields, lateralKeys, lateralTranslation, preComputedSprigs);

                    return parentKey;
            }
            
            private void CompileObjectTypeSecondPass(Type type, string parentKey)
            {
                
                //This position type represents the run time type of a position from this DTFObject
                var positionType = typeof(Position);
                
                //Get the meta object
                var meta = _objectMetaData[type];
                
                foreach (var field in type.GetFields())
                {
                    var forwardPositionAttribute = field.GetCustomAttribute(ForwardPositionAttr) as ForwardPositionAttribute;
                    var lateralPositionAttributes = field.GetCustomAttributes(LateralPositionAttr) as LateralPositionAttribute[];

                    if (lateralPositionAttributes != null)
                    {
                        var fieldType = field.FieldType;
                        
                        #region CHECKS
                        
                        //The field must be static
                        if (!field.IsStatic)
                            throw new DTFObjectCompilerException("All of " + fieldType.Name + "\'s attributed positions must be static");
                        
                        //The field must be assignable from Position<Type>
                        if (!fieldType.IsAssignableFrom(positionType))
                            throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);

                        //If it has a lateralPositionAttribute, then it must have a forwardPositionAttribute as well
                        if (forwardPositionAttribute == null)
                            throw new DTFObjectCompilerException(field.Name + " of type " + type.Name + " does not have a ForwardPositionAttribute");
                        
                        #endregion

                        var fieldValue = (Position) field.GetValue(null);

                        foreach (var attribute in lateralPositionAttributes)
                        {
                            var key = attribute.LateralKey == parentKey ? PARENT_KEY : attribute.LateralKey;
                            
                            if(!meta.LateralTranslation.ContainsKey(key))
                                meta.LateralTranslation[key] = new Map<Position, Position>();

                            var lateralTranslation = meta.LateralTranslation[key];
                            
                            //Map the positions
                            var attrValue = Position.Alloc(type, attribute.Flag);

                            lateralTranslation[fieldValue] = attrValue;
                            
                            //Map the reverse positions
                            var otherType = meta.LateralFields[key];
                            
                            //We'll need to check to make sure the structure is okay
                            bool hasBackField;

                        }
                    }
                }
            }

            #region FUNCTIONS

            internal SprigVector GetNormalizedVector(Type type, Position position, ulong date) 
            {
                var vector = _objectMetaData[type].GetSprigVector(position);
                    
                //All precomputed sprigVectors are centered at SPRIG_CENTER. We need to shift the values to the correct date
                //We have to dance around straight subtraction for the sake of avoiding overflow
                if (date < SPRIG_CENTER)
                {
                    vector.ShiftBackward(SPRIG_CENTER - date);
                }

                if (date > SPRIG_CENTER)
                {
                    vector.ShiftForward(date - SPRIG_CENTER);
                }

                return vector;
            }

            internal void PushConstraints(DTFObject dtfObj, Diff diff)
            {
                //Get the metadata object
                var meta = _objectMetaData[dtfObj.GetType()];
                
                //Iterate through each key and recursively constrain
                foreach (var key in meta.LateralKeys) {
                    var dest = dtfObj.GetLateralObject(key);
                    if (!Constrain(dtfObj, dest, meta, diff)) continue;
                    
                    //If the constraint results in change, then push it's constraints as well
                    
                    PushConstraints(dest, diff);
                }
                
            }

            internal void PullConstraints(DTFObject dtfObj, Diff diff)
            {
                
                if (dtfObj.GetParent() == null)
                    //It has no parents, no need to pull any constraints
                    return;
                
                //It has a parent, so pull its parent's constraints
                        
                //Get the parent object
                var parent = dtfObj.GetParent();

                PullConstraints(parent, diff);
                
                //Now that we know, after some amount of recursion, that the parent object
                //is fully constrained, we can constrain by it
                
                //Get the meta object for the other object
                var otherMeta = _objectMetaData[parent.GetType()];
                
                //Constrain
                Constrain(parent, dtfObj, otherMeta, diff);
            }

            private static bool Constrain(DTFObject source, DTFObject dest, MetaData meta, Diff diff)
            {
                //Get the sprig
                var sprig = dest.GetSprig(diff);
                
                //Get the translation vector
                //Todo - Refactor to access parent without key
                var translationVector = meta.Translate(key, source.GetSprig(diff).ToVector());
                
                //AND the sprigs together and check if there was a change
                return sprig.And(translationVector);
            }

            #endregion

            private class MetaData
            {
                public HashSet<string> LateralKeys { get; }
                
                public Map<string, Map<Position, Position>> LateralTranslation { get; }
                
                public Map<Position, SprigVector> PreComputedSprigs { get; }
                
                public MetaData(HashSet<string> lateralKeys, Map<string, Map<Position, Position>> lateralTranslation, Map<Position, SprigVector> preComputedSprigs)
                {
                    LateralKeys = lateralKeys;
                    LateralTranslation = lateralTranslation;
                    PreComputedSprigs = preComputedSprigs;
                }

                public Position Translate(string key, Position input)
                {
                    Position output;

                    try
                    {
                        output = LateralTranslation[key][input];
                    }
                    catch (InvalidCastException)
                    {
                        //This means that the key given does not match the type of the lateral object
                        throw new ArgumentException("The given key does not map to the correct lateral type");
                    }

                    return output;
                }

                public SprigVector Translate(string key, SprigVector input)
                {
                    var currentIn = input.Head;
                    var currentOut = new SprigNode(null, Translate(key, currentIn.Position), currentIn.Index);
                    var output = new SprigVector(currentOut);

                    while (currentIn.Last != null)
                    {
                        currentIn = currentIn.Last;
                        
                        currentOut.Last = new SprigNode(null, Translate(key, currentIn.Position), currentIn.Index);

                        currentOut = currentOut.Last;
                    }

                    return output;

                }

                public SprigVector GetSprigVector(Position position)
                {
                    var eigenValues = position.GetEigenValues();
                    var vector = new SprigVector(position.Type);

                    return eigenValues.Aggregate(vector, (current, eigenValue) => current | PreComputedSprigs[eigenValue]);
                }
            }
        }
    }
}