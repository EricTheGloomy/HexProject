using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationsAssigningHandler : IGameStateHandler
{
    private readonly MapLocationManager locationManager;
    private readonly Action<GameState> transitionCallback;

    public LocationsAssigningHandler(MapLocationManager locationManager, Action<GameState> transitionCallback)
    {
        this.locationManager = locationManager;
        this.transitionCallback = transitionCallback;
    }

    public void EnterState(Dictionary<Vector2, Tile> cachedHexCells)
    {
        Debug.Log("Assigning locations...");
        locationManager.AssignLocations(cachedHexCells);
        transitionCallback(GameState.MapRendering);
    }
}