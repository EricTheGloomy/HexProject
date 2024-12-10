using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridManager
{
    event Action<Dictionary<Vector2, Tile>> OnGridReady;
    void InitializeGrid(Dictionary<Vector2Int, TileData> mapData);

    Dictionary<Vector2, Tile> GetHexCells();
}
