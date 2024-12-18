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

    public BiomeBandConfig BiomeBandConfig;

    public void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Starting biome assignment...");

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("ProceduralMapGenerator: Received empty tile data.");
            return;
        }

        // Step 1: Precompute Elevation
        PrecomputeElevationNoise(tiles);

        // Step 2: Assign Elevation Categories
        foreach (var tile in tiles.Values)
        {
            AssignElevationBiome(tile);
        }

        // Step 3: Precompute Moisture
        PrecomputeMoisture(tiles);

        // Step 4: Precompute Temperature
        PrecomputeTemperature(tiles);

        // Step 5: Assign Biomes Using Temperature and Moisture Bands
        foreach (var tile in tiles.Values)
        {
            AssignBiomeByBands(tile);
        }

        Debug.Log("ProceduralMapGenerator: Biome assignment complete.");
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

    private void AssignBiomeByBands(Tile tile)
    {
        if (tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Land)
            return; // Skip Water and Mountain tiles

        float temperature = tile.Attributes.Procedural.Temperature;
        float moisture = tile.Attributes.Procedural.Moisture;

        foreach (var band in BiomeBandConfig.BiomeBands)
        {
            if (temperature >= band.MinTemperature && temperature <= band.MaxTemperature &&
                moisture >= band.MinMoisture && moisture <= band.MaxMoisture)
            {
                tile.SetTileTypeData(band.TileTypeData);
                return; // Stop once a match is found
            }
        }

        // Fallback for unassigned tiles
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

    private void PrecomputeElevationNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Generating elevation...");

        switch (MapGenerationConfig.SelectedElevationMode)
        {
            case ElevationGenerationMode.PerlinNoise:
                GenerateElevationWithPerlinNoise(tiles);
                break;

            case ElevationGenerationMode.LandBudget:
                GenerateElevationWithLandBudget(tiles);
                break;

            default:
                Debug.LogError($"Unsupported elevation generation mode: {MapGenerationConfig.SelectedElevationMode}");
                break;
        }
    }

    private void GenerateElevationWithPerlinNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Generating elevation using Perlin Noise...");

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            tile.Attributes.Procedural.Elevation = GeneratePerlinValue(
                gridPosition.x, gridPosition.y,
                MapGenerationConfig.ElevationScale,
                MapGenerationConfig.ElevationOctaves,
                MapGenerationConfig.ElevationPersistence,
                MapGenerationConfig.ElevationLacunarity
            );
        }

        Debug.Log("ProceduralMapGenerator: Elevation generation with Perlin Noise complete.");
    }

    private void GenerateElevationWithLandBudget(Dictionary<Vector2, Tile> tiles)
    {
        float landBudget = MapGenerationConfig.LandBudget; // Changed to float

        int safetyCounter = 10000; // Prevent infinite loops
        while (landBudget > 0f && safetyCounter > 0)
        {
            for (int i = 0; i < MapGenerationConfig.AddElevationCycles; i++)
            {
                AddElevation(tiles, ref landBudget);
            }

            for (int i = 0; i < MapGenerationConfig.SubtractElevationCycles; i++)
            {
                SubtractElevation(tiles, ref landBudget);
            }

            safetyCounter--;
        }

        if (safetyCounter <= 0)
        {
            Debug.LogError("GenerateElevationWithLandBudget: Terminating due to potential infinite loop.");
        }

        Debug.Log("Elevation map generation complete.");

        if (MapGenerationConfig.ApplySmoothing)
        {
            SmoothElevation(tiles);
        }
    }

    private void AddElevation(Dictionary<Vector2, Tile> tiles, ref float landBudget)
    {
        Tile centerTile = GetRandomTile(tiles);
        List<Tile> affectedTiles = GetTilesInRadius(
            centerTile,
            MapGenerationConfig.AddElevationRadius,
            tiles
        );

        foreach (var tile in affectedTiles)
        {
            // Skip tiles that are already at maximum elevation
            if (tile.Attributes.Procedural.Elevation >= 1f) continue;

            if (landBudget <= 0f) break;

            float elevationChange = UnityEngine.Random.Range(
                MapGenerationConfig.AddMinElevationChange,
                MapGenerationConfig.AddMaxElevationChange
            );

            // Ensure we don't exceed the remaining budget
            elevationChange = Mathf.Min(elevationChange, landBudget);

            // Clamp final elevation to max 1
            tile.Attributes.Procedural.Elevation = Mathf.Clamp(
                tile.Attributes.Procedural.Elevation + elevationChange,
                0f,
                1f
            );

            landBudget -= elevationChange; // Decrement budget directly with float
        }
    }

    private void SubtractElevation(Dictionary<Vector2, Tile> tiles, ref float landBudget)
    {
        Tile centerTile = GetRandomTile(tiles);
        List<Tile> affectedTiles = GetTilesInRadius(
            centerTile,
            MapGenerationConfig.SubtractElevationRadius,
            tiles
        );

        foreach (var tile in affectedTiles)
        {
            // Skip tiles that are already at minimum elevation
            if (tile.Attributes.Procedural.Elevation <= 0f) continue;

            if (landBudget <= 0f) break;

            float elevationChange = UnityEngine.Random.Range(
                MapGenerationConfig.SubtractMinElevationChange,
                MapGenerationConfig.SubtractMaxElevationChange
            );

            // Ensure we don't exceed the remaining budget
            elevationChange = Mathf.Min(elevationChange, landBudget);

            // Clamp final elevation to min 0
            tile.Attributes.Procedural.Elevation = Mathf.Clamp(
                tile.Attributes.Procedural.Elevation - elevationChange,
                0f,
                1f
            );

            landBudget += elevationChange; // Increment budget directly with float
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

    private List<Tile> GetTilesInRadius(Tile centerTile, int radius, Dictionary<Vector2, Tile> tiles)
    {
        return HexUtility.GetHexesInRange(centerTile, radius, tiles);
    }

    private Tile GetRandomTile(Dictionary<Vector2, Tile> tiles)
    {
        List<Tile> tileList = new List<Tile>(tiles.Values);
        int randomIndex = UnityEngine.Random.Range(0, tileList.Count);
        return tileList[randomIndex];
    }

    private void PrecomputeMoisture(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Precomputing Moisture...");

        switch (MapGenerationConfig.SelectedMoistureMode)
        {
            case MoistureGenerationMode.PerlinNoise:
                foreach (var tile in tiles.Values)
                {
                    Vector2Int gridPosition = new Vector2Int((int)tile.Attributes.GridPosition.x, (int)tile.Attributes.GridPosition.y);

                    tile.Attributes.Procedural.Moisture = GeneratePerlinValue(
                        gridPosition.x, gridPosition.y, 
                        MapGenerationConfig.MoistureScale, 
                        MapGenerationConfig.MoistureOctaves, 
                        MapGenerationConfig.MoisturePersistence, 
                        MapGenerationConfig.MoistureLacunarity
                    );
                }
                Debug.Log("ProceduralMapGenerator: Moisture generated using Perlin Noise.");
                break;

            case MoistureGenerationMode.WaterPropagation:
                SpreadMoistureFromWater(tiles);
                Debug.Log("ProceduralMapGenerator: Moisture generated using Water Propagation.");
                break;
        }
    }

    private void SpreadMoistureFromWater(Dictionary<Vector2, Tile> tiles)
    {
        Queue<(Tile tile, int distance)> frontier = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        foreach (var tile in tiles.Values)
        {
            if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water)
            {
                tile.Attributes.Procedural.Moisture = 1.0f;
                frontier.Enqueue((tile, 0));
                visited.Add(tile);
            }
            else
            {
                tile.Attributes.Procedural.Moisture = 0.0f;
            }
        }

        int safetyCounter = 10000; // Prevent infinite BFS
        while (frontier.Count > 0 && safetyCounter > 0)
        {
            var (currentTile, distance) = frontier.Dequeue();
            if (distance >= MapGenerationConfig.MoistureMaxRange) continue;

            foreach (Tile neighbor in HexUtility.GetNeighbors(currentTile, tiles))
            {
                if (!visited.Contains(neighbor))
                {
                    float decayMoisture = currentTile.Attributes.Procedural.Moisture * MapGenerationConfig.MoistureDecayRate;
                    float jitter = UnityEngine.Random.Range(-MapGenerationConfig.MoistureJitter, MapGenerationConfig.MoistureJitter);
                    float moistureContribution = Mathf.Clamp01(decayMoisture + jitter);

                    neighbor.Attributes.Procedural.Moisture = Mathf.Clamp01(neighbor.Attributes.Procedural.Moisture + moistureContribution);
                    frontier.Enqueue((neighbor, distance + 1));
                    visited.Add(neighbor);
                }
            }

            safetyCounter--;
        }

        if (safetyCounter <= 0)
        {
            Debug.LogError("SpreadMoistureFromWater: Terminating due to potential infinite loop.");
        }

        Debug.Log("ProceduralMapGenerator: Moisture propagation complete.");
    }

    private int HexDistance(Tile origin, Tile target)
    {
        Vector3 originCube = origin.Attributes.CubeCoordinates;
        Vector3 targetCube = target.Attributes.CubeCoordinates;

        return (int)((Mathf.Abs(originCube.x - targetCube.x) 
                    + Mathf.Abs(originCube.y - targetCube.y) 
                    + Mathf.Abs(originCube.z - targetCube.z)) / 2);
    }

    private void PrecomputeTemperature(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Precomputing Temperature...");

        switch (MapGenerationConfig.SelectedTemperatureMode)
        {
            case TemperatureGenerationMode.PerlinNoise:
                GenerateTemperatureWithPerlinNoise(tiles);
                break;

            case TemperatureGenerationMode.PolarToEquator:
                GenerateTemperaturePolarToEquator(tiles);
                // Apply elevation-based adjustment and jitter
                AdjustTemperatureForElevationAndJitter(tiles);
                break;

            case TemperatureGenerationMode.EquatorCentered:
                GenerateTemperatureEquatorCentered(tiles);
                // Apply elevation-based adjustment and jitter
                AdjustTemperatureForElevationAndJitter(tiles);
                break;
        }
        Debug.Log("ProceduralMapGenerator: Temperature computation complete.");
    }

    private void GenerateTemperatureWithPerlinNoise(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tile in tiles.Values)
        {
            Vector2Int gridPosition = new Vector2Int((int)tile.Attributes.GridPosition.x, (int)tile.Attributes.GridPosition.y);
            tile.Attributes.Procedural.Temperature = GeneratePerlinValue(
                gridPosition.x, gridPosition.y,
                MapGenerationConfig.TemperatureScale,
                MapGenerationConfig.TemperatureOctaves,
                MapGenerationConfig.TemperaturePersistence,
                MapGenerationConfig.TemperatureLacunarity
            );
        }
        Debug.Log("Temperature generated using Perlin Noise.");
    }

    private void GenerateTemperaturePolarToEquator(Dictionary<Vector2, Tile> tiles)
    {
        float mapHeight = tiles.Keys.Max(tile => tile.y);

        foreach (var tile in tiles.Values)
        {
            float normalizedRow = tile.Attributes.GridPosition.y / mapHeight; // Normalize [0, 1]
            tile.Attributes.Procedural.Temperature = Mathf.Lerp(
                MapGenerationConfig.PolarTemperatureMin,
                MapGenerationConfig.PolarTemperatureMax,
                normalizedRow
            );
        }
        Debug.Log("Temperature generated using Polar to Equator method.");
    }

    private void GenerateTemperatureEquatorCentered(Dictionary<Vector2, Tile> tiles)
    {
        float mapHeight = tiles.Keys.Max(tile => tile.y);
        float equator = mapHeight / 2f;

        foreach (var tile in tiles.Values)
        {
            float distanceFromEquator = Mathf.Abs(tile.Attributes.GridPosition.y - equator);
            float normalizedDistance = distanceFromEquator / equator; // Normalize [0, 1]

            tile.Attributes.Procedural.Temperature = Mathf.Lerp(
                MapGenerationConfig.PolarTemperatureMax,
                MapGenerationConfig.PolarTemperatureMin,
                normalizedDistance
            );
        }
        Debug.Log("Temperature generated using Equator-Centered method.");
    }

    private void AdjustTemperatureForElevationAndJitter(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tile in tiles.Values)
        {
            float elevation = tile.Attributes.Procedural.Elevation;
            float baseTemperature = tile.Attributes.Procedural.Temperature;

            // Apply elevation-based adjustment (temperature drops with higher elevation)
            if (elevation > MapGenerationConfig.ElevationTemperatureThreshold)
            {
                float elevationAdjustment = (elevation - MapGenerationConfig.ElevationTemperatureThreshold)
                                            * MapGenerationConfig.ElevationTemperatureDropRate;

                baseTemperature -= elevationAdjustment;
            }

            // Apply jitter for slight variation
            float jitter = UnityEngine.Random.Range(-MapGenerationConfig.TemperatureJitter, MapGenerationConfig.TemperatureJitter);
            tile.Attributes.Procedural.Temperature = Mathf.Clamp01(baseTemperature + jitter);
        }
        Debug.Log("ProceduralMapGenerator: Applied elevation-based temperature adjustment and jitter.");
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

    private float GeneratePerlinValue(int col, int row, float? scale = null, int? octaves = null, float? persistence = null, float? lacunarity = null)
    {
        // Use specific parameters if provided, otherwise fallback to general defaults
        float finalScale = scale ?? MapGenerationConfig.NoiseScale;
        int finalOctaves = octaves ?? MapGenerationConfig.Octaves;
        float finalPersistence = persistence ?? MapGenerationConfig.Persistence;
        float finalLacunarity = lacunarity ?? MapGenerationConfig.Lacunarity;

        float amplitude = MapGenerationConfig.InitialAmplitude;
        float frequency = MapGenerationConfig.InitialFrequency;
        float noiseHeight = 0f;

        for (int i = 0; i < finalOctaves; i++)
        {
            float sampleX = (col / finalScale) * frequency;
            float sampleY = (row / finalScale) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
            noiseHeight += perlinValue * amplitude;

            amplitude *= finalPersistence;
            frequency *= finalLacunarity;
        }

        noiseHeight *= MapGenerationConfig.NoiseHeightMultiplier;
        noiseHeight = Mathf.Clamp(noiseHeight, MapGenerationConfig.MinHeightClamp, MapGenerationConfig.MaxHeightClamp);
        return Mathf.InverseLerp(MapGenerationConfig.NoiseMin, MapGenerationConfig.NoiseMax, noiseHeight);
    }

}
