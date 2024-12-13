using UnityEngine;
using System;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Dependencies")]
    public MapGenerationConfig MapGenerationConfig;
    public TileTypeDataMappingConfig TileTypeDataMappingConfig;

    public event Action<Dictionary<Vector2Int, TileTypeData>> OnMapGenerated;

    private Dictionary<Vector2Int, TileTypeData> generatedMapData;
    public Dictionary<Vector2Int, TileTypeData> GeneratedMapData => generatedMapData;

    public void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Applying TileTypeData...");

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("ProceduralMapGenerator: Received empty tile data.");
            return;
        }

        generatedMapData = new Dictionary<Vector2Int, TileTypeData>();

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            // Generate TileTypeData using map generation logic
            float perlinValue = GeneratePerlinValue(gridPosition.x, gridPosition.y, GetPerlinOffsets(new System.Random(MapGenerationConfig.Seed)));
            TileTypeData tileTypeData = GetTileTypeDataFromNoise(perlinValue);

            tile.SetTileTypeData(tileTypeData);

            generatedMapData[gridPosition] = tileTypeData;
        }

        Debug.Log("ProceduralMapGenerator: TileTypeData applied successfully.");
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
        float amplitude = MapGenerationConfig.InitialAmplitude;
        float frequency = MapGenerationConfig.InitialFrequency;
        float noiseHeight = 0f;

        for (int i = 0; i < MapGenerationConfig.Octaves; i++)
        {
            float sampleX = (col / MapGenerationConfig.NoiseScale) * frequency + octaveOffsets[i].x;
            float sampleY = (row / MapGenerationConfig.NoiseScale) * frequency + octaveOffsets[i].y;

            float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * MapGenerationConfig.NoiseAdjustmentFactor) - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= MapGenerationConfig.Persistence;
            frequency *= MapGenerationConfig.Lacunarity;
        }

        return Mathf.InverseLerp(MapGenerationConfig.NoiseMin, MapGenerationConfig.NoiseMax, noiseHeight);
    }

    private TileTypeData GetTileTypeDataFromNoise(float noiseValue)
    {
        foreach (var mapping in TileTypeDataMappingConfig.TileMappings)
        {
            if (noiseValue >= mapping.MinNoiseValue && noiseValue <= mapping.MaxNoiseValue)
            {
                return mapping.TileTypeData;
            }
        }

        Debug.LogWarning($"ProceduralMapGenerator: No TileTypeData found for noise value {noiseValue}. Using fallback.");
        return TileTypeDataMappingConfig.FallbackTileTypeData;
    }
}
