using System;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderingHandler : IGameStateHandler
{
    private readonly HexMapRenderer mapRenderer;
    private readonly Action<GameState> transitionCallback;

    public MapRenderingHandler(HexMapRenderer mapRenderer, Action<GameState> transitionCallback)
    {
        this.mapRenderer = mapRenderer;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Rendering map...");
        mapRenderer.RenderMap(cachedHexCells);
        transitionCallback(GameState.FogOfWarInitialization);
    }
}