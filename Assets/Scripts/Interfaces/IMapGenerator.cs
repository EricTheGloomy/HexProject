using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMapGenerator
{
    event Action<Dictionary<Vector2Int, TileTypeData>> OnMapGenerated;

    void ApplyTileTypeData(Dictionary<Vector2, Tile> tiles); // New method to update tiles
    Dictionary<Vector2Int, TileTypeData> GeneratedMapData { get; }
}

