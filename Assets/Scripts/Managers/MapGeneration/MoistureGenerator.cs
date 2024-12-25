using System.Collections.Generic;
using UnityEngine;

public class MoistureGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;
    private readonly TileTypeDataMappingConfig mappingConfig;

    public MoistureGenerator(MapGenerationConfig config, TileTypeDataMappingConfig mappingConfig)
    {
        this.config = config;
        this.mappingConfig = mappingConfig;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("MoistureGenerator: Generating moisture...");
        PrecomputeMoisture(tiles);
        Debug.Log("MoistureGenerator: Moisture generation complete.");
    }

    private void PrecomputeMoisture(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("MoistureGenerator: Precomputing Moisture...");

        switch (config.SelectedMoistureMode)
        {
            case MoistureGenerationMode.PerlinNoise:
                foreach (var tile in tiles.Values)
                {
                    Vector2Int gridPosition = new Vector2Int((int)tile.Attributes.GridPosition.x, (int)tile.Attributes.GridPosition.y);

                    tile.Attributes.Procedural.Moisture = NoiseGenerationUtility.GeneratePerlinValue(
                        gridPosition.x, gridPosition.y,
                        config.MoistureScale,
                        config.MoistureOctaves,
                        config.MoisturePersistence,
                        config.MoistureLacunarity
                    );
                }
                Debug.Log("MoistureGenerator: Moisture generated using Perlin Noise.");
                break;

            case MoistureGenerationMode.WaterPropagation:
                SpreadMoistureFromWater(tiles);
                Debug.Log("MoistureGenerator: Moisture generated using Water Propagation.");
                break;
        }
    }

    private void SpreadMoistureFromWater(Dictionary<Vector2, Tile> tiles)
    {
        Queue<(Tile tile, int distance)> waterFrontier = new Queue<(Tile, int)>();
        Queue<(Tile tile, int distance)> riverFrontier = new Queue<(Tile, int)>();
        HashSet<Tile> waterVisited = new HashSet<Tile>();
        HashSet<Tile> riverVisited = new HashSet<Tile>();

        // Initialize moisture values for water and river tiles
        foreach (var tile in tiles.Values)
        {
            if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water)
            {
                tile.Attributes.Procedural.Moisture = 1.0f;
                waterFrontier.Enqueue((tile, 0));
                waterVisited.Add(tile);
            }
            else if (tile.Attributes.Gameplay.HasRiver)
            {
                tile.Attributes.Procedural.Moisture = Mathf.Max(tile.Attributes.Procedural.Moisture, 0.5f); // Combine moisture if already set
                riverFrontier.Enqueue((tile, 0));
                riverVisited.Add(tile);
            }
        }

        // Propagate moisture from water tiles
        PropagateMoisture(waterFrontier, tiles, waterVisited, config.MoistureDecayRate, config.MoistureJitter, config.MoistureMaxRange);

        // Propagate moisture from river tiles
        PropagateMoisture(riverFrontier, tiles, riverVisited, config.RiverMoistureDecayRate, config.RiverMoistureJitter, config.RiverMoistureMaxRange);

        Debug.Log("MoistureGenerator: Moisture propagation complete.");
    }

    private void PropagateMoisture(
        Queue<(Tile tile, int distance)> frontier,
        Dictionary<Vector2, Tile> tiles,
        HashSet<Tile> visited,
        float decayRate,
        float jitter,
        int maxRange
    )
    {
        int safetyCounter = 100000; // Prevent infinite BFS
        while (frontier.Count > 0 && safetyCounter > 0)
        {
            var (currentTile, distance) = frontier.Dequeue();
            if (distance >= maxRange) continue;

            foreach (Tile neighbor in HexUtility.GetNeighbors(currentTile, tiles))
            {
                if (!visited.Contains(neighbor))
                {
                    float decayMoisture = currentTile.Attributes.Procedural.Moisture * decayRate;
                    float randomJitter = UnityEngine.Random.Range(-jitter, jitter);
                    float moistureContribution = Mathf.Clamp01(decayMoisture + randomJitter);

                    neighbor.Attributes.Procedural.Moisture = Mathf.Clamp01(neighbor.Attributes.Procedural.Moisture + moistureContribution);
                    frontier.Enqueue((neighbor, distance + 1));
                    visited.Add(neighbor);
                }
            }

            safetyCounter--;
        }

        if (safetyCounter <= 0)
        {
            Debug.LogError("PropagateMoisture: Terminating due to potential infinite loop.");
        }
    }

}
