using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Objects;
using DynamicTimelineFramework.Objects.Attributes;

namespace DynamicTimelineFramework.Core
{
    /// <summary>
    /// The core object of DynamicTimelineFramework. A multiverse represents a collection of branching universes that
    /// all share states up until a particular date, where the timelines "branch" off into separate paths. TH
    ///
    /// The Multiverse constructor will seek through the calling assembly's classes and look for classes that inherit
    /// from DTFObject, and then evaluate the required attributes and can throw various exceptions if errors are made.
    /// </summary>
    public class Multiverse
    {

        private readonly Dictionary<Diff, Universe> _multiverse = new Dictionary<Diff, Universe>();
        
        internal SprigManager SprigManager { get; }
        
        internal ObjectCompiler Compiler { get; }
        
        private readonly HashSet<Diff> _pendingDiffs = new HashSet<Diff>();

        /// <summary>
        /// Whether or not to validate operations made to the timeline. Turning this off will
        /// increase performance, but make it more difficult to detect invalid bridging exceptions
        /// definitions. It's recommended to leave this on if you are testing new DTFObjects with
        /// lateral definitions. It is turned ON by default
        /// </summary>
        public bool ValidateOperations { get; set; }
        
        /// <summary>
        /// The core universe that all other universes branch off of.
        /// </summary>
        public Universe BaseUniverse { get; }

        /// <summary>
        /// Instantiates a new multiverse with a blank base universe and compiles the system of objects.
        /// </summary>
        public Multiverse(bool validateOps = true)
        {
            //Compile all states from the calling assembly
            Compiler = new ObjectCompiler(Assembly.GetCallingAssembly(), this);
            
            //Set up the initial universe
            BaseUniverse = new Universe(this);
            
            //Instantiate the multiverse timeline
            SprigManager = new SprigManager(BaseUniverse.Diff, BaseUniverse);
            
            //Whether or not to validate ops
            ValidateOperations = validateOps;

        }

        /// <summary>
        /// Accesses the specific universe that was constructed using the given diff.
        /// </summary>
        /// <param name="diff">The diff used to construct the universe</param>
        public Universe this[Diff diff] => _multiverse[diff];

        internal void AddDiffPending(Diff diff)
        {
            _pendingDiffs.Add(diff);
        }

        internal void ClearPendingDiffs()
        {
            foreach (var diff in _pendingDiffs)
                diff.IsExpired = true;
            
            _pendingDiffs.Clear();
        }

        internal void AddUniverse(Universe universe)
        {
            _multiverse[universe.Diff] = universe;
        }

        internal void RemoveUniverse(Universe universe) {
            _multiverse.Remove(universe.Diff);
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

                    if (!(type.GetCustomAttribute(DTFObjectDefAttr) is DTFObjectDefinitionAttribute))
                        throw new DTFObjectCompilerException(type.Name + " is assignable from " + DTFObjectType.Name +
                                                             " but does not have a " + DTFObjectDefAttr.Name +
                                                             " defined.");
                    
                    dtfTypes.Add(type);
                    _objectMetaData[type] = new TypeMetaData(type);
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
                                if (!lateralTranslation.Contains(attribute.LateralKey)) {
                                    lateralTranslation.AddType(attribute.LateralKey, _objectMetaData[attribute.Type]);
                                }

                                //Store the translation
                                var translation = Position.Alloc(attribute.Type, attribute.ConstraintSetBits);
                                lateralTranslation.GetPositionMap(attribute.LateralKey)[fieldValue] = translation;
                                
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
                                        if (!otherLatTrans.Contains(attribute.LateralKey + "-BACK_REFERENCE-" + type.Name)) {
                                            otherLatTrans.AddType(attribute.LateralKey + "-BACK_REFERENCE-" + type.Name, _objectMetaData[type]);
                                        }

                                        var backTranslation = otherLatTrans.GetPositionMap(attribute.LateralKey + "-BACK_REFERENCE-" + type.Name);
                                        
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
                        var backwardVals = backwardDictionary[pos].GetEigenstates();
                        var forwardVals = forwardDictionary[pos].GetEigenstates();

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
                    PositionVector MakeSpanVectorFromNode(HelperNode node)
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
                        
                        return new PositionVector(head);
                    }
                    
