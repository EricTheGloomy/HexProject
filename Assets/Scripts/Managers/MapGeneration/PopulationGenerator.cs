using System.Collections.Generic;
using UnityEngine;

public class PopulationGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;
    private readonly TileTypeDataMappingConfig mappingConfig;

    public PopulationGenerator(MapGenerationConfig config, TileTypeDataMappingConfig mappingConfig)
    {
        this.config = config;
        this.mappingConfig = mappingConfig;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log("PopulationGenerator: Generating population...");

        // Step 1: Place settlements
        PlaceSettlement(tiles, SettlementType.City, config.NumberOfCities, config.CityRadius, config.CityMinPopulation, config.CityMaxPopulation);
        PlaceSettlement(tiles, SettlementType.Town, config.NumberOfTowns, config.TownRadius, config.TownMinPopulation, config.TownMaxPopulation);
        PlaceSettlement(tiles, SettlementType.Village, config.NumberOfVillages, config.VillageRadius, config.VillageMinPopulation, config.VillageMaxPopulation);
        PlaceSettlement(tiles, SettlementType.Hamlet, config.NumberOfHamlets, config.HamletRadius, config.HamletMinPopulation, config.HamletMaxPopulation);

        // Step 2: Populate remaining tiles
        PopulateRemainingTiles(tiles);

        Debug.Log("PopulationGenerator: Population generation complete.");
    }

    private void PlaceSettlement(Dictionary<Vector2, Tile> tiles, SettlementType type, int count, int radius, int minPop, int maxPop)
    {
        int retries = config.PlacementRetries;
        List<Tile> placedSettlements = new List<Tile>();

        for (int i = 0; i < count; i++)
        {
            bool placed = false;

            while (retries > 0)
            {
                Tile candidate = GetRandomEligibleTile(tiles, placedSettlements, radius);

                if (candidate != null)
                {
                    int population = Random.Range(minPop, maxPop + 1);
                    candidate.Attributes.Gameplay.Population = population;
                    candidate.Attributes.Gameplay.HasHousing = true;
                    candidate.Attributes.Gameplay.IsOccupied = true;
                    candidate.Attributes.Gameplay.SettlementType = type; // Assign SettlementType
                    placedSettlements.Add(candidate);

//                    Debug.Log($"{type} placed at {candidate.Attributes.GridPosition} with population {population}");
                    placed = true;
                    break;
                }

                retries--;
            }

            if (!placed)
            {
                Debug.LogWarning($"Could not place {type}. Retries exhausted.");
            }
        }
    }

    private Tile GetRandomEligibleTile(Dictionary<Vector2, Tile> tiles, List<Tile> placedSettlements, int radius)
    {
        List<Tile> eligibleTiles = new List<Tile>();

        foreach (var tile in tiles.Values)
        {
            if (IsTileEligible(tile, placedSettlements, radius))
            {
                eligibleTiles.Add(tile);
            }
        }

        if (eligibleTiles.Count > 0)
        {
            return eligibleTiles[Random.Range(0, eligibleTiles.Count)];
        }

        return null;
    }

    private bool IsTileEligible(Tile tile, List<Tile> placedSettlements, int radius)
    {
        // Skip water and mountain tiles
        if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water ||
            tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Mountain)
        {
            return false;
        }

        // Check suitability thresholds
        float elevation = tile.Attributes.Procedural.Elevation;
        float moisture = tile.Attributes.Procedural.Moisture;
        float temperature = tile.Attributes.Procedural.Temperature;

        bool isSuitable = elevation >= config.MinElevationForHousing && elevation <= config.MaxElevationForHousing &&
                          moisture >= config.MinMoistureForHousing && moisture <= config.MaxMoistureForHousing &&
                          temperature >= config.MinTemperatureForHousing && temperature <= config.MaxTemperatureForHousing;

        bool isExtremeSettlement = !isSuitable && Random.value < config.ExtremeSettlementChance;

        if (!isSuitable && !isExtremeSettlement)
        {
            return false;
        }
        if (isExtremeSettlement)
        {
            tile.Attributes.Gameplay.IsExtremeSettlement = true;
        }

        // Check radius
        foreach (var settlement in placedSettlements)
        {
            if (HexUtility.GetHexDistance(tile, settlement) <= radius)
            {
                return false;
            }
        }

        return true;
    }

    private void PopulateRemainingTiles(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tile in tiles.Values)
        {
            if (!tile.Attributes.Gameplay.HasHousing &&
                tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Water &&
                tile.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Mountain)
            {
                tile.Attributes.Gameplay.Population = Random.Range(config.MinPopulationForUninhabited, config.MaxPopulationForUninhabited + 1);
            }
        }
    }
}

