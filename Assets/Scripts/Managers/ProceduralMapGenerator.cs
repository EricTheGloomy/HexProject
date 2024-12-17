using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProceduralMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Dependencies")]
    public MapGenerationConfig MapGenerationConfig;
    public TileTypeDataMappingConfig TileTypeDataMappingConfig;

    public event Action<Dictionary<Vector2Int, TileTypeData>> OnMapGenerated;

    private Dictionary<Vector2Int, TileTypeData> generatedMapData;
    private Dictionary<Vector2Int, float> precomputedNoise;

    public Dictionary<Vector2Int, TileTypeData> GeneratedMapData => generatedMapData;

    public void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Starting biome assignment...");

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("ProceduralMapGenerator: Received empty tile data.");
            return;
        }

        // Precompute procedural attributes
        PrecomputeNoise(tiles);

        // Pass 1: Assign based on elevation
        foreach (var tile in tiles.Values)
        {
            AssignElevationBiome(tile);
        }

        // Pass 2: Assign based on moisture
        foreach (var tile in tiles.Values)
        {
            AssignMoistureBiome(tile);
        }

        // Pass 3: Adjust based on temperature
        foreach (var tile in tiles.Values)
        {
            AdjustBiomeByTemperature(tile);
        }

        Debug.Log("ProceduralMapGenerator: TileTypeData assignment complete.");
        OnMapGenerated?.Invoke(GenerateMapData(tiles));
    }

    private void AssignElevationBiome(Tile tile)
    {
        float elevation = tile.Attributes.Procedural.Elevation;

        foreach (var mapping in TileTypeDataMappingConfig.ElevationMappings)
        {
            if (elevation >= mapping.MinElevation && elevation <= mapping.MaxElevation)
            {
                tile.SetTileTypeData(mapping.TileTypeData);
                tile.Attributes.Procedural.FixedElevationCategory = mapping.Category; // Mandatory assignment
                return; // Stop further checks once matched
            }
        }

        // Fallback if no match (shouldn't happen if mappings cover all ranges)
        tile.SetTileTypeData(TileTypeDataMappingConfig.FallbackTileTypeData);
        tile.Attributes.Procedural.FixedElevationCategory = TileTypeDataMappingConfig.ElevationCategory.Land; // Default to "Land" if no match
    }

    private void AssignMoistureBiome(Tile tile)
    {
        if (tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Land)
            return; // Skip Water and Mountain tiles

        float moisture = tile.Attributes.Procedural.Moisture;

        foreach (var mapping in TileTypeDataMappingConfig.MoistureMappings)
        {
            if (moisture >= mapping.MinMoisture && moisture <= mapping.MaxMoisture)
            {
                tile.SetTileTypeData(mapping.TileTypeData);
                return; // Stop further checks once matched
            }
        }

        // Fallback for unadjusted tiles (if needed)
        tile.SetTileTypeData(TileTypeDataMappingConfig.FallbackTileTypeData);
    }

    private void AdjustBiomeByTemperature(Tile tile)
    {
        if (tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Land)
            return; // Skip Water and Mountain tiles

        float temperature = tile.Attributes.Procedural.Temperature;

        foreach (var mapping in TileTypeDataMappingConfig.TemperatureMappings)
        {
            if (temperature >= mapping.MinTemperature && temperature <= mapping.MaxTemperature)
            {
                tile.SetTileTypeData(mapping.TileTypeData);
                return; // Stop further checks once matched
            }
        }

        // Fallback for unadjusted tiles (if needed)
        tile.SetTileTypeData(TileTypeDataMappingConfig.FallbackTileTypeData);
    }

    // Utility method to prepare final map data
    private Dictionary<Vector2Int, TileTypeData> GenerateMapData(Dictionary<Vector2, Tile> tiles)
    {
        var mapData = new Dictionary<Vector2Int, TileTypeData>();
        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            mapData[gridPosition] = tileEntry.Value.Attributes.TileTypeData;
        }
        return mapData;
    }

    private void PrecomputeNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Precomputing Perlin noise...");

        Vector2[] octaveOffsets = GetPerlinOffsets(new System.Random(MapGenerationConfig.Seed));

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            tile.Attributes.Procedural.Elevation = GeneratePerlinValue(gridPosition.x, gridPosition.y, MapGenerationConfig.ElevationScale, MapGenerationConfig.ElevationOctaves, MapGenerationConfig.ElevationPersistence, MapGenerationConfig.ElevationLacunarity);
            tile.Attributes.Procedural.Moisture = GeneratePerlinValue(gridPosition.x, gridPosition.y, MapGenerationConfig.MoistureScale, MapGenerationConfig.MoistureOctaves, MapGenerationConfig.MoisturePersistence, MapGenerationConfig.MoistureLacunarity);
            tile.Attributes.Procedural.Temperature = GeneratePerlinValue(gridPosition.x, gridPosition.y, MapGenerationConfig.TemperatureScale, MapGenerationConfig.TemperatureOctaves, MapGenerationConfig.TemperaturePersistence, MapGenerationConfig.TemperatureLacunarity);
        }

        Debug.Log($"ProceduralMapGenerator: Precomputed noise for {tiles.Count} tiles.");
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

    private float GeneratePerlinValue(int col, int row, float scale, int octaves, float persistence, float lacunarity)
    {
        float amplitude = MapGenerationConfig.InitialAmplitude;
        float frequency = MapGenerationConfig.InitialFrequency;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (col / scale) * frequency;
            float sampleY = (row / scale) * frequency;

            float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * MapGenerationConfig.NoiseAdjustmentFactor) - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        noiseHeight *= MapGenerationConfig.NoiseHeightMultiplier;
        noiseHeight = Mathf.Clamp(noiseHeight, MapGenerationConfig.MinHeightClamp, MapGenerationConfig.MaxHeightClamp);
        return Mathf.InverseLerp(MapGenerationConfig.NoiseMin, MapGenerationConfig.NoiseMax, noiseHeight);
    }

}
