using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TemperatureGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;
    private readonly TileTypeDataMappingConfig mappingConfig;

    public TemperatureGenerator(MapGenerationConfig config, TileTypeDataMappingConfig mappingConfig)
    {
        this.config = config;
        this.mappingConfig = mappingConfig;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("TemperatureGenerator: Generating temperature...");
        PrecomputeTemperature(tiles);
        Debug.Log("TemperatureGenerator: Temperature generation complete.");
    }

    private void PrecomputeTemperature(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("TemperatureGenerator: Precomputing Temperature...");

        switch (config.SelectedTemperatureMode)
        {
            case TemperatureGenerationMode.PerlinNoise:
                GenerateTemperatureWithPerlinNoise(tiles);
                break;

            case TemperatureGenerationMode.PolarToEquator:
                GenerateTemperaturePolarToEquator(tiles);
                AdjustTemperatureForElevationAndJitter(tiles);
                break;

            case TemperatureGenerationMode.EquatorCentered:
                GenerateTemperatureEquatorCentered(tiles);
                AdjustTemperatureForElevationAndJitter(tiles);
                break;
        }

        Debug.Log("TemperatureGenerator: Temperature computation complete.");
    }

    private void GenerateTemperatureWithPerlinNoise(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tile in tiles.Values)
        {
            Vector2Int gridPosition = new Vector2Int((int)tile.Attributes.GridPosition.x, (int)tile.Attributes.GridPosition.y);
            tile.Attributes.Procedural.Temperature = NoiseGenerationUtility.GeneratePerlinValue(
                gridPosition.x, gridPosition.y,
                config.TemperatureScale,
                config.TemperatureOctaves,
                config.TemperaturePersistence,
                config.TemperatureLacunarity
            );
        }
        Debug.Log("TemperatureGenerator: Temperature generated using Perlin Noise.");
    }

    private void GenerateTemperaturePolarToEquator(Dictionary<Vector2, Tile> tiles)
    {
        float mapHeight = tiles.Keys.Max(tile => tile.y);

        foreach (var tile in tiles.Values)
        {
            float normalizedRow = tile.Attributes.GridPosition.y / mapHeight; // Normalize [0, 1]
            tile.Attributes.Procedural.Temperature = Mathf.Lerp(
                config.PolarTemperatureMin,
                config.PolarTemperatureMax,
                normalizedRow
            );
        }
        Debug.Log("TemperatureGenerator: Temperature generated using Polar to Equator method.");
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
                config.PolarTemperatureMax,
                config.PolarTemperatureMin,
                normalizedDistance
            );
        }
        Debug.Log("TemperatureGenerator: Temperature generated using Equator-Centered method.");
    }

    private void AdjustTemperatureForElevationAndJitter(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tile in tiles.Values)
        {
            float elevation = tile.Attributes.Procedural.Elevation;
            float baseTemperature = tile.Attributes.Procedural.Temperature;

            // Apply elevation-based adjustment (temperature drops with higher elevation)
            if (elevation > config.ElevationTemperatureThreshold)
            {
                float elevationAdjustment = (elevation - config.ElevationTemperatureThreshold)
                                            * config.ElevationTemperatureDropRate;

                baseTemperature -= elevationAdjustment;
            }

            // Apply jitter for slight variation
            float jitter = UnityEngine.Random.Range(-config.TemperatureJitter, config.TemperatureJitter);
            tile.Attributes.Procedural.Temperature = Mathf.Clamp01(baseTemperature + jitter);
        }
        Debug.Log("TemperatureGenerator: Applied elevation-based temperature adjustment and jitter.");
    }
}
