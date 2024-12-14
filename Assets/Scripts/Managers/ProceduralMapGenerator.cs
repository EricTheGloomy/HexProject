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
    private Dictionary<Vector2Int, float> precomputedNoise; // Precomputed noise

    public Dictionary<Vector2Int, TileTypeData> GeneratedMapData => generatedMapData;

    public void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Applying TileTypeData...");

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("ProceduralMapGenerator: Received empty tile data.");
            return;
        }

        PrecomputeNoise(tiles); // Precompute noise

        generatedMapData = new Dictionary<Vector2Int, TileTypeData>();

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            float perlinValue = precomputedNoise[gridPosition]; // Use precomputed noise
            TileTypeData tileTypeData = GetTileTypeDataFromNoise(perlinValue);

            tile.SetTileTypeData(tileTypeData);
            generatedMapData[gridPosition] = tileTypeData;
        }

        Debug.Log("ProceduralMapGenerator: TileTypeData applied successfully.");
        OnMapGenerated?.Invoke(generatedMapData);
    }

    private void PrecomputeNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Precomputing Perlin noise...");

        precomputedNoise = new Dictionary<Vector2Int, float>();
        Vector2[] octaveOffsets = GetPerlinOffsets(new System.Random(MapGenerationConfig.Seed));

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            float perlinValue = GeneratePerlinValue(gridPosition.x, gridPosition.y, octaveOffsets);
            precomputedNoise[gridPosition] = perlinValue;
        }

        Debug.Log($"ProceduralMapGenerator: Precomputed noise for {precomputedNoise.Count} tiles.");
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

        // Apply the noise height multiplier
        noiseHeight *= MapGenerationConfig.NoiseHeightMultiplier;

        // Clamp the final noise height
        noiseHeight = Mathf.Clamp(noiseHeight, MapGenerationConfig.MinHeightClamp, MapGenerationConfig.MaxHeightClamp);

        // Normalize the clamped noise height
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

    private void GenerateTileAttributes(Tile tile, Vector2Int gridPosition)
    {
        //tile.Attributes.Elevation = GeneratePerlinValue(gridPosition.x, gridPosition.y, elevationNoiseSettings);
        //tile.Attributes.Moisture = GeneratePerlinValue(gridPosition.x, gridPosition.y, moistureNoiseSettings);
        //tile.Attributes.Temperature = GeneratePerlinValue(gridPosition.x, gridPosition.y, temperatureNoiseSettings);

        // Example: Assign vegetation based on attributes
        //tile.Attributes.HasVegetation = tile.Attributes.Moisture > 0.7f && tile.Attributes.Elevation < 0.5f;
    }
}
