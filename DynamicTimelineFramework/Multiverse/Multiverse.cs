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
                var types = callingAssembly.GetTypes();
                
                var dtfTypes = new List<Type>();
                
                //Go through the types and create their metadata
                foreach (var type in types)
                {
                    if (!type.IsAssignableFrom(typeof(DTFObject))) continue;

                    if (!(type.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute definitionAttr))
                        throw new DTFObjectCompilerException(type.Name + " is assignable from " + DTFObjectType.Name +
                                                             " but does not have a " + DTFObjectDefAttr.Name +
                                                             " defined.");

                    if (definitionAttr.MvIdentifierKey == mvIdentifierKey)
                    {
                        dtfTypes.Add(type);
                        _objectMetaData[type] = new MetaData();
                    }
                }

                foreach (var type in dtfTypes)
                {

                    //This is a type we are looking for. Now iterate though each field and build the metadata
                    var lateralTranslation = _objectMetaData[type].LateralTranslation;
                    var preComputedSprigs = _objectMetaData[type].PreComputedSprigs;
                    
                    var positionType = typeof(Position);
                    
                    //Collect all fields that have a ForwardPositionAttribute
                    var typeFields = new List<FieldInfo>();

                    foreach (var field in type.GetFields())
                    {
                        if(field.GetCustomAttribute(ForwardPositionAttr) is ForwardPositionAttribute)
                            typeFields.Add(field);
                    }
                    
                    foreach (var field in typeFields)
                    {
                        
                        var forwardPositionAttribute = field.GetCustomAttribute(ForwardPositionAttr) as ForwardPositionAttribute;
                        var lateralPositionAttributes = field.GetCustomAttributes(LateralPositionAttr) as LateralPositionAttribute[];

                        //Perform a series of run time checks, and add to some preliminary structures
                        var fieldType = field.FieldType;
                        
                        #region CHECKS
                            
                        //The field must be static
                        if (!field.IsStatic)
                            throw new DTFObjectCompilerException("All of " + fieldType.Name + "\'s attributed positions must be static");
                            
                        //The field must be assignable from Position<Type>
                        if (!fieldType.IsAssignableFrom(positionType))
                            throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);
                            
                        #endregion
                            
                        //Todo - Use the forward mapping to compute the sprigVector for the field position as an eigen value
                        
                        //Compute and store the cross type translation
                        if (lateralPositionAttributes != null)
                        {
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
                                //Make sure the structure exists
                                if (!lateralTranslation.ContainsKey(attribute.LateralKey))
                                    lateralTranslation[attribute.LateralKey] = new Map<Position, Position>();
                                
                                //Store the translation
                                var translation = Position.Alloc(attribute.Type, attribute.Flag);
                                lateralTranslation[attribute.LateralKey][fieldValue] = translation;
                                
                                //Store the reverse translation using a confirmation mask
                                
                                //If there's no metadata, that either means that the type 
                                //isn't assignable from DTFObject, or that the mvIdentifierKey
                                //isn't the one that was given.
                                
                                if(!_objectMetaData.ContainsKey(attribute.Type))
                                    throw new DTFObjectCompilerException(attribute.Type + " has invalid metadata. Make sure it inherits from " + DTFObjectType.Name + " and the DTFObjectDefinitionAttribute has mvIdentifierKey '" + mvIdentifierKey + "'");
                                
                                var otherLatTrans = _objectMetaData[attribute.Type].LateralTranslation;
                                foreach (var otherField in attribute.Type.GetFields())
                                {
                                    if (otherField.GetCustomAttribute(ForwardPositionAttr) is ForwardPositionAttribute)
                                    {
                                        if (!otherLatTrans.ContainsKey(attribute.LateralKey + "-BACK_REFERENCE-" + type.GetHashCode()))
                                            otherLatTrans[attribute.LateralKey + "-BACK_REFERENCE-" + type.GetHashCode()] = new Map<Position, Position>();

                                        var backTranslation = otherLatTrans[attribute.LateralKey + "-BACK_REFERENCE-" + type.GetHashCode()];
                                        
                                        var otherPosition = (Position) otherField.GetValue(null);

                                        if ((translation & otherPosition).Uncertainty >= 0)
                                        {
                                            //This means that the field we're looking at is part of the translation
                                            //we mapped to earlier, we can "Add" the outer field's position to the other
                                            //object as a reverse translation. This fulfills the transitive property for
                                            //the relation.
                                            if (!backTranslation.ContainsKey(otherPosition))
                                                backTranslation[otherPosition] = Position.Alloc(type);

                                            backTranslation[otherPosition] |= fieldValue;

                                        }
                                    }
                                }
                            }
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
                var keys = dtfObj.GetLateralKeys();
                foreach (var key in keys) {
                    
                    var dest = dtfObj.GetLateralObject(key);
                    
                    if (!Constrain(dtfObj, key, meta, diff)) continue;
                    
                    //If the constraint results in change, then push it's constraints as well
                    
                    PushConstraints(dest, diff);
                }
                
            }

            internal void PullConstraints(DTFObject dtfObj, Diff diff)
            {
                
                if (dtfObj.HasParent())
                    //It has no parents, no need to pull any constraints
                    return;
                
                //It has a parent, so pull its parent's constraints
                        
                //Get the parent object
                var key = dtfObj.GetParentKey();
                var parent = dtfObj.GetLateralObject(key);

                PullConstraints(parent, diff);
                
                //Now that we know, after some amount of recursion, that the parent object
                //is fully constrained, we can constrain by it
                
                //Get the meta object for the other object
                var otherMeta = _objectMetaData[parent.GetType()];
                
                //Constrain
                Constrain(parent, dtfObj.GetParentKey(), otherMeta, diff);
            }

            private static bool Constrain(DTFObject source, string key, MetaData meta, Diff diff)
            {
                //Get the destination object
                var dest = source.GetLateralObject(key);
                
                //Get the sprig
                var sprig = dest.GetSprig(diff);
                
                //Get the translation vector
                var translationVector = meta.Translate(key, source.GetSprig(diff).ToVector());
                
                //AND the sprigs together and check if there was a change
                return sprig.And(translationVector);
            }

            #endregion

            private class MetaData
            {
                
                public Map<string, Map<Position, Position>> LateralTranslation { get; }
                
                public Map<Position, SprigVector> PreComputedSprigs { get; }
                
                public MetaData()
                {
                    LateralTranslation = new Map<string, Map<Position, Position>>();
                    PreComputedSprigs = new Map<Position, SprigVector>();
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