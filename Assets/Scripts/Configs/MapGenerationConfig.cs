using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Game/MapGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Perlin Noise Settings")]
    [Range(0.1f, 10f)] public float NoiseScale = 1f;
    [Range(1, 8)] public int Octaves = 4;
    [Range(0f, 1f)] public float Persistence = 0.5f;
    [Range(1f, 5f)] public float Lacunarity = 2f;

    [Header("Initial Perlin Settings")]
    public float InitialAmplitude = 1f;
    public float InitialFrequency = 1f;

    [Header("Perlin Noise Adjustment")]
    public float NoiseAdjustmentFactor = 2f;

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
    private int? cachedSeed = null;
    public int RandomSeedMin = 0;
    public int RandomSeedMax = 10000;

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
