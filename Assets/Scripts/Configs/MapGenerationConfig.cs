// File: Scripts/Config/MapGenerationConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Game/MapGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Perlin Noise Settings")]
    [Range(0.1f, 10f)] public float NoiseScale = 1f;
    [Range(1, 8)] public int Octaves = 4;
    [Range(0f, 1f)] public float Persistence = 0.5f;
    [Range(1f, 5f)] public float Lacunarity = 2f;
    [SerializeField] private int seed = 42;
    private int? cachedSeed = null;
    public int Seed
    {
        get
        {
            // If the seed is -1, generate a random one, but cache it so it's consistent for this session
            if (seed == -1)
            {
                if (cachedSeed == null)
                {
                    cachedSeed = Random.Range(0, 10001);
                    Debug.Log($"Random Seed generated: {cachedSeed}");
                }
                return cachedSeed.Value;
            }

            return seed;
        }
    }

    [Header("Perlin Noise Offsets")]
    public int OffsetRangeMin = -100000;
    public int OffsetRangeMax = 100000;

    [Header("Noise Normalization")]
    public float NoiseMin = -1f;
    public float NoiseMax = 1f;
}
