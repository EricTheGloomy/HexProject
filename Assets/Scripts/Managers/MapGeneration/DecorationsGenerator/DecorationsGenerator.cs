using System.Collections.Generic;
using UnityEngine;

public class DecorationsGenerator : IMapGenerationStep
{
    private readonly TileTypeDataMappingConfig mappingConfig;
    private readonly MapGenerationConfig config;

    public DecorationsGenerator(MapGenerationConfig config, TileTypeDataMappingConfig mappingConfig)
    {
        this.mappingConfig = mappingConfig;
        this.config = config;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("DecorationsGenerator: Generating decorations...");

        // Get mountain elevation range from the mappings
        var mountainMapping = GetMountainMapping();
        if (mountainMapping == null)
        {
            Debug.LogWarning("DecorationsGenerator: No mountain mapping found.");
            return;
        }

        float mountainRange = mountainMapping.MaxElevation - mountainMapping.MinElevation;
        float lowMountainThreshold = mountainMapping.MinElevation + mountainRange * config.LowMountainPercentage / 100f;
        float highMountainThreshold = mountainMapping.MaxElevation;

        foreach (var tile in tiles.Values)
        {
            if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Mountain)
            {
                float elevation = tile.Attributes.Procedural.Elevation;

                if (elevation <= lowMountainThreshold)
                {
                    tile.Attributes.Gameplay.IsOccupied = true; // Block other uses
                    tile.Attributes.Gameplay.HasVegetation = false; // Remove vegetation if necessary
                    tile.Attributes.Gameplay.MountainType = MountainType.LowMountain;
                }
                else if (elevation <= highMountainThreshold)
                {
                    tile.Attributes.Gameplay.IsOccupied = true; // Block other uses
                    tile.Attributes.Gameplay.HasVegetation = false; // Remove vegetation if necessary
                    tile.Attributes.Gameplay.MountainType = MountainType.HighMountain;
                }

//                Debug.Log($"Mountain decoration assigned at {tile.Attributes.GridPosition} as {tile.Attributes.Gameplay.MountainType}");
            }
        }

        Debug.Log("DecorationsGenerator: Decoration generation complete.");
    }

    private TileTypeDataMappingConfig.ElevationMapping GetMountainMapping()
    {
        foreach (var mapping in mappingConfig.ElevationMappings)
        {
            if (mapping.Category == TileTypeDataMappingConfig.ElevationCategory.Mountain)
            {
                return mapping;
            }
        }
        return null;
    }
}

