using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraInitializationHandler : IGameStateHandler
{
    private readonly CameraManager cameraManager;
    private readonly HexGridDataManager gridManager;
    private readonly Action<GameState> transitionCallback;

    public CameraInitializationHandler(CameraManager cameraManager, HexGridDataManager gridManager, Action<GameState> transitionCallback)
    {
        this.cameraManager = cameraManager;
        this.gridManager = gridManager;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Initializing camera...");
        float tileSizeX = gridManager.GetTileWidth();
        float tileSizeZ = gridManager.GetTileHeight();

        cameraManager.Initialize(cachedHexCells, tileSizeX, tileSizeZ);
        transitionCallback(GameState.Gameplay);
    }
}
