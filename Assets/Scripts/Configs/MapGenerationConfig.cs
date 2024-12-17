using UnityEngine;


public enum MoistureGenerationMode { PerlinNoise, WaterPropagation }
public enum TemperatureGenerationMode { PerlinNoise, PolarToEquator, EquatorCentered }


[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Game/MapGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
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
