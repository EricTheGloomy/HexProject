using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator : IMapGenerationStep
{
    private readonly BiomeBandConfig biomeConfig;
    private readonly TileTypeDataMappingConfig mappingConfig;

    public BiomeGenerator(BiomeBandConfig biomeConfig, TileTypeDataMappingConfig mappingConfig)
    {
        this.biomeConfig = biomeConfig;
        this.mappingConfig = mappingConfig;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("BiomeGenerator: Assigning biomes...");

        foreach (var tile in tiles.Values)
        {
            AssignBiomeByBands(tile);
        }

        Debug.Log("BiomeGenerator: Biome assignment complete.");
    }

    private void AssignBiomeByBands(Tile tile)
    {
        if (tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Land)
            return; // Skip Water and Mountain tiles

        float temperature = tile.Attributes.Procedural.Temperature;
        float moisture = tile.Attributes.Procedural.Moisture;

        foreach (var band in biomeConfig.BiomeBands)
        {
            if (temperature >= band.MinTemperature && temperature <= band.MaxTemperature &&
                moisture >= band.MinMoisture && moisture <= band.MaxMoisture)
            {
                tile.SetTileTypeData(band.TileTypeData);
                return; // Stop once a match is found
            }
        }

        // Fallback for unassigned tiles
        tile.SetTileTypeData(mappingConfig.FallbackTileTypeData);
    }
}

