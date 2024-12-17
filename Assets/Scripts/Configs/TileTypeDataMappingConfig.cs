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

    public ElevationMapping[] ElevationMappings; 

    public TileTypeData FallbackTileTypeData; // Default biome if no mapping matches
}
