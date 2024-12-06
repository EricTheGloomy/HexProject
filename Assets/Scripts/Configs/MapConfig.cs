// File: Scripts/Config/MapConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "MapConfig", menuName = "Game/MapConfig")]
public class MapConfig : ScriptableObject
{
    public GameObject HexTilePrefab;
    public int MapWidth;
    public int MapHeight;
}
