// File: Scripts/Config/MapConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Hex Tile Settings")]
    public GameObject HexTilePrefabDefault; // Refactored name to clarify purpose.

    [Header("Map Dimensions")]
    public int MapWidth = 10;
    public int MapHeight = 10;
}