                    PositionVector MakeSpanVectorFromPos(Position pos, ulong start, ulong span)
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
                        
                        return new PositionVector(head);
                    }

                    //When we collapse to a position, the focal date can be anywhere from
                    //the beginning of the positions length or the end. So, we can compute the
                    //vector by calculating the SPAN of the states separately, then OR'ing them together
                    foreach (var positionField in typeFields) {

                        var spanVectors = new List<PositionVector>();
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

                                var eigenValues = initialPosition.GetEigenstates();
                                lastSuperPosition = Position.Alloc(type);

                                foreach (var eigenValue in eigenValues) {
                                    lastSuperPosition |= backwardDictionary[eigenValue];
                                }
                            }

                            //Calculate backward spans, and stop when we hit a position that matches the initial position
                            var currentPosition = backwardDictionary[pos];
                            var repeatsBackward = (currentPosition & pos).Uncertainty == 0;

                            //We use helper nodes to keep track of where we are on the spans
                            var backwardNodeVals = currentPosition.GetEigenstates();
                            var helperNodes = new List<HelperNode>();

                            var index = SprigCenter - 1;
                            foreach (var backwardNodeVal in backwardNodeVals) {
                                length = lengthsDictionary[backwardNodeVal];
                                span = (ulong) length + (ulong) shift;
                                helperNodes.Add(new HelperNode(backwardNodeVal, index - span + 1, span, shift, false, repeatsBackward || (backwardDictionary[backwardNodeVal] & backwardNodeVal).Uncertainty == 0));
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
                            spanVectors.Add(MakeSpanVectorFromPos(initialPosition, 0, index + 1));

                        }    

