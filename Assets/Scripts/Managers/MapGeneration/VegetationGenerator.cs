using System.Collections.Generic;
using UnityEngine;

public class VegetationGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;

    public VegetationGenerator(MapGenerationConfig config)
    {
        this.config = config;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("VegetationGenerator: Generating vegetation...");

        // Collect eligible tiles
        List<Tile> eligibleTiles = new List<Tile>();
        foreach (var tile in tiles.Values)
        {
            if (IsTileSuitableForVegetation(tile))
            {
                eligibleTiles.Add(tile);
            }
        }

        // Shuffle eligible tiles
        ShuffleList(eligibleTiles);

        // Add vegetation to the shuffled tiles
        int vegetationCount = 0;
        foreach (var tile in eligibleTiles)
        {
            if (vegetationCount >= config.MaxVegetationTiles)
                break;

            if (Random.value <= config.VegetationChance)
            {
                tile.Attributes.Gameplay.HasVegetation = true;
                tile.Attributes.Gameplay.IsOccupied = true; // Prevent other uses
                vegetationCount++;
                Debug.Log($"Vegetation added at {tile.Attributes.GridPosition}");
            }
        }

        Debug.Log($"VegetationGenerator: Added vegetation to {vegetationCount} tiles.");
    }

    private bool IsTileSuitableForVegetation(Tile tile)
    {
        // Skip occupied, water, and mountain tiles
        if (tile.Attributes.Gameplay.IsOccupied) return false;
        if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water ||
            tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Mountain)
        {
            return false;
        }
        
        // Prevent vegetation on tiles with rivers
        if (tile.Attributes.Gameplay.HasRiver)
        {
            return false;
        }

        float elevation = tile.Attributes.Procedural.Elevation;
        float moisture = tile.Attributes.Procedural.Moisture;

        return elevation >= config.MinVegetationElevation && elevation <= config.MaxVegetationElevation &&
            moisture >= config.MinVegetationMoisture && moisture <= config.MaxVegetationMoisture;
    }

    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random(config.Seed); // Use a consistent seed for reproducibility
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

}
