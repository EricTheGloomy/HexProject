// File: Scripts/Config/TileType.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Game/TileData")]
public class TileData : ScriptableObject
{
    public string Name;           // Name of the tile type
    [TextArea] public string Description; // Description of the tile
    public GameObject TileModel;  // Model associated with this tile type
    public GameObject FogOverlay; // Fog of war model for this tile type
}
