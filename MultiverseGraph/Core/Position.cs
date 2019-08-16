using System;
using System.Collections;
using System.Collections.Generic;

namespace MultiverseGraph.Core
{
    public struct Position
    {
        public BitArray Bits { get; }

        public int Size => Bits.Length;

        public int Uncertainty
        {
            get
            {
                var uncertainty = -1;
                
                foreach (bool bit in Bits)
                {
                    if (bit)
                        uncertainty++;
                }

                return uncertainty;
            }
        }

        public Position(int length, bool defaultValue)
        {
            Bits = new BitArray(length, defaultValue);
        }

        public Position(int length)
        {
            Bits = new BitArray(length);
        }

        public Position(bool[] bools)
        {
            Bits = new BitArray(bools);
        }

        public Position(byte[] bytes)
        {
            Bits = new BitArray(bytes);
        }

        public Position(int[] ints)
        {
            Bits = new BitArray(ints);
        }

        public Position(BitArray bits)
        {
            Bits = new BitArray(bits);
        }

        public Position(Position position)
        {
            Bits = new BitArray(position.Bits);
        }

        //CSV constructor for compile time / one-time construction
        public Position(string csv)
        {
            Bits = new BitArray(Array.ConvertAll(csv.Split(','), int.Parse));
        }
        
        public Position(string[] values)
        {
            Bits = new BitArray(Array.ConvertAll(values, int.Parse));
        }

        public Position Collapse(Random rand)
        {
            var setBits = new List<int>(Size);

            for (var i = 0; i < Size; i++)
            {
                if (Bits[i])
                    setBits.Add(i);
            }

            var setPosition = setBits[rand.Next(setBits.Count)];

            var bools = new bool[setPosition + 1];
            bools[setPosition] = true;
            
            return new Position(bools);
        }

        public static Position operator |(Position left, Position right)
        {
            
            var longest = left.Size > right.Size ? left : right;
            var shortest = longest == left ? right : left;
            
            var newValue = new BitArray(longest.Bits);
            
            for (var i = 0; i < shortest.Size; i++)
            {
                newValue[i] |= shortest.Bits[i];
            }
            
            return new Position(newValue);
        }
        
        public static Position operator &(Position left, Position right)
        {
            
            var longest = left.Size > right.Size ? left : right;
            var shortest = longest == left ? right : left;
            
            var newValue = new BitArray(shortest.Bits);
            
            for (var i = 0; i < shortest.Size; i++)
            {
                shortest.Bits[i] &= longest.Bits[i];
            }
            
            // The rest can stay false

            return new Position(newValue);
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Bits == right.Bits;
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position right)) return false;
            
            var longest = Size > right.Size ? this : right;
            var shortest = longest == this ? right : this;

            var i = 0;
            for (; i < shortest.Size; i++)
            {
                if (Bits[i] != right.Bits[i])
                    return false;
            }

            //The rest on the longest must ALL be 0
            for (; i < longest.Size; i++)
            {
                if (longest.Bits[i])
                    return false;
            }

            return true;

        }

        public override int GetHashCode()
        {
            var count = 0;
            var shuffleHash = int.MaxValue;

            for (var i = 0; i < Size; i++)
            {
                if (Bits[i])
                {
                    count++;
                    shuffleHash ^= i;
                }
            }

            return (count * 397) ^ shuffleHash;

        }
    }
}