using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeDataMappingConfig", menuName = "Game/TileTypeDataMappingConfig")]
public class TileTypeDataMappingConfig : ScriptableObject
{
    public enum ElevationCategory
    {
        Water,
        Land,
        Mountain
    }


    [System.Serializable]
    public class ElevationMapping
    {
        public ElevationCategory Category;  // Water, Land, or Mountain
        public TileTypeData TileTypeData;   // Biome to assign
        public float MinElevation;          // Minimum elevation range
        public float MaxElevation;          // Maximum elevation range
    }

    [System.Serializable]
    public class MoistureMapping
    {
        public TileTypeData TileTypeData;
        public float MinMoisture;
        public float MaxMoisture;
    }

    [System.Serializable]
    public class TemperatureMapping
    {
        public TileTypeData TileTypeData;
        public float MinTemperature;
        public float MaxTemperature;
    }

    public ElevationMapping[] ElevationMappings; // For Pass 1
    public MoistureMapping[] MoistureMappings;   // For Pass 2
    public TemperatureMapping[] TemperatureMappings; // For Pass 3

    public TileTypeData FallbackTileTypeData; // Default biome if no mapping matches
}
