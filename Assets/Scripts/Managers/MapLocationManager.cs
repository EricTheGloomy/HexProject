// File: Scripts/Managers/MapLocationManager.cs
using System.Collections.Generic;
using UnityEngine;

public class MapLocationManager : MonoBehaviour, IMapLocationManager
{
    private Dictionary<Vector2, Tile> hexCells;

    public void AssignLocations(Dictionary<Vector2, Tile> grid)
    {
        if (grid == null || grid.Count == 0)
        {
            Debug.LogError("MapLocationManager: Grid is empty. Cannot assign locations.");
            return;
        }

        hexCells = grid; // Store grid for further use
        Debug.Log("MapLocationManager: Received grid data for location assignment.");

        // Select eligible tiles
        var eligibleTiles = new List<Tile>();
        foreach (var tile in hexCells.Values)
        {
            if (tile.TileTypeData.isEligibleForStart)
            {
                eligibleTiles.Add(tile);
            }
        }

        if (eligibleTiles.Count == 0)
        {
            Debug.LogError("MapLocationManager: No eligible tiles found for starting location.");
            return;
        }

        // Randomly select a starting tile
        var startingTile = eligibleTiles[Random.Range(0, eligibleTiles.Count)];
        startingTile.SetAsStartingLocation(); // Mark the tile as the starting location at runtime

        Debug.Log($"MapLocationManager: Starting location assigned at {startingTile.GridPosition}.");
    }

    public Tile GetStartingTile()
    {
        if (hexCells == null || hexCells.Count == 0)
        {
            Debug.LogError("MapLocationManager: No grid data available. Cannot determine starting tile.");
            return null;
        }

        foreach (var tile in hexCells.Values)
        {
            if (tile.IsStartingLocation) // Check runtime-specific property
            {
                return tile;
            }
        }

        Debug.LogError("MapLocationManager: No starting location found.");
        return null;
    }
}
