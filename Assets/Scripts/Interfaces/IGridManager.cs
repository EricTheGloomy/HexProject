using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGridManager
{
    event Action<Dictionary<Vector2, Tile>> OnGridReady;

    void InitializeGrid(); // Changed to no longer depend on mapData
    Dictionary<Vector2, Tile> GetHexCells();
    float GetTileWidth();
    float GetTileHeight();
}
