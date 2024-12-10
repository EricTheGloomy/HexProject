using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMapGenerator
{
    event Action<Dictionary<Vector2Int, TileData>> OnMapGenerated;
    void GenerateMap();

    Dictionary<Vector2Int, TileData> GeneratedMapData { get; }
}
