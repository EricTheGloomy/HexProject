using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProceduralMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Dependencies")]
    public MapGenerationConfig MapGenerationConfig;
    public TileTypeDataMappingConfig TileTypeDataMappingConfig;
    public BiomeBandConfig BiomeBandConfig;

    public event Action<Dictionary<Vector2Int, TileTypeData>> OnMapGenerated;

    private Dictionary<Vector2Int, TileTypeData> generatedMapData = new(); // Initialize the generated map data
    public Dictionary<Vector2Int, TileTypeData> GeneratedMapData => generatedMapData;

    private List<IMapGenerationStep> mapGenerators;

    private void GenerateMap(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Starting map generation...");
        InitializeGenerators();

        foreach (var generator in mapGenerators)
        {
            Debug.Log($"Running {generator.GetType().Name}...");
            generator.Generate(tiles); // Each generator modifies `tiles` in place.
            Debug.Log($"Generator {generator.GetType().Name} finished processing.");

            // Debug logs to ensure each tile has updated data after processing
            foreach (var tile in tiles)
            {
//                Debug.Log($"Tile at {tile.Key}: Elevation = {tile.Value.Attributes.Procedural.Elevation}, TileTypeData = {tile.Value.Attributes.TileTypeData}");
            }
        }

        Debug.Log("ProceduralMapGenerator: Map generation complete.");
        OnMapGenerated?.Invoke(PrepareFinalMapData(tiles));
    }

    public void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("ProceduralMapGenerator: Applying tile type data...");
        GenerateMap(tiles); // Reuse GenerateMap for actual processing logic
    }

    private void InitializeGenerators()
    {
        mapGenerators = new List<IMapGenerationStep>
        {
            new ElevationGenerator(MapGenerationConfig, TileTypeDataMappingConfig),
            new MoistureGenerator(MapGenerationConfig, TileTypeDataMappingConfig),
            new TemperatureGenerator(MapGenerationConfig, TileTypeDataMappingConfig),
            new BiomeGenerator(BiomeBandConfig, TileTypeDataMappingConfig),
            new PopulationGenerator(MapGenerationConfig, TileTypeDataMappingConfig),
            new VegetationGenerator(MapGenerationConfig)
            // Add more generators here as needed
        };
    }

    private Dictionary<Vector2Int, TileTypeData> PrepareFinalMapData(Dictionary<Vector2, Tile> tiles)
    {
        var mapData = tiles.ToDictionary(
            tile => new Vector2Int((int)tile.Key.x, (int)tile.Key.y),
            tile => tile.Value.Attributes.TileTypeData
        );
        Debug.Log($"PrepareFinalMapData: Generated map data with {mapData.Count} entries.");
        return mapData;
    }

}
