using System.Collections.Generic;
using UnityEngine;

public interface IGameStateHandler
{
    void EnterState(Dictionary<Vector2, Tile> cachedHexCells);
}