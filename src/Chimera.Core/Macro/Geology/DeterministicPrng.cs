using System;
using System.Runtime.CompilerServices;

namespace Chimera.Core.Macro.Geology;

/// <summary>
/// High-speed, unmanaged 64-bit deterministic pseudo-random number generator (Xoshiro256**).
/// Guarantees bit-for-bit identical sequences across all OS and runtime environments.
/// Zero heap allocations.
/// </summary>
public struct DeterministicPrng
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    public DeterministicPrng(ulong seed)
    {
        // Initialize state using SplitMix64
        _s0 = SplitMix64(ref seed);
        _s1 = SplitMix64(ref seed);
        _s2 = SplitMix64(ref seed);
        _s3 = SplitMix64(ref seed);
        if ((_s0 | _s1 | _s2 | _s3) == 0UL)
        {
            _s0 = 0x8a5cd789635d2dffUL;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong NextUInt64()
    {
        ulong result = RotL(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = RotL(_s3, 45);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint NextUInt32() => (uint)(NextUInt64() >> 32);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt32(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive) return minInclusive;
        uint range = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextUInt32() % range);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloat()
    {
        // 24 bits of precision mapped to [0, 1)
        return (NextUInt64() >> 40) * (1.0f / 16777216.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float NextFloatRange(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotL(ulong x, int k) => (x << k) | (x >> (64 - k));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitMix64(ref ulong x)
    {
        ulong z = (x += 0x9e3779b97f4a7c15UL);
        z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9UL;
        z = (z ^ (z >> 27)) * 0x94d049bb133111ebUL;
        return z ^ (z >> 31);
    }
}
