// File: Scripts/Config/TileTypeDataMappingConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeDataMappingConfig", menuName = "Game/TileTypeDataMappingConfig")]
public class TileTypeDataMappingConfig : ScriptableObject
{
    [System.Serializable]
    public class TileMapping
    {
        public TileTypeData TileTypeData;
        public float MinNoiseValue; // Minimum noise value for this TileTypeData
        public float MaxNoiseValue; // Maximum noise value for this TileTypeData
    }

    public TileMapping[] TileMappings;
}
