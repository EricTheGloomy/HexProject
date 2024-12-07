// File: Scripts/Config/TileTypeMappingConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeMappingConfig", menuName = "Game/TileTypeMappingConfig")]
public class TileTypeMappingConfig : ScriptableObject
{
    [System.Serializable]
    public class TileMapping
    {
        public TileType TileType;
        public float MinNoiseValue; // Minimum noise value for this TileType
        public float MaxNoiseValue; // Maximum noise value for this TileType
    }

    public TileMapping[] TileMappings;
}
