using System;
using System.Collections.Generic;
using UnityEngine;

public class GridInitializationHandler : IGameStateHandler
{
    private readonly HexGridDataManager gridManager;
    private readonly Action<GameState> transitionCallback;

    public GridInitializationHandler(HexGridDataManager gridManager, Action<GameState> transitionCallback)
    {
        this.gridManager = gridManager;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Initializing grid...");
        gridManager.InitializeGrid();
        transitionCallback(GameState.MapGeneration);
    }
}