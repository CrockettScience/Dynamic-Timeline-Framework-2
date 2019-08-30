using System;

namespace DynamicTimelineFramework.Internal
{
    internal static class BitHacks
    {
        
        private static readonly Random Random = new Random();
        
        public static int SelectRandomSetBit(long num)
        {
            var flag = (ulong) num;
            
            var a = flag - ((flag >> 1) & ~0UL / 3);
            var b = (a            & ~0UL / 5) + ((a >> 2) & ~0UL / 5);
            var c = (b + (b                         >> 4)) & ~0UL / 0x11;
            var d = (c + (c                         >> 8)) & ~0UL / 0x101;
            var t = ((d >> 32) + (d >> 48));
            var   n = (int)((d * (~(ulong)0 / 255)) >> (64 - 1) * 8);
            ulong r = (uint) Random.Next(1, n +1);
            ulong s = 64;

            s -= ((t - r) & 256) >> 3;
            r -= (t & ((t      - r) >> 8));
            t =  (d >> (int)(s - 16)) & 0xff;
            s -= ((t - r) & 256) >> 4;
            r -= (t & ((t      - r) >> 8));
            t =  (c >> (int)(s - 8)) & 0xf;
            s -= ((t - r) & 256) >> 5;
            r -= (t & ((t      - r) >> 8));
            t =  (b >> (int)(s - 4)) & 0x7;
            s -= ((t - r) & 256) >> 6;
            r -= (t & ((t      - r) >> 8));
            t =  (a >> (int)(s - 2)) & 0x3;
            s -= ((t - r) & 256) >> 7;
            r -= (t & ((t      - r) >> 8));
            t =  (flag >> (int)(s - 1)) & 0x1;
            s -= ((t - r) & 256) >> 8;

            return (int)(s -1);
        }
	
        public static int NumberOfSetBits(long num)
        {
            var i = (ulong) num;
            
            i = i - ((i >> 1) & 0x5555555555555555UL);
            i = (i            & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }
    }
}