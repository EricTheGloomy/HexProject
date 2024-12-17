using UnityEngine;

[CreateAssetMenu(fileName = "BiomeBandConfig", menuName = "Game/BiomeBandConfig")]
public class BiomeBandConfig : ScriptableObject
{
    [System.Serializable]
    public class BiomeBand
    {
        public string Name;                             // Friendly name for debugging
        public float MinTemperature, MaxTemperature;   // Temperature range
        public float MinMoisture, MaxMoisture;         // Moisture range
        public TileTypeData TileTypeData;              // The biome to assign
    }

    [Header("Biome Bands")]
    public BiomeBand[] BiomeBands; // Array of all temperature-moisture bands
}
