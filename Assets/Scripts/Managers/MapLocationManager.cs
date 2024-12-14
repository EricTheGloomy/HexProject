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

        hexCells = grid;
        Debug.Log("MapLocationManager: Received grid data for location assignment.");

        var eligibleTiles = new List<Tile>();
        foreach (var tile in grid.Values)
        {
            if (tile.Attributes.TileTypeData.isEligibleForStart)
            {
                eligibleTiles.Add(tile);
            }
        }

        if (eligibleTiles.Count == 0)
        {
            Debug.LogWarning("MapLocationManager: No eligible tiles found for starting location. Assigning the first tile as fallback.");
            var firstTile = grid.Values.GetEnumerator();
            if (firstTile.MoveNext())
            {
                firstTile.Current.SetAsStartingLocation();
            }
            return;
        }

        // Randomly select a starting tile
        var startingTile = eligibleTiles[Random.Range(0, eligibleTiles.Count)];
        startingTile.SetAsStartingLocation();

        Debug.Log($"MapLocationManager: Starting location assigned at {startingTile.Attributes.GridPosition}.");
    }

    public Tile GetStartingTile()
    {
        if (hexCells == null || hexCells.Count == 0)
        {
            Debug.LogError("MapLocationManager: Hex cells are not initialized. Cannot find starting tile.");
            return null;
        }

        foreach (var tile in hexCells.Values)
        {
            if (tile.Attributes.IsStartingLocation)
            {
                return tile;
            }
        }

        Debug.LogError("MapLocationManager: No starting location found.");
        return null;
    }
}
