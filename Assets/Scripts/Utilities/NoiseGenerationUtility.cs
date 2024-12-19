using System;
using UnityEngine;

public static class NoiseGenerationUtility
{

    public static float GeneratePerlinValue(int x, int y, float scale, int octaves, float persistence, float lacunarity, float minClamp = -1f, float maxClamp = 1f)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / scale) * frequency;
            float sampleY = (y / scale) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f; // Map Perlin range [-1, 1]
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Clamp and normalize the noise height
        noiseHeight = Mathf.Clamp(noiseHeight, minClamp, maxClamp);
        return Mathf.InverseLerp(minClamp, maxClamp, noiseHeight);
    }

    public static Vector2[] GetPerlinOffsets(int octaves, int offsetRangeMin, int offsetRangeMax, System.Random random)
    {
        Vector2[] offsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(offsetRangeMin, offsetRangeMax);
            float offsetY = random.Next(offsetRangeMin, offsetRangeMax);
            offsets[i] = new Vector2(offsetX, offsetY);
        }
        return offsets;
    }
}
