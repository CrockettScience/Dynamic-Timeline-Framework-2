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
        /// <param name="id">The key to identify objects the object compiler will look for</param>
        public Multiverse(string id)
        {
            // Set up the big bang date node
            BaseUniverse = new Universe(this);

            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
            //Run the Compiler
            Compiler = new ObjectCompiler(id, Assembly.GetCallingAssembly());
            
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
            private static readonly Type PositionAttr = typeof(PositionAttribute);
            private static readonly Type LateralConstraintAttr = typeof(LateralConstraintAttribute);
            
            //2^63
            public const ulong SPRIG_CENTER = 9_223_372_036_854_775_808;

            private readonly Map<Type, MetaData> _objectMetaData = new Map<Type, MetaData>(); 
            
            public ObjectCompiler(string mvID, Assembly callingAssembly)
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

                    if (definitionAttr.MvID == mvID)
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
                    var typeFields = type.GetFields().Where(field => field.GetCustomAttribute(PositionAttr) is PositionAttribute).ToList();

                    var forwardMap = new Map<Position, Position>();
                    var backwardMap = new Map<Position, Position>();
                    
                    foreach (var field in typeFields)
                    {
                        var forwardTransitionAttribute = (PositionAttribute) field.GetCustomAttribute(PositionAttr);
                        var lateralConstraintAttributes = field.GetCustomAttributes(LateralConstraintAttr) as LateralConstraintAttribute[];

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
                            
                        //Use the position attribute to precompute a sprig vector about the position
                        var fieldValue = (Position) field.GetValue(null);
                        
                        //Store the forward map
                        var forwardMask = Position.Alloc(type, forwardTransitionAttribute.ForwardTransitionSetBits);
                        forwardMap[fieldValue] = forwardMask;
                        
                        //Store the reverse map
                        foreach (var innerField in typeFields) {
                            var innerPos = (Position) innerField.GetValue(null);
                            
                            if(!backwardMap.ContainsKey(innerPos))
                                backwardMap[innerPos] = Position.Alloc(type);
                            
                            //If the outer field contains innerfield in it's forward mask, that
                            //makes the outer field part of the inner field's backward mask
                            if ((forwardMask & innerPos).Uncertainty >= 0)
                                backwardMap[innerPos] |= fieldValue;
                        }
                        
                        //Compute and store the cross type translation
                        if (lateralConstraintAttributes != null)
                        {

                            foreach (var attribute in lateralConstraintAttributes)
                            {
                                //Make sure the structure exists
                                if (!lateralTranslation.ContainsKey(attribute.LateralKey))
                                    lateralTranslation[attribute.LateralKey] = new Map<Position, Position>();
                                
                                //Store the translation
                                var translation = Position.Alloc(attribute.Type, attribute.ConstraintSetBits);
                                lateralTranslation[attribute.LateralKey][fieldValue] = translation;
                                
                                //Store the reverse translation using a confirmation mask
                                
                                //If there's no metadata, that either means that the type 
                                //isn't assignable from DTFObject, or that the mvIdentifierKey
                                //isn't the one that was given.
                                
                                if(!_objectMetaData.ContainsKey(attribute.Type))
                                    throw new DTFObjectCompilerException(attribute.Type + " has invalid metadata. Make sure it inherits from " + DTFObjectType.Name + " and the DTFObjectDefinitionAttribute has mvIdentifierKey '" + mvID + "'");
                                
                                var otherLatTrans = _objectMetaData[attribute.Type].LateralTranslation;
                                foreach (var otherField in attribute.Type.GetFields())
                                {
                                    if (otherField.GetCustomAttribute(PositionAttr) is PositionAttribute)
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
                    
                    //Todo - Use the forward and backward map to compute the SprigVector
                    
                    //When we collapse to a position, the collapsed-to date can be anywhere from
                    //the beginning of the positions length or the end. So, we can compute the
                    //vector as if the collapsed position is at the "beginning" of the length, then
                    //copy it and shift the copy to the LEFT by the length value, then OR the
                    //vector and it's copy together to get the complete breadth of possibilities
                    foreach (var positionField in typeFields) {

                        //Compute backward half
                        
                        //Compute forward states
                        
                        //Save
                    }

                }
            }

            #region FUNCTIONS

            internal SprigVector GetNormalizedVector(Type type, Position position, ulong date) 
            {
                var vector = _objectMetaData[type].GetPossibilityBreadth(position);
                
                throw new NotImplementedException();
                    
                //All precomputed sprigVectors are centered at SPRIG_CENTER. We need to shift the values to the correct date
                //We have to dance around straight subtraction for the sake of avoiding overflow
                if (date < SPRIG_CENTER)
                {
                    //vector.ShiftBackward(SPRIG_CENTER - date);
                }

                if (date > SPRIG_CENTER)
                {
                    //vector.ShiftForward(date - SPRIG_CENTER);
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
                
                //Todo - Get the sprig
                
                throw new NotImplementedException();
                
                //Get the translation vector
                //var translationVector = meta.Translate(key, source.GetSprig(diff).ToVector());
                
                //AND the sprigs together and check if there was a change
                //return sprig.And(translationVector);
            }

            #endregion

            private class MetaData
            {
                public Map<string, Map<Position, Position>> LateralTranslation { get; }
                
                public Map<Position, SprigVector> PreComputedSprigs { get; }
                public Map<Position, ulong> Length { get; }
                
                public MetaData()
                {
                    LateralTranslation = new Map<string, Map<Position, Position>>();
                    PreComputedSprigs = new Map<Position, SprigVector>();
                    Length = new Map<Position, ulong>();
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
                    
                    throw new NotImplementedException();
                    /*
                     
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
                    
                    */

                }

                public SprigVector GetPossibilityBreadth(Position position)
                {
                    throw new NotImplementedException();
                    
                    /*
                     
                    var eigenValues = position.GetEigenValues();
                    var vector = new SprigVector(position.Type);

                    foreach (var eigenValue in eigenValues) {
                        var backwardBreadth = PreComputedSprigs[eigenValue].Copy();
                        backwardBreadth.ShiftBackward(Length[eigenValue]);
                        
                        vector |= PreComputedSprigs[eigenValue] | backwardBreadth;
                    }

                    return vector;
                    
                    */
                }
            }
        }
    }
}