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
        PrecomputeNoise(tiles);

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

    private void PrecomputeNoise(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Precomputing Perlin noise for Elevation...");

        Vector2[] octaveOffsets = GetPerlinOffsets(new System.Random(MapGenerationConfig.Seed));

        foreach (var tileEntry in tiles)
        {
            Vector2Int gridPosition = new Vector2Int((int)tileEntry.Key.x, (int)tileEntry.Key.y);
            Tile tile = tileEntry.Value;

            // Generate Elevation
            tile.Attributes.Procedural.Elevation = GeneratePerlinValue(
                gridPosition.x, gridPosition.y,
                MapGenerationConfig.ElevationScale,
                MapGenerationConfig.ElevationOctaves,
                MapGenerationConfig.ElevationPersistence,
                MapGenerationConfig.ElevationLacunarity
            );
        }

        Debug.Log("ProceduralMapGenerator: Elevation noise generation complete.");
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
        Debug.Log("ProceduralMapGenerator: Starting moisture propagation...");

        // BFS frontier for spreading moisture
        Queue<(Tile tile, int distance)> frontier = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // Step 1: Initialize water tiles
        foreach (var tile in tiles.Values)
        {
            if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water)
            {
                tile.Attributes.Procedural.Moisture = 1.0f; // Water tiles start with full moisture
                frontier.Enqueue((tile, 0)); // Start BFS from water tiles
                visited.Add(tile);
            }
            else
            {
                tile.Attributes.Procedural.Moisture = 0.0f; // Initialize non-water tiles to 0
            }
        }

        // Step 2: Propagate moisture
        while (frontier.Count > 0)
        {
            var (currentTile, distance) = frontier.Dequeue();

            // Stop if range limit is reached
            if (distance >= MapGenerationConfig.MoistureMaxRange)
                continue;

            foreach (Tile neighbor in HexUtility.GetNeighbors(currentTile, tiles))
            {
                // Calculate moisture to transfer
                float decayMoisture = currentTile.Attributes.Procedural.Moisture * MapGenerationConfig.MoistureDecayRate;
                float jitter = UnityEngine.Random.Range(-MapGenerationConfig.MoistureJitter, MapGenerationConfig.MoistureJitter);
                float moistureContribution = Mathf.Clamp01(decayMoisture + jitter);

                // Add moisture contribution to the neighbor
                float newMoisture = neighbor.Attributes.Procedural.Moisture + moistureContribution;

                // Clamp final moisture value to [0, 1]
                neighbor.Attributes.Procedural.Moisture = Mathf.Clamp01(newMoisture);

                // Enqueue neighbor for further spreading if not already visited
                if (!visited.Contains(neighbor))
                {
                    frontier.Enqueue((neighbor, distance + 1));
                    visited.Add(neighbor);
                }
            }
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
