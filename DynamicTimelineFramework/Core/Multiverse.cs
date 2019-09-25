using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal.Buffer;
using DynamicTimelineFramework.Internal.Sprig;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    public class Multiverse
    {
        private readonly Dictionary<Diff, Universe> _multiverse;
        
        internal SprigBuilder SprigBuilder { get; }
        
        internal ObjectCompiler Compiler { get; }
        
        public Universe BaseUniverse { get; }

        /// <summary>
        /// Instantiates a new multiverse with a blank base universe and compiles the system of objects
        /// the share the identifier key in the DTFObjectDefinition attribute
        /// </summary>
        /// <param name="id">The key to identify objects the object compiler will look for</param>
        public Multiverse()
        {
            // Set up the big bang diff
            BaseUniverse = new Universe(this);
            
            //Instantiate the multiverse timeline
            SprigBuilder = new SprigBuilder(BaseUniverse.Diff);

            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
            //Run the Compiler
            Compiler = new ObjectCompiler(Assembly.GetCallingAssembly());
            
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

            private readonly Dictionary<Type, MetaData> _objectMetaData = new Dictionary<Type, MetaData>(); 
            
            public ObjectCompiler(Assembly callingAssembly)
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
                }

                foreach (var type in dtfTypes)
                {

                    //This is a type we are looking for. Now iterate though each field and build the metadata
                    var lateralTranslation = _objectMetaData[type].LateralTranslation;
                    var preComputedSprigs = _objectMetaData[type].PreComputedSprigs;
                    
                    var positionType = typeof(Position);
                    
                    //Collect all fields that have a ForwardPositionAttribute
                    var typeFields = type.GetFields().Where(field => field.GetCustomAttribute(PositionAttr) is PositionAttribute).ToList();

                    var forwardDictionary = new Dictionary<Position, Position>();
                    var backwardDictionary = new Dictionary<Position, Position>();
                    
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
                            
                        //The field must be assignable from Position
                        if (!fieldType.IsAssignableFrom(positionType))
                            throw new DTFObjectCompilerException(fieldType.Name + " is not assignable from " + positionType.Name + ", and cannot be attributed as a position by " + type.Name);
                            
                        #endregion
                            
                        //Use the position attribute to precompute a sprig vector about the position
                        var fieldValue = (Position) field.GetValue(null);
                        
                        //Store the forward Dictionary
                        var forwardMask = Position.Alloc(type, forwardTransitionAttribute.ForwardTransitionSetBits);
                        forwardDictionary[fieldValue] = forwardMask;
                        
                        //Store the reverse Dictionary
                        foreach (var innerField in typeFields) {
                            var innerPos = (Position) innerField.GetValue(null);
                            
                            if(!backwardDictionary.ContainsKey(innerPos))
                                backwardDictionary[innerPos] = Position.Alloc(type);
                            
                            //If the outer field contains innerfield in it's forward mask, that
                            //makes the outer field part of the inner field's backward mask
                            if ((forwardMask & innerPos).Uncertainty >= 0)
                                backwardDictionary[innerPos] |= fieldValue;
                        }
                        
                        //Compute and store the cross type translation
                        if (lateralConstraintAttributes != null)
                        {

                            foreach (var attribute in lateralConstraintAttributes)
                            {
                                //Make sure the structure exists
                                if (!lateralTranslation.ContainsKey(attribute.LateralKey))
                                    lateralTranslation[attribute.LateralKey] = new Dictionary<Position, Position>();
                                
                                //Store the translation
                                var translation = Position.Alloc(attribute.Type, attribute.ConstraintSetBits);
                                lateralTranslation[attribute.LateralKey][fieldValue] = translation;
                                
                                //Store the reverse translation using a confirmation mask
                                
                                //If there's no metadata, that either means that the type 
                                //isn't assignable from DTFObject, or that the mvIdentifierKey
                                //isn't the one that was given.
                                
                                if(!_objectMetaData.ContainsKey(attribute.Type))
                                    throw new DTFObjectCompilerException(attribute.Type + " has invalid metadata. Make sure it inherits from " + DTFObjectType.Name);
                                
                                var otherLatTrans = _objectMetaData[attribute.Type].LateralTranslation;
                                foreach (var otherField in attribute.Type.GetFields())
                                {
                                    if (otherField.GetCustomAttribute(PositionAttr) is PositionAttribute)
                                    {
                                        if (!otherLatTrans.ContainsKey(attribute.LateralKey + "-BACK_REFERENCE-" + type.GetHashCode()))
                                            otherLatTrans[attribute.LateralKey + "-BACK_REFERENCE-" + type.GetHashCode()] = new Dictionary<Position, Position>();

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
                    
                    //Todo - Compute Sprig position vectors
                    
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
            
            //Todo - Remake compiler functions to account for the need to compile constraints for various scenarios,
            //including applying crumple for diff chains, getting the breadth of timelines for both positions AND
            //position buffers, etc

            #endregion

            private class MetaData
            {
                public Dictionary<string, Dictionary<Position, Position>> LateralTranslation { get; }
                
                public Dictionary<Position, SprigPositionVector> PreComputedSprigs { get; }
                
                public MetaData()
                {
                    LateralTranslation = new Dictionary<string, Dictionary<Position, Position>>();
                    PreComputedSprigs = new Dictionary<Position, SprigPositionVector>();
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

                public SprigPositionVector Translate(string key, SprigPositionVector input, Slice operativeSlice)
                {
                     
                    var currentIn = input.Head;
                    var currentOut = new PositionNode(null, currentIn.Index, Translate(key, (Position) currentIn.SuperPosition.Copy()), operativeSlice);
                    var output = new SprigPositionVector(input.Slice, currentOut);

                    while (currentIn.Last != null)
                    {
                        currentIn = currentIn.Last;
                        
                        currentOut.Last = new PositionNode(null, currentIn.Index, Translate(key, (Position) currentIn.SuperPosition.Copy()), operativeSlice);

                        currentOut = (PositionNode) currentOut.Last;
                    }

                    return output;

                }
            }
        }
    }
}