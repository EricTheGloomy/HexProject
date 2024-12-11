using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMapGenerator
{
    event Action<Dictionary<Vector2Int, TileTypeData>> OnMapGenerated;
    void GenerateMap();

    Dictionary<Vector2Int, TileTypeData> GeneratedMapData { get; }
}
