using System;
using System.Collections.Generic;

namespace MultiverseGraph.Core.Compiler
{
    /*internal static class HandleCompiler
    {
        private static Dictionary<Type, Dictionary<Position, Heuristics>> HandleSpecifications{ get; set; }
        private static Dictionary<Type, List<Position>> HandlePositions{ get; set; }
        
        public static Position GetNextPossibilityMask(Type HandleType, Position Position){
            try{
                //Mask for each Handle and return all possibilities OR'd together
                var typeHandles    = HandleBits[HandleType];
                var HandleSpecs     = HandleSpecifications[HandleType];
                var possibilityPosition = new Position();

                foreach (var mask in typeHandles){
                    if ((Position & mask) != Paradox){
                        possibilityPosition |= HandleSpecs[mask].NextSpecification.AcceptableHandles;
                    }
                }

                return possibilityPosition;

            }
            catch (KeyNotFoundException){
                throw new StateNotDefinedException();
            }
        }
        
        public static Position GetPrevPossibilityMask(Type stateType, Position Position){
            try{
                //Mask for each state and return all possibilities OR'd together
                var typeStates      = StateBits[stateType];
                var stateSpecs      = StateSpecifications[stateType];
                var possibilityPosition = new Position();

                foreach (var mask in typeStates){
                    if ((Position & mask) != Paradox){
                        possibilityPosition |= stateSpecs[mask].PrevSpecification.AcceptableStates;
                    }
                }

                return possibilityPosition;

            }
            catch (KeyNotFoundException){
                throw new StateNotDefinedException();
            }
        }
        
        internal static void Compile(Type handle, Type objectId)
        {
            var HandleFields = handle.GetFields();
            var HandleHeuristics = new Dictionary<Position, Specifications>();
            var bitSet = new List<Position>();
            HandleSpecifications[objectId] = HandleSpec;
            HandlePositions[objectId] = bitSet;

            foreach (var field in HandleFields){
                
                //Get the Specs from the attributes
                var nextSpecAttr   = Attribute.GetCustomAttribute(field, typeof(NextStateSpecificationAttribute)) as NextStateSpecificationAttribute;
                var childSpecAttrs = Attribute.GetCustomAttributes(field, typeof(ChildStateSpecificationAttribute)) as ChildStateSpecificationAttribute[];

                //If no specs, then not valid field
                if (!(nextSpecAttr != null || childSpecAttrs != null)) continue;
                
                var val = (Position) field.GetValue(null);
                var acceptablePrevStates = new Position();
                
                //Compile reverse states
                foreach (var maskField in StateFields){
                    var maskVal = ((NextStateSpecificationAttribute)Attribute.GetCustomAttribute(maskField, typeof(NextStateSpecificationAttribute))).AcceptableStates;
                    if ((val & maskVal) != Paradox){
                        acceptablePrevStates |= (Position) maskField.GetValue(null);
                    }
                }
                
                
                var spec = new Specifications(childSpecAttrs, nextSpecAttr, new PrevStateSpecification(acceptablePrevStates));

                stateSpec[val] = spec;
                StateBits[objectId].Add(val);
            }
        }
    }
    */
}