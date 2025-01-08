using System;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarInitializationHandler : IGameStateHandler
{
    private readonly FogOfWarManager fogOfWarManager;
    private readonly Action<GameState> transitionCallback;

    public FogOfWarInitializationHandler(FogOfWarManager fogOfWarManager, Action<GameState> transitionCallback)
    {
        this.fogOfWarManager = fogOfWarManager;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Initializing fog of war...");

        if (cachedHexCells == null || cachedHexCells.Count == 0)
        {
            Debug.LogError("FogOfWarInitializationHandler: Cached hex cells are null or empty!");
            return;
        }

        // Log tiles for verification
        Debug.Log($"FogOfWarInitializationHandler: Initializing fog for {cachedHexCells.Count} tiles.");

        fogOfWarManager.Initialize(cachedHexCells);

        transitionCallback(GameState.CameraInitialization);
    }
}

