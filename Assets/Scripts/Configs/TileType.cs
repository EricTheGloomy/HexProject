// File: Scripts/Config/TileType.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TileType", menuName = "Game/TileType")]
public class TileType : ScriptableObject
{
    public string Name;           // Name of the tile type
    [TextArea] public string Description; // Description of the tile
    public GameObject Prefab;     // Prefab associated with this tile type
}