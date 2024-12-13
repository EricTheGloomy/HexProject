// File: Scripts/Config/MapConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    [Header("Hex Tile Settings")]
    public GameObject HexTilePrefabDefault;

    [Header("Map Dimensions")]
    public int MapWidth = 10;
    public int MapHeight = 10;

    [Header("Grid Settings")]
    public bool useFlatTop = false; // False = pointy-top hexes
}
