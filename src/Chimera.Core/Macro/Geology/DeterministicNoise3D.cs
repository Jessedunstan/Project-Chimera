using System;
using System.Runtime.CompilerServices;

namespace Chimera.Core.Macro.Geology;

/// <summary>
/// Pure, deterministic 2D and 3D coherent noise evaluator.
/// Completely decoupled from platform architecture, floating point rounding differences, or external libraries.
/// Zero heap allocations.
/// </summary>
public static class DeterministicNoise3D
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastFloor(float f) => f >= 0.0f ? (int)f : (int)f - 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fade(float t) => t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t) => a + t * (b - a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint HashCoords2D(ulong seed, int x, int y)
    {
        uint h = (uint)(seed ^ (seed >> 32));
        h ^= (uint)(x * 374761393);
        h = (h << 13) | (h >> 19);
        h ^= (uint)(y * 668265263);
        h = (h * 1274126177) ^ 0x85ebca6b;
        return h ^ (h >> 16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint HashCoords3D(ulong seed, int x, int y, int z)
    {
        uint h = (uint)(seed ^ (seed >> 32));
        h ^= (uint)(x * 374761393);
        h = (h << 13) | (h >> 19);
        h ^= (uint)(y * 668265263);
        h = (h << 17) | (h >> 15);
        h ^= (uint)(z * 83492791);
        h = (h * 1274126177) ^ 0x85ebca6b;
        return h ^ (h >> 16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad2D(uint hash, float x, float y)
    {
        uint h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad3D(uint hash, float x, float y, float z)
    {
        uint h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    /// <summary>
    /// Coherent 2D gradient noise in range [-1.0, 1.0].
    /// </summary>
    public static float Noise2D(ulong seed, float x, float y)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float dx = x - x0;
        float dy = y - y0;

        float u = Fade(dx);
        float v = Fade(dy);

        float n00 = Grad2D(HashCoords2D(seed, x0, y0), dx, dy);
        float n10 = Grad2D(HashCoords2D(seed, x1, y0), dx - 1.0f, dy);
        float n01 = Grad2D(HashCoords2D(seed, x0, y1), dx, dy - 1.0f);
        float n11 = Grad2D(HashCoords2D(seed, x1, y1), dx - 1.0f, dy - 1.0f);

        float nx0 = Lerp(n00, n10, u);
        float nx1 = Lerp(n01, n11, u);

        return Math.Clamp(Lerp(nx0, nx1, v) * 0.7071f, -1.0f, 1.0f);
    }

    /// <summary>
    /// Coherent 3D gradient noise in range [-1.0, 1.0].
    /// </summary>
    public static float Noise3D(ulong seed, float x, float y, float z)
    {
        int x0 = FastFloor(x);
        int y0 = FastFloor(y);
        int z0 = FastFloor(z);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        int z1 = z0 + 1;

        float dx = x - x0;
        float dy = y - y0;
        float dz = z - z0;

        float u = Fade(dx);
        float v = Fade(dy);
        float w = Fade(dz);

        float n000 = Grad3D(HashCoords3D(seed, x0, y0, z0), dx, dy, dz);
        float n100 = Grad3D(HashCoords3D(seed, x1, y0, z0), dx - 1.0f, dy, dz);
        float n010 = Grad3D(HashCoords3D(seed, x0, y1, z0), dx, dy - 1.0f, dz);
        float n110 = Grad3D(HashCoords3D(seed, x1, y1, z0), dx - 1.0f, dy - 1.0f, dz);

        float n001 = Grad3D(HashCoords3D(seed, x0, y0, z1), dx, dy, dz - 1.0f);
        float n101 = Grad3D(HashCoords3D(seed, x1, y0, z1), dx - 1.0f, dy, dz - 1.0f);
        float n011 = Grad3D(HashCoords3D(seed, x0, y1, z1), dx, dy - 1.0f, dz - 1.0f);
        float n111 = Grad3D(HashCoords3D(seed, x1, y1, z1), dx - 1.0f, dy - 1.0f, dz - 1.0f);

        float nx00 = Lerp(n000, n100, u);
        float nx10 = Lerp(n010, n110, u);
        float nx01 = Lerp(n001, n101, u);
        float nx11 = Lerp(n011, n111, u);

        float nxy0 = Lerp(nx00, nx10, v);
        float nxy1 = Lerp(nx01, nx11, v);

        return Math.Clamp(Lerp(nxy0, nxy1, w), -1.0f, 1.0f);
    }

    /// <summary>
    /// Multi-octave fractional Brownian motion 2D noise in range [-1.0, 1.0].
    /// </summary>
    public static float OctaveNoise2D(
        ulong seed,
        float x,
        float y,
        int octaves = 4,
        float persistence = 0.5f,
        float lacunarity = 2.0f)
    {
        float total = 0.0f;
        float frequency = 1.0f;
        float amplitude = 1.0f;
        float maxVal = 0.0f;

        for (int i = 0; i < octaves; i++)
        {
            total += Noise2D(seed + (ulong)(i * 1013), x * frequency, y * frequency) * amplitude;
            maxVal += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return maxVal > 0.0f ? total / maxVal : 0.0f;
    }

    /// <summary>
    /// Multi-octave fractional Brownian motion 3D noise in range [-1.0, 1.0].
    /// </summary>
    public static float OctaveNoise3D(
        ulong seed,
        float x,
        float y,
        float z,
        int octaves = 4,
        float persistence = 0.5f,
        float lacunarity = 2.0f)
    {
        float total = 0.0f;
        float frequency = 1.0f;
        float amplitude = 1.0f;
        float maxVal = 0.0f;

        for (int i = 0; i < octaves; i++)
        {
            total += Noise3D(seed + (ulong)(i * 1013), x * frequency, y * frequency, z * frequency) * amplitude;
            maxVal += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return maxVal > 0.0f ? total / maxVal : 0.0f;
    }

    /// <summary>
    /// 3D Cellular / Worley noise for discrete ore clusters, geodes, and kimberlite pipes.
    /// Returns the minimum distance to cell center in range [0.0, 1.0] and the cell's hash.
    /// </summary>
    public static float Cellular3D(ulong seed, float x, float y, float z, out uint cellHash)
    {
        int cellX = FastFloor(x);
        int cellY = FastFloor(y);
        int cellZ = FastFloor(z);

        float minDistanceSq = float.MaxValue;
        uint nearestHash = 0;

        for (int oz = -1; oz <= 1; oz++)
        {
            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    int nx = cellX + ox;
                    int ny = cellY + oy;
                    int nz = cellZ + oz;

                    uint hash = HashCoords3D(seed, nx, ny, nz);

                    // Deterministic jittered point within the neighbor cell
                    float px = nx + ((hash & 0xFF) / 255.0f);
                    float py = ny + (((hash >> 8) & 0xFF) / 255.0f);
                    float pz = nz + (((hash >> 16) & 0xFF) / 255.0f);

                    float dx = x - px;
                    float dy = y - py;
                    float dz = z - pz;

                    float distSq = dx * dx + dy * dy + dz * dz;
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        nearestHash = hash;
                    }
                }
            }
        }

        cellHash = nearestHash;
        return MathF.Sqrt(minDistanceSq);
    }
}
