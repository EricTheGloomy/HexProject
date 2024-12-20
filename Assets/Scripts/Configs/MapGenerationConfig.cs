using UnityEngine;


public enum ElevationGenerationMode { PerlinNoise, LandBudget }
public enum MoistureGenerationMode { PerlinNoise, WaterPropagation }
public enum TemperatureGenerationMode { PerlinNoise, PolarToEquator, EquatorCentered }


[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Game/MapGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Elevation Settings")]
    public ElevationGenerationMode SelectedElevationMode = ElevationGenerationMode.PerlinNoise; // Default to PerlinNoise
    [Header("Land Budget Settings")]
    public float LandBudget = 500; // Total land budget
    public int AddElevationCycles = 5; // Number of add elevation cycles
    public int SubtractElevationCycles = 3; // Number of subtract elevation cycles
    [Header("Add Elevation Settings")]
    public float AddMinElevationChange = 0.1f; // Minimum elevation to add
    public float AddMaxElevationChange = 0.3f; // Maximum elevation to add
    public int AddElevationRadius = 3; // Radius for add elevation
    public float AddAffectedTilePercentage = 0.5f; // Percentage of affected tiles for add

    [Header("Subtract Elevation Settings")]
    public float SubtractMinElevationChange = 0.1f; // Minimum elevation to subtract
    public float SubtractMaxElevationChange = 0.3f; // Maximum elevation to subtract
    public int SubtractElevationRadius = 3; // Radius for subtract elevation
    public float SubtractAffectedTilePercentage = 0.5f; // Percentage of affected tiles for subtract
    [Header("Smoothing Settings")]
    public bool ApplySmoothing = false; // Toggle smoothing after elevation generation

    [Header("Water Propagation Settings")]
    public MoistureGenerationMode SelectedMoistureMode = MoistureGenerationMode.PerlinNoise; // Add this field
    public float MoistureDecayRate = 0.2f;    // % moisture lost per tile distance
    public float MoistureJitter = 0.05f;      // Random moisture variation per tile

    [Header("Moisture Jitter")]
    public int MoistureMaxRange = 5;          // Range for water-based moisture spreading

    [Header("Temperature Settings")]
    public TemperatureGenerationMode SelectedTemperatureMode = TemperatureGenerationMode.PerlinNoise;
    public float PolarTemperatureMin = 0f; // Minimum temperature at the poles
    public float PolarTemperatureMax = 1.0f; // Maximum temperature at the equator

    [Header("Temperature Jitter")]
    public float TemperatureJitter = 0.05f; // Random variation for temperature

    [Header("Temperature Elevation Adjustment")]
    public float ElevationTemperatureDropRate = 0.1f; // Temperature decrease per unit elevation
    public float ElevationTemperatureThreshold = 0.5f; // Elevation at which temperature starts dropping

    [Header("Settlement Settings")]
    public int NumberOfCities = 3;
    public int NumberOfTowns = 5;
    public int NumberOfVillages = 10;
    public int NumberOfHamlets = 20;

    public int CityMinPopulation = 500;
    public int CityMaxPopulation = 1000;
    public int CityRadius = 4;

    public int TownMinPopulation = 200;
    public int TownMaxPopulation = 499;
    public int TownRadius = 3;

    public int VillageMinPopulation = 50;
    public int VillageMaxPopulation = 199;
    public int VillageRadius = 2;

    public int HamletMinPopulation = 10;
    public int HamletMaxPopulation = 49;
    public int HamletRadius = 1;

    public int PlacementRetries = 10; // Limit retries to avoid infinite loops
    [Header("Population for Tiles Without Housing")]
    public int MinPopulationForUninhabited = 1;
    public int MaxPopulationForUninhabited = 10;

    [Header("Terrain Suitability Settings")]
    public float MinElevationForHousing = 0.35f;
    public float MaxElevationForHousing = 0.55f;

    public float MinMoistureForHousing = 0.3f;
    public float MaxMoistureForHousing = 0.6f;

    public float MinTemperatureForHousing = 0.3f;
    public float MaxTemperatureForHousing = 0.6f;

    [Header("Vegetation Settings")]
    public int MaxVegetationTiles = 100; // Maximum tiles to populate with vegetation
    public float MinVegetationElevation = 0.31f; // Minimum elevation for vegetation
    public float MaxVegetationElevation = 0.6f; // Maximum elevation for vegetation
    public float MinVegetationMoisture = 0.4f; // Minimum moisture for vegetation
    public float MaxVegetationMoisture = 1.0f; // Maximum moisture for vegetation
    public float VegetationChance = 0.75f; // Probability a suitable tile gets vegetation

    [Range(0f, 1f)]
    public float ExtremeSettlementChance = 0.1f; // 10% chance

    [Header("Seed Settings")]
    [SerializeField] private int seed = 42;
    [SerializeField] private bool isSeedRandomized = false; // If true, use random seed
    public int Seed
    {
        get
        {
            if (isSeedRandomized)
            {
                if (cachedSeed == null)
                {
                    cachedSeed = Random.Range(RandomSeedMin, RandomSeedMax + 1);
                    Debug.Log($"Random Seed generated: {cachedSeed}");
                }
                return cachedSeed.Value;
            }
            return seed;
        }
    }

    [Header("Elevation Noise Settings")]
    public float ElevationScale = 1f;
    public int ElevationOctaves = 4;
    public float ElevationPersistence = 0.5f;
    public float ElevationLacunarity = 2f;

    [Header("Moisture Noise Settings")]
    public float MoistureScale = 1.5f;
    public int MoistureOctaves = 3;
    public float MoisturePersistence = 0.6f;
    public float MoistureLacunarity = 1.8f;

    [Header("Temperature Noise Settings")]
    public float TemperatureScale = 2f;
    public int TemperatureOctaves = 2;
    public float TemperaturePersistence = 0.4f;
    public float TemperatureLacunarity = 2.2f;

    private int? cachedSeed = null;
    public int RandomSeedMin = 0;
    public int RandomSeedMax = 10000;

    [Header("General Perlin Noise Settings")]
    [Range(0.1f, 10f)] public float NoiseScale = 1f;
    [Range(1, 8)] public int Octaves = 4;
    [Range(0f, 1f)] public float Persistence = 0.5f;
    [Range(1f, 5f)] public float Lacunarity = 2f;

    [Header("Initial Perlin Settings")]
    public float InitialAmplitude = 1f;
    public float InitialFrequency = 1f;

    [Header("Perlin Noise Adjustment")]
    public float NoiseAdjustmentFactor = 2f;

    [Header("Perlin Noise Offsets")]
    public int OffsetRangeMin = -100000;
    public int OffsetRangeMax = 100000;

    [Header("Noise Normalization")]
    public float NoiseMin = -1f;
    public float NoiseMax = 1f;

    [Header("Noise Output Scaling")]
    public float NoiseHeightMultiplier = 1f;

    [Header("Height Clamping")]
    public float MinHeightClamp = -1f;
    public float MaxHeightClamp = 1f;
}
