// File: Scripts/Config/TileTypeData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeData", menuName = "Game/TileTypeData")]
public class TileTypeData : ScriptableObject
{
    public string Name;           // Name of the tile type
    [TextArea] public string Description; // Description of the tile
    public GameObject TileModel;  // Model associated with this tile type
    public GameObject FogOverlay; // Fog of war model for this tile type
    public bool isEligibleForStart; //can the tile type be chosen as a starting location
}
