using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElevationGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;
    private readonly TileTypeDataMappingConfig mappingConfig;

    public ElevationGenerator(MapGenerationConfig config, TileTypeDataMappingConfig mappingConfig)
    {
        this.config = config;
        this.mappingConfig = mappingConfig;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ElevationGenerator: Generating elevation...");

        PrecomputeElevationNoise(tiles);

        foreach (var tile in tiles.Values)
        {
            AssignElevationBiome(tile);
        }

        Debug.Log("ElevationGenerator: Elevation generation complete.");
    }

    private void AssignElevationBiome(Tile tile)
    {
        float elevation = tile.Attributes.Procedural.Elevation;

        foreach (var mapping in mappingConfig.ElevationMappings)
        {
            if (elevation >= mapping.MinElevation && elevation <= mapping.MaxElevation)
            {
                tile.SetTileTypeData(mapping.TileTypeData);
                tile.Attributes.Procedural.FixedElevationCategory = mapping.Category; // Mandatory assignment
                return; // Stop further checks once matched
            }
        }

        // Fallback if no match (shouldn't happen if mappings cover all ranges)
        tile.SetTileTypeData(mappingConfig.FallbackTileTypeData);
        tile.Attributes.Procedural.FixedElevationCategory = TileTypeDataMappingConfig.ElevationCategory.Land; // Default to "Land" if no match
    }

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

    private void PrecomputeElevationNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ElevationGenerator: Generating elevation noise...");

        switch (config.SelectedElevationMode)
        {
            case ElevationGenerationMode.PerlinNoise:
                GenerateElevationWithPerlinNoise(tiles);
                break;

            case ElevationGenerationMode.LandBudget:
                GenerateElevationWithLandBudget(tiles);
                break;

            default:
                Debug.LogError($"Unsupported elevation generation mode: {config.SelectedElevationMode}");
                break;
        }
    }

    private void GenerateElevationWithPerlinNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ElevationGenerator: Generating elevation using Perlin Noise...");

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            tile.Attributes.Procedural.Elevation = NoiseGenerationUtility.GeneratePerlinValue(
                gridPosition.x, gridPosition.y,
                config.ElevationScale,
                config.ElevationOctaves,
                config.ElevationPersistence,
                config.ElevationLacunarity
            );
        }

        Debug.Log("ElevationGenerator: Elevation generation with Perlin Noise complete.");
    }

    private void GenerateElevationWithLandBudget(Dictionary<Vector2, Tile> tiles)
    {
        float landBudget = config.LandBudget;

        int safetyCounter = 10000; // Prevent infinite loops
        while (landBudget > 0f && safetyCounter > 0)
        {
            for (int i = 0; i < config.AddElevationCycles; i++)
            {
                AddElevation(tiles, ref landBudget);
            }

            for (int i = 0; i < config.SubtractElevationCycles; i++)
            {
                SubtractElevation(tiles, ref landBudget);
            }

            safetyCounter--;
        }

        if (safetyCounter <= 0)
        {
            Debug.LogError("GenerateElevationWithLandBudget: Terminating due to potential infinite loop.");
        }

        Debug.Log("ElevationGenerator: Land budget applied.");

        if (config.ApplySmoothing)
        {
            SmoothElevation(tiles);
        }
    }

    private void AddElevation(Dictionary<Vector2, Tile> tiles, ref float landBudget)
    {
        Tile centerTile = GetRandomTile(tiles);
        List<Tile> affectedTiles = HexUtility.GetHexesInRange(
            centerTile,
            config.AddElevationRadius,
            tiles
        );

        foreach (var tile in affectedTiles)
        {
            if (tile.Attributes.Procedural.Elevation >= 1f) continue;

            if (landBudget <= 0f) break;

            float elevationChange = UnityEngine.Random.Range(
                config.AddMinElevationChange,
                config.AddMaxElevationChange
            );

            elevationChange = Mathf.Min(elevationChange, landBudget);

            tile.Attributes.Procedural.Elevation = Mathf.Clamp(
                tile.Attributes.Procedural.Elevation + elevationChange,
                0f,
                1f
            );

            landBudget -= elevationChange;
        }
    }

    private void SubtractElevation(Dictionary<Vector2, Tile> tiles, ref float landBudget)
    {
        Tile centerTile = GetRandomTile(tiles);
        List<Tile> affectedTiles = HexUtility.GetHexesInRange(
            centerTile,
            config.SubtractElevationRadius,
            tiles
        );

        foreach (var tile in affectedTiles)
        {
            if (tile.Attributes.Procedural.Elevation <= 0f) continue;

            if (landBudget <= 0f) break;

            float elevationChange = UnityEngine.Random.Range(
                config.SubtractMinElevationChange,
                config.SubtractMaxElevationChange
            );

            elevationChange = Mathf.Min(elevationChange, landBudget);

            tile.Attributes.Procedural.Elevation = Mathf.Clamp(
                tile.Attributes.Procedural.Elevation - elevationChange,
                0f,
                1f
            );

            landBudget += elevationChange;
        }
    }

    private void SmoothElevation(Dictionary<Vector2, Tile> tiles)
    {
        if (tiles == null || tiles.Count == 0) return;

        foreach (var tile in tiles.Values)
        {
            var neighbors = HexUtility.GetNeighbors(tile, tiles);
            if (neighbors.Count > 0)
            {
                float averageElevation = neighbors.Average(neighbor => neighbor.Attributes.Procedural.Elevation);
                tile.Attributes.Procedural.Elevation = Mathf.Lerp(tile.Attributes.Procedural.Elevation, averageElevation, 0.5f);
            }
        }

        Debug.Log("Smoothing complete.");
    }

    private Tile GetRandomTile(Dictionary<Vector2, Tile> tiles)
    {
        List<Tile> tileList = new List<Tile>(tiles.Values);
        int randomIndex = UnityEngine.Random.Range(0, tileList.Count);
        return tileList[randomIndex];
    }
}
