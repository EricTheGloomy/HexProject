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
            if (distance >= config.MoistureMaxRange) continue;

            foreach (Tile neighbor in HexUtility.GetNeighbors(currentTile, tiles))
            {
                if (!visited.Contains(neighbor))
                {
                    float decayMoisture = currentTile.Attributes.Procedural.Moisture * config.MoistureDecayRate;
                    float jitter = UnityEngine.Random.Range(-config.MoistureJitter, config.MoistureJitter);
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

        Debug.Log("MoistureGenerator: Moisture propagation complete.");
    }
}
