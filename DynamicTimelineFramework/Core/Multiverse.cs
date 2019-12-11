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
        //Bonus Level:
        //Todo - Document public facing classes and members
        //Todo - Add more options to constrain, including a way to constrain more than one object at once and encode changes in a single diff
        //Todo - Add a way to "select" a single position from a list of positions, both for one object and for several at a time
        //Todo - Add ways to discern exactly what objects and states are encoded inside diffs and diffChains

        private readonly Dictionary<Diff, Universe> _multiverse;
        
        internal SprigBuilder SprigBuilder { get; }
        
        internal ObjectCompiler Compiler { get; }
        
        public Universe BaseUniverse { get; }

        /// <summary>
        /// Instantiates a new multiverse with a blank base universe and compiles the system of objects
        /// </summary>
        public Multiverse()
        {
            //Run the Compiler
            Compiler = new ObjectCompiler(Assembly.GetCallingAssembly(), this);
            
            // Set up the big bang diff
            BaseUniverse = new Universe(this);
            
            //Instantiate the multiverse timeline
            SprigBuilder = new SprigBuilder(BaseUniverse.Diff, BaseUniverse);
            
            _multiverse = new Dictionary<Diff, Universe>()
            {
                {BaseUniverse.Diff, BaseUniverse}
            };
            
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

        internal void AddUniverse(Universe universe)
        {
            _multiverse[universe.Diff] = universe;
        }
        
        internal class ObjectCompiler
        {
            private static readonly Type DTFObjectType = typeof(DTFObject);
            
            private static readonly Type DTFObjectDefAttr = typeof(DTFObjectDefinitionAttribute);
            private static readonly Type PositionAttr = typeof(PositionAttribute);
            private static readonly Type LateralConstraintAttr = typeof(LateralConstraintAttribute);
            
            //2^63
            public const ulong SprigCenter = 9_223_372_036_854_775_808;

            private readonly Dictionary<Type, TypeMetaData> _objectMetaData = new Dictionary<Type, TypeMetaData>();

            private readonly Multiverse Owner;
            
            public ObjectCompiler(Assembly callingAssembly, Multiverse owner)
            {
                Owner = owner;
                var types = callingAssembly.GetTypes();
                
                var dtfTypes = new List<Type>();
                
                //Go through the types and create their metadata
                foreach (var type in types)
                {
                    if (!type.IsSubclassOf(typeof(DTFObject))) continue;

                    if (!(type.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute definitionAttr))
                        throw new DTFObjectCompilerException(type.Name + " is assignable from " + DTFObjectType.Name +
                                                             " but does not have a " + DTFObjectDefAttr.Name +
                                                             " defined.");
                    
                    dtfTypes.Add(type);
                    _objectMetaData[type] = new TypeMetaData();
                }

                foreach (var type in dtfTypes)
                {

                    //This is a type we are looking for. Now iterate though each field and build the metadata
                    var lateralTranslation = _objectMetaData[type].LateralTranslation.Map;
                    
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
                                
                                var otherLatTrans = _objectMetaData[attribute.Type].LateralTranslation.Map;
                                foreach (var otherField in attribute.Type.GetFields())
                                {
                                    if (otherField.GetCustomAttribute(PositionAttr) is PositionAttribute)
                                    {
                                        if (!otherLatTrans.ContainsKey(attribute.LateralKey + "-BACK_REFERENCE" + type.Name))
                                            otherLatTrans[attribute.LateralKey + "-BACK_REFERENCE" + type.Name] = new Dictionary<Position, Position>();

                                        var backTranslation = otherLatTrans[attribute.LateralKey + "-BACK_REFERENCE" + type.Name];
                                        
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
                        var backwardVals = backwardDictionary[pos].GetEigenStates();
                        var forwardVals = forwardDictionary[pos].GetEigenStates();

                        var length = lengthsDictionary[backwardVals[0]];
                        for (var i = 1; i < backwardVals.Count; i++)
                        {
                            //All backward node values must have the same length
                            if (lengthsDictionary[backwardVals[i]] != length)
                                throw new DTFObjectCompilerException(
                                    "Position Field '" + field.Name + "' of Type '" + type.Name +
                                    "' has one or more possible forward positions with mismatched lengths");
                        }

                        length = lengthsDictionary[forwardVals[0]];
                        for (var i = 1; i < forwardVals.Count; i++)
                        {
                            //All forward node values must have the same length
                            if (lengthsDictionary[forwardVals[i]] != length)
                                throw new DTFObjectCompilerException(
                                    "Position Field '" + field.Name + "' of Type '" + type.Name +
                                    "' has one or more possible forward positions with mismatched lengths");
                        }
                    }

                    #endregion
                    
                    var preComputedSprigs = _objectMetaData[type].PreComputedVectors;
                    
                    //Helper methods for span creation
                    SprigPositionVector MakeSpanVectorFromNode(HelperNode node)
                    {
                        var pos = node.Value;
                        var start = node.EarliestStart;
                        var span = node.LatestEnd - node.EarliestStart;
                        
                        PositionNode head;
                        if (start == 0) {
                            head = new PositionNode(null, span, Position.Alloc(pos.Type)) {
                                Last = new PositionNode(null, 0, pos)
                            };
                            
                        }
                        
                        else if (start + span == ulong.MaxValue) {
                            head = new PositionNode(null, start, pos) {
                                Last = new PositionNode(null, 0, Position.Alloc(pos.Type))
                            };
                        }
                        
                        else {
                            head = new PositionNode(null, start + span, Position.Alloc(pos.Type)) {
                                Last = new PositionNode(null, start, pos) {
                                    Last = new PositionNode(null, 0, Position.Alloc(pos.Type))
                                }
                            };
                        }
                        
                        return new SprigPositionVector(default , head);
                    }
                    
                    SprigPositionVector MakeSpanVectorFromPos(Position pos, ulong start, ulong span)
                    {
                        
                        PositionNode head;
                        if (start == 0) {
                            head = new PositionNode(null, span, Position.Alloc(pos.Type)) {
                                Last = new PositionNode(null, 0, pos)
                            };
                            
                        }
                        
                        else if (start + span == ulong.MaxValue) {
                            head = new PositionNode(null, start, pos) {
                                Last = new PositionNode(null, 0, Position.Alloc(pos.Type))
                            };
                        }
                        
                        else {
                            head = new PositionNode(null, start + span, Position.Alloc(pos.Type)) {
                                Last = new PositionNode(null, start, pos) {
                                    Last = new PositionNode(null, 0, Position.Alloc(pos.Type))
                                }
                            };
                        }
                        
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
                        spanVectors.Add(MakeSpanVectorFromPos(pos, SprigCenter - (ulong) shift, span));

                        {
                            //Find initial position
                            var initialPosition = pos;
                            var lastSuperPosition = backwardDictionary[pos];

                            while (!initialPosition.Equals(lastSuperPosition)) {
                                initialPosition = lastSuperPosition;

                                var eigenValues = initialPosition.GetEigenStates();
                                lastSuperPosition = Position.Alloc(type);

                                foreach (var eigenValue in eigenValues) {
                                    lastSuperPosition |= backwardDictionary[eigenValue];
                                }
                            }

                            //Calculate backward spans, and stop when we hit a position that matches the initial position
                            var currentPosition = backwardDictionary[pos];
                            var repeatsBackward = (currentPosition & pos).Uncertainty == 0;

                            //We use helper nodes to keep track of where we are on the spans
                            var backwardNodeVals = currentPosition.GetEigenStates();
                            var helperNodes = new List<HelperNode>();

                            var index = SprigCenter;
                            foreach (var backwardNodeVal in backwardNodeVals) {
                                length = lengthsDictionary[backwardNodeVal];
                                span = (ulong) length + (ulong) shift;
                                helperNodes.Add(new HelperNode(backwardNodeVal, index - span, span, shift, false, repeatsBackward || (backwardDictionary[backwardNodeVal] & backwardNodeVal).Uncertainty == 0));
                            }

                            while (!currentPosition.Equals(initialPosition)) {
                                //Find the node with the lowest distance to earliest start
                                HelperNode lowest = null;

                                foreach (var node in helperNodes)
                                    if ((lowest == null || index - lowest.TheoreticalEarliestStart > index - node.TheoreticalEarliestStart) && !node.SpanMade)
                                        lowest = node;

                                //Move offset to that node
                                index = lowest.TheoreticalEarliestStart;

                                //Create span vector
                                spanVectors.Add(MakeSpanVectorFromNode(lowest));
                                lowest.SpanMade = true;

                                //Add in removed node's preceding values
                                lowest.AddbackwardValues(backwardDictionary, lengthsDictionary, helperNodes, shift);

                                //Set current position to the OR of all the helper nodes that are in between the lines
                                currentPosition = null;
                                foreach (var node in helperNodes)
                                {
                                    if (index >= node.EarliestStart && index < node.LatestEnd)
                                        currentPosition = currentPosition == null
                                            ? node.Value
                                            : currentPosition | node.Value;
                                }
                            }

                            //Starting at this offset we're at now, the position will remain initialPosition all the way back to the beginning
                            spanVectors.Add(MakeSpanVectorFromPos(initialPosition, 0, index));

                        }    

                        {
                            //Find terminal position
                            var terminalPosition = pos;
                            var nextSuperPosition = forwardDictionary[pos];

                            while (!terminalPosition.Equals(nextSuperPosition)) {
                                terminalPosition = nextSuperPosition;

                                var eigenValues = terminalPosition.GetEigenStates();
                                nextSuperPosition = Position.Alloc(type);

                                foreach (var eigenValue in eigenValues) {
                                    nextSuperPosition |= forwardDictionary[eigenValue];
                                }
                            }

                            //Calculate forward spans, and stop when we hit a position that matches the terminal position
                            var currentPosition = forwardDictionary[pos];
                            var repeatsforward = (currentPosition & pos).Uncertainty == 0;

                            //We use helper nodes to keep track of where we are on the spans
                            var forwardNodeVals = currentPosition.GetEigenStates();
                            var helperNodes = new List<HelperNode>();

                            var index = SprigCenter;
                            foreach (var forwardNodeVal in forwardNodeVals) {
                                length = lengthsDictionary[forwardNodeVal];
                                span = (ulong) length + (ulong) shift;
                                helperNodes.Add(new HelperNode(forwardNodeVal, index + 1, span, shift, repeatsforward || (forwardDictionary[forwardNodeVal] & forwardNodeVal).Uncertainty == 0, false));
                            }

                            while (!currentPosition.Equals(terminalPosition)) {
                                //Find the node with the lowest distance to the earliest start
                                HelperNode lowest = null;

                                foreach (var node in helperNodes)
                                    if ((lowest == null || lowest.LatestStart - index > node.LatestStart - index) && !node.SpanMade)
                                        lowest = node;

                                //Move offset to that node
                                index = lowest.LatestStart;

                                //Create span vector
                                spanVectors.Add(MakeSpanVectorFromNode(lowest));
                                lowest.SpanMade = true;

                                //Add in removed node's next values
                                lowest.AddForwardValues(forwardDictionary, lengthsDictionary, helperNodes, shift);

                                //Set current position to the OR of all the helper nodes that the index is inside of
                                currentPosition = null;
                                foreach (var node in helperNodes)
                                {
                                    if(index >= node.EarliestStart && index < node.LatestEnd)
                                        currentPosition = currentPosition == null
                                            ? node.Value
                                            : currentPosition | node.Value;
                                }
                            }

                            //Starting at this offset we're at now, the position will remain terminalPosition all the way to the end
                            spanVectors.Add(MakeSpanVectorFromPos(terminalPosition, index, ulong.MaxValue - index));
                        }

                        //Save out computed vector
                        var computedVector = spanVectors[0];

                        for (var i = 1; i < spanVectors.Count; i++) {
                            computedVector |= spanVectors[i];
                        }
                        
                        preComputedSprigs[pos] = new PositionMetaData(computedVector, length);
                    }
                }
            }
            
            private class HelperNode
            {
                
                public Position Value { get; }
                public ulong TheoreticalEarliestStart { get; }
                public ulong EarliestStart { get; }  
                public ulong LatestStart { get; }
                public ulong LatestEnd { get; }
                public bool SpanMade { get; set; }

                private readonly HelperNode _repInheritedFrom;

                private readonly bool _repFwd;
                private readonly bool _repBwd;
                
                public HelperNode(Position value, ulong theoreticalEarliestStart, ulong span, long shift, bool repFwd, bool repBwd, HelperNode repInheritedFrom = null) 
                {
                    //Determine if the state repeats forward or backward
                    _repFwd = repFwd;
                    _repBwd = repBwd;

                    _repInheritedFrom = repInheritedFrom;
                    
                    Value = value;
                    TheoreticalEarliestStart = theoreticalEarliestStart;
                    EarliestStart = _repBwd ? 0 : theoreticalEarliestStart;
                    LatestStart = theoreticalEarliestStart + (ulong) shift;
                    LatestEnd = _repFwd ? ulong.MaxValue : theoreticalEarliestStart + span;
                    SpanMade = false;
                }

                public void AddForwardValues(Dictionary<Position, Position> forwardDictionary, Dictionary<Position, long> lengths, List<HelperNode> nodes, long shift)
                {
                    
                    var forwardNodeVals = forwardDictionary[Value].GetEigenStates();
                    foreach (var forwardNodeVal in forwardNodeVals) {
                        
                        //Don't add the forward val if it's the same as the current val
                        if (forwardNodeVal.Equals(Value) || IsInheritedFrom(forwardNodeVal))
                            continue;

                        var span = (ulong) lengths[forwardNodeVal] + (ulong) shift;

                        nodes.Add(_repFwd
                            ? new HelperNode(forwardNodeVal, EarliestStart + (ulong) lengths[Value], span, shift, true, false, this)
                            : new HelperNode(forwardNodeVal, EarliestStart + (ulong) lengths[Value], span, shift, (forwardDictionary[forwardNodeVal] & forwardNodeVal).Uncertainty == 0, false));
                    }
                }
                
                public void AddbackwardValues(Dictionary<Position, Position> backwardDictionary, Dictionary<Position, long> lengths, List<HelperNode> nodes, long shift)
                {
                    
                    var backwardNodeVals = backwardDictionary[Value].GetEigenStates();
                    foreach (var backwardNodeVal in backwardNodeVals) {
                        
                        //Don't add the forward val if it's the same as the current val
                        if (backwardNodeVal.Equals(Value) || IsInheritedFrom(backwardNodeVal))
                            continue;

                        var length = lengths[backwardNodeVal];
                        var span = (ulong) length + (ulong) shift;

                        nodes.Add(_repBwd
                            ? new HelperNode(backwardNodeVal, LatestStart - span, span, shift, false, true, this)
                            : new HelperNode(backwardNodeVal, LatestStart - span, span, shift, false, (backwardDictionary[backwardNodeVal] & backwardNodeVal).Uncertainty == 0));
                    }
                }

                private bool IsInheritedFrom(Position pos)
                {
                    var current = this;
                    while (current._repInheritedFrom != null)
                    {
                        if (current._repInheritedFrom.Value.Equals(pos))
                            return true;

                        current = current._repInheritedFrom;
                    }

                    return false;
                }
            }

            #region FUNCTIONS

            internal void AddLateralProxy(Type destType, Type sourceType, string inputKey, string sourceKey) {
                _objectMetaData[destType].LateralTranslation.AddProxy(inputKey, sourceKey + "-BACK_REFERENCE" + sourceType.Name);
            }
            
            internal void PushLateralConstraints(DTFObject dtfObj, Universe universe)
            {
                //Get the metadata object
                var meta = _objectMetaData[dtfObj.GetType()];
                
                //Iterate through each key and recursively constrain
                var keys = dtfObj.GetLateralKeys();
                foreach (var key in keys) {
                    
                    var dest = dtfObj.GetLateralObject(key);
                    
                    if (!ConstrainLateralObject(dtfObj, key, meta, universe)) continue;
                    
                    //If the constraint results in change, then push it's constraints as well
                    
                    PushLateralConstraints(dest, universe);
                }
                
            }

            private bool ConstrainLateralObject(DTFObject source, string lateralKey, TypeMetaData lateralMetaData, Universe universe)
            {
                var dest = source.GetLateralObject(lateralKey);
                
                //Get the translation vector
                var translationVector = lateralMetaData.Translate(lateralKey, universe.Sprig.ToPositionVector(source), dest.SprigBuilderSlice);
                
                //AND the sprig with the translation vector and return whether or not a change was made
                return universe.Sprig.And(translationVector);
            }

            
            public SprigBufferVector GetTimelineVector(ulong date, PositionBuffer buffer) {
                var builder = Owner.SprigBuilder;
                var timelineVector = new SprigBufferVector(builder.IndexedSpace);

                var registry = builder.Registry;

                foreach (var dtfObject in registry)
                {
                    timelineVector |= GetTimelineVector(dtfObject, date, buffer.PositionAtSlice(dtfObject.GetType(), dtfObject.SprigBuilderSlice));
                }

                return timelineVector;
            }

            public SprigPositionVector GetTimelineVector(DTFObject dtfObject, ulong date, Position position)
            {
                var timelineVector = new SprigPositionVector(dtfObject.SprigBuilderSlice, new PositionNode(null, 0, Position.Alloc(dtfObject.GetType())));

                var eigenVals = position.GetEigenStates();

                foreach (var val in eigenVals)
                {
                    timelineVector |= _objectMetaData[dtfObject.GetType()].GetVector(val);
                }
                
                if(date < SprigCenter)
                    timelineVector.ShiftBackward(SprigCenter - date);
                
                if(date > SprigCenter)
                    timelineVector.ShiftForward(date - SprigCenter);

                return timelineVector;
            }

            #endregion

            private class TypeMetaData
            {
                public LateralTranslation LateralTranslation { get; }
                
                public Dictionary<Position, PositionMetaData> PreComputedVectors { get; }
                
                public TypeMetaData()
                {
                    LateralTranslation = new LateralTranslation();
                    PreComputedVectors = new Dictionary<Position, PositionMetaData>();
                }

                public SprigPositionVector Translate(string key, SprigPositionVector input, OperativeSlice outputOperativeSlice)
                {
                     
                    var currentIn = input.Head;
                    var currentOut = new PositionNode(null, currentIn.Index, LateralTranslation[key, (Position) currentIn.SuperPosition.Copy()]);
                    var output = new SprigPositionVector(outputOperativeSlice, currentOut);

                    while (currentIn.Last != null)
                    {
                        currentIn = currentIn.Last;

                        var lastTranslate = LateralTranslation[key, (Position) currentIn.SuperPosition.Copy()];

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


            private class LateralTranslation {
                public readonly Dictionary<string, Dictionary<Position, Position>> Map = new Dictionary<string, Dictionary<Position, Position>>();
                private readonly Dictionary<string, string> _proxy = new Dictionary<string, string>();

                public Position this[string key, Position input] {
                    get {
                        var latMap = Map[_proxy.ContainsKey(key) ? _proxy[key] : key];
                        var eigenvals = input.GetEigenStates();

                        var superPosition = latMap[eigenvals[0]];

                        for (var i = 1; i < eigenvals.Count; i++) {
                            superPosition |= latMap[eigenvals[i]];
                        }

                        return superPosition;
                    }
                }

                public void AddProxy(string input, string output) {
                    _proxy[input] = output;
                }
                
            }
        }
    }
}