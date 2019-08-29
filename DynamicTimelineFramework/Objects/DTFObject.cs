using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicTimelineFramework.Core;
using DynamicTimelineFramework.Core.Tools.Sprig;
using DynamicTimelineFramework.Exception;

namespace DynamicTimelineFramework.Objects {
    public abstract class DTFObject {
        #region STATIC

        public const string FORWARD_RULE_KEY = "SELF_FORWARD";
        public const string BACKWARD_RULE_KEY = "SELF_BACKWARD";
        
        internal static Dictionary<Type, LinkedList<string>> LateralObjectKeys { get; }
        internal static Dictionary<Type, Dictionary<string, FieldInfo>> LateralObjectFields { get; }
        internal static Dictionary<Type, Dictionary<string, Dictionary<Position, Position>>> RuleSets { get; }

        static DTFObject() {
            //Yuck. Just gets all types that inherit DTFObject in the assembly
            //Todo - If I compile this to a .dll, and reference it as a library, will this refer to the correct assembly?
            var typesForCompile = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(DTFObject).IsAssignableFrom(t));
            
            LateralObjectKeys = new Dictionary<Type, LinkedList<string>>();
            LateralObjectFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            RuleSets = new Dictionary<Type, Dictionary<string, Dictionary<Position, Position>>>();
            
            foreach (var type in typesForCompile) {
                //Gather the lateral object references
                var fields = type.GetFields();

                foreach (var field in fields) {
                    var lateralObjectAttr = Attribute.GetCustomAttribute(field, typeof(LateralObjectAttribute)) as LateralObjectAttribute;
                    var lateralRuleAttrs = Attribute.GetCustomAttributes(field, typeof(LateralRuleAttribute)) as LateralRuleAttribute[];
                    var forwardRuleAttr = Attribute.GetCustomAttribute(field, typeof(ForwardRuleAttribute)) as ForwardRuleAttribute;

                    var fieldType = field.GetType();
                    
                    if (lateralObjectAttr != null) {
                        //We are supposedly looking at a field representing a lateral object
                        //definition and need to key in the field info. 
                        
                        //First, assert that the value type is valid
                        if (typeof(DTFObject).IsAssignableFrom(fieldType) || typeof(IEnumerable<DTFObject>).IsAssignableFrom(fieldType)) {
                            if(!LateralObjectFields.ContainsKey(type))
                                LateralObjectFields[type] = new Dictionary<string, FieldInfo>();
                            
                            if(!LateralObjectKeys.ContainsKey(type))
                                LateralObjectKeys[type] = new LinkedList<string>();
                            
                            if(!LateralObjectFields.ContainsKey(type))
                                LateralObjectFields[fieldType] = new Dictionary<string, FieldInfo>();
                            
                            if(!LateralObjectKeys.ContainsKey(type))
                                LateralObjectKeys[fieldType] = new LinkedList<string>();

                            //If it already contains a key, throw an exception
                            if(LateralObjectFields[type].ContainsKey(lateralObjectAttr.LateralKey))
                                throw new DTFObjectCompilerException(lateralObjectAttr.LateralKey + " for type " + type.FullName + " has assigned more than once");
                            
                            if(LateralObjectFields[fieldType].ContainsKey(lateralObjectAttr.LateralKey))
                                throw new DTFObjectCompilerException(lateralObjectAttr.LateralKey + " for type " + fieldType.FullName + " has assigned more than once");

                            
                            //Ensure symmetry
                            LateralObjectFields[fieldType][lateralObjectAttr.LateralKey] = field;
                            LateralObjectFields[type][lateralObjectAttr.LateralKey] = field;
                            LateralObjectKeys[type].AddLast(lateralObjectAttr.LateralKey);
                            LateralObjectKeys[fieldType].AddLast(lateralObjectAttr.LateralKey);
                        }

                        else {
                            throw new DTFObjectCompilerException(field.Name + " of class " + type.FullName + " has a LateralObjectAttribute, but it's value is not assignable from " + typeof(DTFObject).Name + " or IEnumerable<" + typeof(DTFObject).Name + ">.");
                        }
                    }
                    
                    else if (forwardRuleAttr != null) {
                        //we are supposedly looking at a field representing a singular position and
                        //need to key in both a forward rule and possibly a set of lateral rules
                        
                        //First, we assert the value type is valid
                        if (fieldType == typeof(Position)) {
                            if(!RuleSets.ContainsKey(type))
                                RuleSets[type] = new Dictionary<string, Dictionary<Position, Position>>();
                            
                            if(!RuleSets[type].ContainsKey(FORWARD_RULE_KEY))
                                RuleSets[type][FORWARD_RULE_KEY] = new Dictionary<Position, Position>();
                            
                            if(!RuleSets[type].ContainsKey(BACKWARD_RULE_KEY))
                                RuleSets[type][BACKWARD_RULE_KEY] = new Dictionary<Position, Position>();

                            var fieldPosition = (Position) field.GetValue(null);
                            
                            //Key in the forward rule
                            RuleSets[type][FORWARD_RULE_KEY][fieldPosition] = forwardRuleAttr.Range;
                            
                            //Key in backward rule. We can check by masking the range with each
                            //position we go through, checking for positive uncertainty, and
                            //thereby reverse constructing the domain
                            foreach (var maskField in fields) {
                                if (maskField.GetCustomAttribute(typeof(ForwardRuleAttribute)) is ForwardRuleAttribute) {

                                    var range = forwardRuleAttr.Range;
                                    if ((fieldPosition & range).Uncertainty >= 0) {
                                        if (RuleSets[type][BACKWARD_RULE_KEY].ContainsKey((Position) maskField.GetValue(null)))
                                            RuleSets[type][BACKWARD_RULE_KEY][(Position) maskField.GetValue(null)] |= fieldPosition;

                                        else
                                            RuleSets[type][BACKWARD_RULE_KEY][(Position) maskField.GetValue(null)] = new Position(fieldPosition);
                                    }
                                }
                            }
                            
                            //TODO - define lateral rule sets
                            
                        }
                        
                        else {
                            throw new DTFObjectCompilerException(field.Name + " of class " + type.FullName + " has a Rule Attribute, but it's value is not of type " + typeof(Position).Name);
                        }
                    }
                }
            }
        }
        
        #endregion
        
        private Dictionary<Diff, Sprig> _sprigs;
        internal Sprig this[Diff diff] => _sprigs[diff];

        internal void Collapse(SprigVector vector, Diff diff) {
            
        }

        #region ATTRIBUTES
        
        [AttributeUsage(AttributeTargets.Field)]
        public class ForwardRuleAttribute : Attribute {
            public Position Range { get; }

            public ForwardRuleAttribute(Position range) {
                Range = range;
            }
        }
        
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public class LateralRuleAttribute : Attribute {
            public string LateralKey { get; }
            public Position Range { get; }

            public LateralRuleAttribute(string lateralKey, Position range) {
                LateralKey = lateralKey;
                Range = range;
            }
        }


        [AttributeUsage(AttributeTargets.Field)]
        public class LateralObjectAttribute : Attribute {
            public string LateralKey { get; }

            public LateralObjectAttribute(string lateralKey) {
                LateralKey = lateralKey;
            }
        }

        #endregion
    }
}