                        {
                            //Find terminal position
                            var terminalPosition = pos;
                            var nextSuperPosition = forwardDictionary[pos];

                            while (!terminalPosition.Equals(nextSuperPosition)) {
                                terminalPosition = nextSuperPosition;

                                var eigenValues = terminalPosition.GetEigenstates();
                                nextSuperPosition = Position.Alloc(type);

                                foreach (var eigenValue in eigenValues) {
                                    nextSuperPosition |= forwardDictionary[eigenValue];
                                }
                            }

                            //Calculate forward spans, and stop when we hit a position that matches the terminal position
                            var currentPosition = forwardDictionary[pos];
                            var repeatsForward = (currentPosition & pos).Uncertainty == 0;

                            //We use helper nodes to keep track of where we are on the spans
                            var forwardNodeVals = currentPosition.GetEigenstates();
                            var helperNodes = new List<HelperNode>();

                            var index = SprigCenter + 1;
                            foreach (var forwardNodeVal in forwardNodeVals) {
                                length = lengthsDictionary[forwardNodeVal];
                                span = (ulong) length + (ulong) shift;
                                helperNodes.Add(new HelperNode(forwardNodeVal, index, span, shift, repeatsForward || (forwardDictionary[forwardNodeVal] & forwardNodeVal).Uncertainty == 0, false));
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
                    
                    var forwardNodeVals = forwardDictionary[Value].GetEigenstates();
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
                    
                    var backwardNodeVals = backwardDictionary[Value].GetEigenstates();
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
            
            internal void PushLateralConstraints(DTFObject dtfObj, Sprig universeSprig, bool pushToParent)
            {
                //Setup affected objects
                
                //Get the metadata object
                var meta = _objectMetaData[dtfObj.GetType()];

                //Constrain up the parent tree
                if (pushToParent && dtfObj.ParentKey != null && ConstrainLateralObject(dtfObj, dtfObj.ParentKey, meta, universeSprig))
                    PushLateralConstraints(dtfObj.Parent, universeSprig, true);

                //Iterate through the rest of the keys and recursively constrain
                var keys = dtfObj.GetLateralKeys();
                foreach (var key in keys) {
                    
                    var dest = dtfObj.GetLateralObject(key);
                    
                    if (!ConstrainLateralObject(dtfObj, key, meta, universeSprig)) continue;
                    
                    //If the constraint results in change, then push it's constraints as well
                    //Since all lateral objects have the same parent, don't need to push to parent every time
                    PushLateralConstraints(dest, universeSprig, false);
                }
            }
            
            internal bool ConstrainLateralObject(DTFObject source, string lateralKey, TypeMetaData lateralMetaData, Sprig universeSprig)
            {
                var dest = source.GetLateralObject(lateralKey);
                
                //Get the translation vector
                var translationVector = lateralMetaData.Translate(lateralKey, universeSprig.ToPositionVector(source));
                
                //AND the sprig with the translation vector and return whether or not a change was made
                return universeSprig.And(translationVector, dest);
            }

            internal bool CanConstrainLateralObject(DTFObject source, DTFObject dest, string lateralKey, PositionVector input, Sprig sprig, out PositionVector translation) {
                var meta = _objectMetaData[source.GetType()];

                translation = meta.Translate(lateralKey, input);
                var destTimeline = sprig.ToPositionVector(dest);

                var result = translation & destTimeline;

                if (result.Validate()) {
                    if (!destTimeline.Equals(result)) {
                        sprig.And(result, dest);
                        PushLateralConstraints(dest, sprig, true);
                    }
                    
                    return true;
                }
                
                return false;
            }
            
            internal void PullLateralConstraints(DTFObject source, DTFObject dest)
            {
                //Get the metadata object
                var meta = _objectMetaData[source.GetType()];
                
                //Iterate through each universe and pull constraints into the timelines
                foreach (var universe in Owner._multiverse.Values)
                {
                    universe.EnqueueConstraintTask(source, dest, meta);
                }
            }
            
            internal void PullParentInformation(DTFObject dest, Universe universe)
            {
                //Recursively pull up
                if (dest.Parent != null) {
                    PullParentInformation(dest.Parent, universe);

                    var source = dest.Parent;

                    //Get the metadata object
                    var meta = _objectMetaData[source.GetType()];

                    //Constrain from parent and push if changes were found
                    var translationVector = meta.Translate(dest.ParentKey + "-BACK_REFERENCE-" + dest.GetType().Name, universe.Sprig.ToPositionVector(source));
                    
                    if(universe.Sprig.And(translationVector, dest))
                        PushLateralConstraints(dest, universe.Sprig, false);
                }
                
            }

            internal PositionVector GetTimelineVector(DTFObject dtfObject, ulong date, Position position)
            {
                var vector = _objectMetaData[dtfObject.GetType()].GetVector(date, position);
                
                //Check for invalid bridging
                if(Owner.ValidateOperations && !vector.Head.Validate())
                    throw new InvalidBridgingException();

                return vector;
            }
            
            internal SprigVector GetCertaintySignature(ulong date, SprigNode head) {
                var output = new SprigVector();
                var objects = head.GetRootedObjects();
                
                //Get a "partial" vector
                while (head.Index >= date)
                    head = (SprigNode) head.Last;
                

                foreach (var obj in objects) {
                    var meta = _objectMetaData[obj.GetType()];
                    var vector = new PositionVector(new PositionNode(null, 0, Position.Alloc(obj.GetType(), true)));
                    
                    //Navigate backwards from head and constrain at the start of every first instance of a position
                    var current = head;
                    var positionIndex = current.GetPosition(obj);

                    while (positionIndex.Uncertainty > -1) {
                        var lastPosition = current.Last == null ? new Position(obj.GetType()) : ((SprigNode) current.Last).GetPosition(obj);

                        if (positionIndex.Equals(lastPosition)) {
                            //Acquire a vector for the superposition that exists at current but not before it
                            vector &= meta.GetVector(current.Index, (positionIndex ^ lastPosition) & positionIndex);
                        }

                        current = (SprigNode) current.Last;
                        positionIndex &= lastPosition;
                    }
                    
                    output.Add(obj, vector);
                }

                return output;
            }

            #endregion

            internal class TypeMetaData
            {
                private readonly Type _type;
                public LateralTranslation LateralTranslation { get; }
                
                public Dictionary<Position, PositionMetaData> PreComputedVectors { get; }
                
                public TypeMetaData(Type type) {
                    _type = type;
                    LateralTranslation = new LateralTranslation();
                    PreComputedVectors = new Dictionary<Position, PositionMetaData>();
                }

                public PositionVector Translate(string key, PositionVector input) {
                    var translateMeta = LateralTranslation.GetTypeMetaData(key);
                    
                    var currentIn = input.Head;
                    var output = translateMeta.GetVector(currentIn.Index, LateralTranslation[key, currentIn.SuperPosition]);

                    while (currentIn.Last != null) {
                        var auxillaryIndex = currentIn.Index - 1;
                        currentIn = (PositionNode) currentIn.Last;
                        output &= translateMeta.GetVector(currentIn.Index, LateralTranslation[key, currentIn.SuperPosition]) & 
                                  translateMeta.GetVector(auxillaryIndex, LateralTranslation[key, currentIn.SuperPosition]);
                    }

                    return output;

                }

                public PositionVector GetVector(ulong date, Position position) {
                    var timelineVector = new PositionVector(new PositionNode(null, 0, Position.Alloc(_type)));

                    var eigenVals = position.GetEigenstates();

                    foreach (var val in eigenVals)
                    {
                        timelineVector |= PreComputedVectors[val].TimelineVector;
                    }
                    
                    if(date < SprigCenter)
                        timelineVector.ShiftBackward(SprigCenter - date);
                    
                    if(date > SprigCenter)
                        timelineVector.ShiftForward(date - SprigCenter);

                    return timelineVector;
                }
            }


            internal class PositionMetaData {
                
                public PositionVector TimelineVector { get; }
                
                public long Length { get; }
                
                public PositionMetaData(PositionVector timelineVector, long length) {
                    TimelineVector = timelineVector;
                    Length = length;
                }
            }

            internal class LateralTranslation {
                private readonly Dictionary<string, Dictionary<Position, Position>> _positionMaps = new Dictionary<string, Dictionary<Position, Position>>();
                private readonly Dictionary<string, TypeMetaData> _types = new Dictionary<string, TypeMetaData>();
                private readonly Dictionary<string, string> _proxy = new Dictionary<string, string>();

                public Position this[string key, Position input] {
                    get {
                        var latMap = _positionMaps[_proxy.ContainsKey(key) ? _proxy[key] : key];
                        var eigenvals = input.GetEigenstates();

                        var superPosition = latMap[eigenvals[0]];

                        for (var i = 1; i < eigenvals.Count; i++) {
                            superPosition |= latMap[eigenvals[i]];
                        }

                        return superPosition;
                    }
                }

                public bool Contains(string key) {
                    return _positionMaps.ContainsKey(key);
                }

                public Dictionary<Position, Position> GetPositionMap(string key) {
                    return _positionMaps[key];
                }

                public TypeMetaData GetTypeMetaData(string key) {
                    return _types[key];
                }

                public void AddType(string key, TypeMetaData meta) {
                    _positionMaps[key] = new Dictionary<Position, Position>();
                    _types[key] = meta;
                }

                public void AddProxy(string input, string output) {
                    _proxy[input] = output;
                }
                
            }
        }
    }
}