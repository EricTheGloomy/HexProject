using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerationHandler : IGameStateHandler
{
    private readonly ProceduralMapGenerator mapGenerator;
    private readonly Action<GameState> transitionCallback;

    public MapGenerationHandler(ProceduralMapGenerator mapGenerator, Action<GameState> transitionCallback)
    {
        this.mapGenerator = mapGenerator;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Generating map...");
        mapGenerator.ApplyTileTypeData(cachedHexCells);
        transitionCallback(GameState.LocationsAssigning);
    }
}