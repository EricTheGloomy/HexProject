using UnityEngine;
using System;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Dependencies")]
    public MapConfig MapConfig;
    public MapGenerationConfig MapGenerationConfig;
    public TileTypeMappingConfig TileTypeMappingConfig;

    public event Action<Dictionary<Vector2Int, TileData>> OnMapGenerated;

    private Dictionary<Vector2Int, TileData> generatedMapData;
    public Dictionary<Vector2Int, TileData> GeneratedMapData => generatedMapData;

    public void GenerateMap()
    {
        if (MapConfig == null || MapGenerationConfig == null || TileTypeMappingConfig == null)
        {
            Debug.LogError("MapConfig, MapGenerationConfig, or TileTypeMappingConfig is missing!");
            return;
        }

        // Generate map data
        Dictionary<Vector2Int, TileData> mapData = new Dictionary<Vector2Int, TileData>();
        System.Random prng = new System.Random(MapGenerationConfig.Seed);
        Vector2[] octaveOffsets = GetPerlinOffsets(prng);

        for (int row = 0; row < MapConfig.MapHeight; row++)
        {
            for (int col = 0; col < MapConfig.MapWidth; col++)
            {
                float perlinValue = GeneratePerlinValue(col, row, octaveOffsets);
                TileData tileType = GetTileTypeFromNoise(perlinValue);
                mapData[new Vector2Int(col, row)] = tileType;
            }
        }

        generatedMapData = mapData; // Correctly assign the populated map data

        Debug.Log("ProceduralMapGenerator: Map generation complete!");
        OnMapGenerated?.Invoke(generatedMapData);
    }

    private Vector2[] GetPerlinOffsets(System.Random prng)
    {
        Vector2[] offsets = new Vector2[MapGenerationConfig.Octaves];
        for (int i = 0; i < MapGenerationConfig.Octaves; i++)
        {
            float offsetX = prng.Next(MapGenerationConfig.OffsetRangeMin, MapGenerationConfig.OffsetRangeMax);
            float offsetY = prng.Next(MapGenerationConfig.OffsetRangeMin, MapGenerationConfig.OffsetRangeMax);
            offsets[i] = new Vector2(offsetX, offsetY);
        }
        return offsets;
    }

    private float GeneratePerlinValue(int col, int row, Vector2[] octaveOffsets)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < MapGenerationConfig.Octaves; i++)
        {
            float sampleX = (col / MapGenerationConfig.NoiseScale) * frequency + octaveOffsets[i].x;
            float sampleY = (row / MapGenerationConfig.NoiseScale) * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= MapGenerationConfig.Persistence;
            frequency *= MapGenerationConfig.Lacunarity;
        }

        return Mathf.InverseLerp(MapGenerationConfig.NoiseMin, MapGenerationConfig.NoiseMax, noiseHeight);
    }

    private TileData GetTileTypeFromNoise(float noiseValue)
    {
        foreach (var mapping in TileTypeMappingConfig.TileMappings)
        {
            if (noiseValue >= mapping.MinNoiseValue && noiseValue <= mapping.MaxNoiseValue)
            {
                return mapping.TileType;
            }
        }

        throw new InvalidOperationException($"No TileType found for noise value {noiseValue}");
    }
}
