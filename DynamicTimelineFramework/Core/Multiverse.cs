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
        /// </summary>
        public Multiverse()
        {
            // Set up the big bang diff
            BaseUniverse = new Universe(this);
            
            //Instantiate the multiverse timeline
            SprigBuilder = new SprigBuilder(BaseUniverse.Diff, BaseUniverse);
            
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

            private readonly Dictionary<Type, TypeMetaData> _objectMetaData = new Dictionary<Type, TypeMetaData>(); 
            
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
                    
                    var positionType = typeof(Position);
                    
                    //Collect all fields that have a ForwardPositionAttribute
                    var typeFields = type.GetFields().Where(field => field.GetCustomAttribute(PositionAttr) is PositionAttribute).ToList();

                    var forwardDictionary = new Dictionary<Position, Position>();
                    var backwardDictionary = new Dictionary<Position, Position>();
                    
                    //We need the lengths for later
                    var lengthsDictionary = new Dictionary<Position, long>();
                    
                    foreach (var field in typeFields)
                    {
                        var positionAttribute = (PositionAttribute) field.GetCustomAttribute(PositionAttr);
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
                        
                        //Save out length
                        lengthsDictionary[fieldValue] = positionAttribute.Length;
                        
                        //Store the forward Dictionary
                        var forwardMask = Position.Alloc(type, positionAttribute.ForwardTransitionSetBits);
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
                                
                                //If there's no metadata, that means that the type 
                                //isn't assignable from DTFObject
                                
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

                    #region Check

                    foreach (var field in typeFields)
                    {
                        
                        var pos = (Position) field.GetValue(null);
                        var backwardVals = backwardDictionary[pos].GetEigenValues();
                        var forwardVals = forwardDictionary[pos].GetEigenValues();

                        var length = lengthsDictionary[pos];
                        foreach (var backwardVal in backwardVals)
                        {

                            //All backward node values must have the same length
                            if (lengthsDictionary[backwardVal] != length)
                                throw new DTFObjectCompilerException(
                                    "Position Field '" + field.Name + "' of Type '" + type.Name +
                                    "' has one or more possible forward positions with mismatched lengths");

                        }
                        
                        length = lengthsDictionary[pos];
                        foreach (var forwardVal in forwardVals)
                        {
                            //All forward node values must have the same length
                            if (lengthsDictionary[forwardVal] != length)
                                throw new DTFObjectCompilerException(
                                    "Position Field '" + field.Name + "' of Type '" + type.Name +
                                    "' has one or more possible forward positions with mismatched lengths");

                        }
                    }

                    #endregion
                    
                    //Todo - Compute Sprig position vectors
                    
                    var preComputedSprigs = _objectMetaData[type].PreComputedVectors;
                    
                    //Helper method for span creation
                    SprigPositionVector MakeSpanVector(Position pos, long startOffset, ulong span)
                    {
                        var head = new PositionNode(null, SPRIG_CENTER + (ulong) startOffset + span, Position.Alloc(pos.Type, true)) {
                            Last = new PositionNode(null, SPRIG_CENTER + (ulong) startOffset, pos) {
                                Last = new PositionNode(null, 0, Position.Alloc(pos.Type, true))
                            }
                        };
                        
                        return new SprigPositionVector(default , head);
                    }

                    //When we collapse to a position, the focal date can be anywhere from
                    //the beginning of the positions length or the end. So, we can compute the
                    //vector by calculating the SPAN of the states separately, then OR'ing them together
                    foreach (var positionField in typeFields) {

                        var spanVectors = new List<SprigPositionVector>();
                        var pos = (Position) positionField.GetValue(null);
                        var length = lengthsDictionary[pos];
                        var shift = length - 1;
                        var span = (ulong) length + (ulong) shift;
                        
                        //Add span for pos
                        spanVectors.Add(MakeSpanVector(pos, -shift, span));
                        
                        //Find initial position
                        var initialPosition = pos;
                        var lastSuperPosition = backwardDictionary[pos];

                        while (!initialPosition.Equals(lastSuperPosition)) {
                            initialPosition = lastSuperPosition;

                            var eigenValues = initialPosition.GetEigenValues();
                            lastSuperPosition = Position.Alloc(initialPosition.Type);

                            foreach (var eigenValue in eigenValues) {
                                lastSuperPosition |= backwardDictionary[eigenValue];
                            }
                        }

                        //Calculate backward spans, and stop when we hit a position that matches the initial position
                        var currentPosition = backwardDictionary[pos];
                        
                        //We use helper nodes to keep track of where we are on the spans
                        var backwardNodeVals = currentPosition.GetEigenValues();
                        var helperNodes = new List<HelperNode>();

                        var indexOffset = 0UL;
                        foreach (var backwardNodeVal in backwardNodeVals)
                        {
                            length = lengthsDictionary[backwardNodeVal];
                            span = (ulong) length + (ulong) shift;
                            helperNodes.Add(new HelperNode(backwardNodeVal, span, (ulong) length));
                        }
                        
                        while (!currentPosition.Equals(initialPosition)) {
                            //Find the node with the lowest distance to latest start
                            var lowest = helperNodes[0];

                            for (var i = 1; i < helperNodes.Count; i++)
                                if (lowest.DistanceToLatestStart > helperNodes[i].DistanceToLatestStart)
                                    lowest = helperNodes[i];
                            
                            helperNodes.Remove(lowest);
                            
                            var distanceChange = lowest.DistanceToLatestStart;

                            //Move offset to that node
                            indexOffset -= distanceChange;
                            lowest.ReduceDistance(distanceChange);
                            
                            //Create span vector
                            length = lengthsDictionary[lowest.Value];
                            span = (ulong) length + (ulong) shift;
                            
                            spanVectors.Add(MakeSpanVector(lowest.Value, -shift, span));

                            //Subtract distance covered from remaining nodes
                            foreach (var helperNode in helperNodes)
                                helperNode.ReduceDistance(distanceChange);

                            //Add in removed node's preceding values
                            backwardNodeVals = backwardDictionary[lowest.Value].GetEigenValues();
                            foreach (var backwardNodeVal in backwardNodeVals)
                            {
                                length = lengthsDictionary[backwardNodeVal];
                                span = (ulong) length + (ulong) shift;
                                helperNodes.Add(new HelperNode(backwardNodeVal, span, (ulong) length));
                            }

                            //Set current position to the OR of all the helper nodes

                            currentPosition = helperNodes[0].Value;
                            for(var i = 1; i < helperNodes.Count; i++)
                            {
                                currentPosition |= helperNodes[i].Value;
                            }
                        }
                        
                        //Starting at this offset we're at now, the position will remain currentPosition all the way back to the beginning
                        spanVectors.Add(MakeSpanVector(initialPosition, long.MinValue, SPRIG_CENTER - indexOffset - 1));
                        
                        //Find terminal position
                        
                        //Calculate forward spans, and stop when we hit a position that matches the initial position

                        //Save out computed vector
                        var computedVector = spanVectors[0];

                        for (var i = 1; i < spanVectors.Count; i++) {
                            computedVector |= spanVectors[i];
                        }
                        
                        preComputedSprigs[pos] = new PositionMetaData(computedVector, length);
                    }
                }
            }


            private class HelperNode {
                
                public Position Value { get; }
                
                public ulong DistanceToEarliestStart { get; set; }  
                
                public ulong DistanceToLatestStart { get; set; }
                
                public HelperNode(Position value, ulong distanceToEarliestStart, ulong distanceToLatestStart) {
                    Value = value;
                    DistanceToEarliestStart = distanceToEarliestStart;
                    DistanceToLatestStart = distanceToLatestStart;
                }

                public void ReduceDistance(ulong amount)
                {
                    DistanceToEarliestStart -= amount;
                    DistanceToLatestStart -= amount;
                }
            }

            #region FUNCTIONS
            
            //Todo - Remake compiler functions to account for the need to compile constraints for various scenarios,

            #endregion

            private class TypeMetaData
            {
                public Dictionary<string, Dictionary<Position, Position>> LateralTranslation { get; }
                
                public Dictionary<Position, PositionMetaData> PreComputedVectors { get; }
                
                public TypeMetaData()
                {
                    LateralTranslation = new Dictionary<string, Dictionary<Position, Position>>();
                    PreComputedVectors = new Dictionary<Position, PositionMetaData>();
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
                    var currentOut = new PositionNode(null, currentIn.Index, Translate(key, (Position) currentIn.SuperPosition.Copy()));
                    var output = new SprigPositionVector(operativeSlice, currentOut);

                    while (currentIn.Last != null)
                    {
                        currentIn = currentIn.Last;

                        var lastTranslate = Translate(key, (Position) currentIn.SuperPosition.Copy());

                        if (lastTranslate.Equals(currentOut.SuperPosition)) {
                            currentOut.Index = currentIn.Index;
                        }
                        
                        else {
                            currentOut.Last = new PositionNode(null, currentIn.Index, lastTranslate);
                            currentOut = (PositionNode) currentOut.Last;
                        }
                    }

                    return output;

                }

                public SprigPositionVector GetVector(Position pos) {
                    return PreComputedVectors[pos].TimelineVector;

                }
            }


            private class PositionMetaData {
                
                public SprigPositionVector TimelineVector { get; }
                
                public long Length { get; }
                
                public PositionMetaData(SprigPositionVector timelineVector, long length) {
                    TimelineVector = timelineVector;
                    Length = length;
                }
            }
        }
    }
}