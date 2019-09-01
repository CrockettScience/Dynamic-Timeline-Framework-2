using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Exception;
using DynamicTimelineFramework.Internal;
using DynamicTimelineFramework.Internal.Interfaces;
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

            private const string PARENT_KEY = "PARENT";
            
            //2^63
            public const ulong SPRIG_CENTER = 9_223_372_036_854_775_808;

            private readonly Map<Type, IMetaData<DTFObject>> _objectMetaData = new Map<Type, IMetaData<DTFObject>>();
            
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
                            //"PARENT" is a reserved lateral key
                            if(lateralObjectAttribute.LateralKey == PARENT_KEY)
                                throw new DTFObjectCompilerException(type.Name + " uses off-limits lateral key \"" + PARENT_KEY + "\"");
                            
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
                            //"PARENT" is a reserved lateral key
                            //if(lateralPositionAttributes.LateralKey == PARENT_KEY)
                                //throw new DTFObjectCompilerException(type.Name + " uses off-limits lateral key \"" + PARENT_KEY + "\"");
                            
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
                            //The field must be static+
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
                var vector = ((MetaData<T>) _objectMetaData[typeof(T)]).GetSprigVector(position);
                    
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

            internal void PushConstraints<T>(T dtfObj, Diff diff) where T : DTFObject
            {
                //Get the metadata object
                var meta = (MetaData<T>) _objectMetaData[typeof(T)];
                
                //Iterate through each key and recursively constrain
                foreach (var key in meta.LateralKeys)
                {
                    if (!Constrain(dtfObj, key, meta, diff)) continue;
                    
                    //If the constraint results in change, then push it's constraints
                    
                    //Get the generic method for the field's type
                    var field = meta.LateralFields[key];
                        
                    var fieldType = field.FieldType;
                    var typeSpecificPushConstrain = typeof(ObjectCompiler).GetMethod("PushConstraints").MakeGenericMethod(fieldType);
                        
                    //Get the field object
                    var otherObject = field.GetValue(dtfObj);
                        
                    //Execute the method
                    typeSpecificPushConstrain.Invoke(this, new[] {otherObject, diff});
                }
                
            }

            internal void PullConstraints<T>(T dtfObj, Diff diff) where T : DTFObject
            {
                
                //Get the metadata object
                var meta = (MetaData<T>) _objectMetaData[typeof(T)];
                
                var parentNode = meta.Graph[PARENT_KEY];

                
                if (parentNode == null)
                    //It has no parents, no need to pull any constraints
                    return;
                
                //It has a parent, so pull its parent's constraints
                
                //Get the generic method for the field's type
                var field = meta.LateralFields[PARENT_KEY];
                        
                var fieldType = field.FieldType;
                var typeSpecificPullConstrain = typeof(ObjectCompiler).GetMethod("PullConstraints").MakeGenericMethod(fieldType);
                        
                //Get the field object
                var otherObject = field.GetValue(dtfObj);

                typeSpecificPullConstrain.Invoke(this, new[] {otherObject, diff});
                
                //Now that we know, after some amount of recursion, that the parent object
                //is fully constrained, we can constrain by it
                
                //Get the generic constrain method
                var typeSpecificConstrain = typeof(ObjectCompiler).GetMethod("Constraint").MakeGenericMethod(fieldType);
                
                //Get the meta object for the other object
                var otherMeta = _objectMetaData[fieldType];
                
                //Constrain
                typeSpecificConstrain.Invoke(null, new[] {otherObject, PARENT_KEY, otherMeta, diff});
            }

            private static bool Constrain<T>(T source, string key, MetaData<T> meta, Diff diff) where T : DTFObject
            {
                
                //Get the field that holds the object
                var field = meta.LateralFields[key];
                var fieldType = field.FieldType;
                
                //Get the object to push the constraint to
                var dest = (DTFObject) field.GetValue(source);
                
                //Get the correct generic method for getting the sprig
                var getSprigMethod = fieldType.GetMethod("GetSprig").MakeGenericMethod(fieldType);
                
                //Get the correct generic method for translate
                var translateMethod = meta.GetTranslateGenericMethod.MakeGenericMethod(fieldType);
                
                //Get the sprig
                var sprig = getSprigMethod.Invoke(dest, new object[]{diff});
                
                //Get the correct generic method to AND the sprigs together
                var andMethod = sprig.GetType().GetMethod("And");
                
                //Get the translation vector
                var translationVector = translateMethod.Invoke(meta, new object[]{key, source.GetSprig<T>(diff).ToVector()});
                
                //AND the sprigs together and check if there was a change
                return (bool) andMethod.Invoke(sprig, new [] {translationVector});
            }
            

            private class MetaData<T> : IMetaData<T> where T : DTFObject
            {
                public ObjectNode Graph { get; }
                public Map<string, FieldInfo> LateralFields { get; }
                
                public List<string> LateralKeys { get; }

                public MethodInfo GetTranslateGenericMethod
                {
                    get
                    {
                        return GetType().GetMethod("Translate", new []{typeof(string), typeof(SprigVector<T>)});
                        
                    }
                }
                
                private Map<Position<T>, SprigVector<T>> PreComputedSprigs { get; }
                
                private IMap<string, IMap<Position<T>, IPosition<DTFObject>>> LateralTranslation { get; }
                
                public MetaData(ObjectNode graph, Map<string, FieldInfo> lateralFields, List<string> lateralKeys, IMap<string, IMap<Position<T>, IPosition<DTFObject>>> lateralTranslation, Map<Position<T>, SprigVector<T>> preComputedSprigs)
                {
                    Graph = graph;
                    LateralFields = lateralFields;
                    LateralKeys = lateralKeys;
                    LateralTranslation = lateralTranslation;
                    PreComputedSprigs = preComputedSprigs;
                }

                public Position<TOut> Translate<TOut>(string key, Position<T> input) where TOut : DTFObject
                {
                    Position<TOut> output;

                    try
                    {
                        output = (Position<TOut>) LateralTranslation[key][input];
                    }
                    catch (InvalidCastException)
                    {
                        //This means that the key given does not match the type of the lateral object
                        throw new ArgumentException("The given key does not map to the correct lateral type");
                    }

                    return output;
                }

                public SprigVector<TOut> Translate<TOut>(string key, SprigVector<T> input) where TOut : DTFObject
                {
                    var currentIn = input.Head;
                    var currentOut = new SprigNode<TOut>(null, Translate<TOut>(key, currentIn.Position), currentIn.Index);
                    var output = new SprigVector<TOut>(currentOut);

                    while (currentIn.Last != null)
                    {
                        currentIn = currentIn.Last;
                        
                        currentOut.Last = new SprigNode<TOut>(null, Translate<TOut>(key, currentIn.Position), currentIn.Index);

                        currentOut = currentOut.Last;
                    }

                    return output;

                }

                public SprigVector<T> GetSprigVector(Position<T> position)
                {
                    var eigenValues = position.GetEigenValues();
                    var vector = new SprigVector<T>();

                    return eigenValues.Aggregate(vector, (current, eigenValue) => current | PreComputedSprigs[eigenValue]);
                }
            }
        }
    }
}