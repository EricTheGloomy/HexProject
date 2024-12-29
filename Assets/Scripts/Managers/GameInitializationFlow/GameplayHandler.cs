using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayHandler : IGameStateHandler
{
    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Gameplay has started!");
    }
}

