// File: Scripts/Config/TileTypeDataMappingConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeDataMappingConfig", menuName = "Game/TileTypeDataMappingConfig")]
public class TileTypeDataMappingConfig : ScriptableObject
{
    [System.Serializable]
    public class TileMapping
    {
        public TileTypeData TileTypeData;
        public float MinNoiseValue;
        public float MaxNoiseValue;
    }

    public TileMapping[] TileMappings;

    [Header("Fallback Settings")]
    public TileTypeData FallbackTileTypeData; // Default TileTypeData if no mapping matches
}
