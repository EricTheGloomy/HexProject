using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RiverDebugTool : MonoBehaviour
{
    [Header("Debug Settings")]
    public KeyCode AddRiverKey = KeyCode.T; // Key to add rivers to tiles
    public KeyCode ConnectRiverKey = KeyCode.Y; // Key to connect rivers
    public HexGridDataManager GridManager; // Reference to grid manager

    public List<Vector2> riverTilePositions = new List<Vector2>(); // Stores tile positions for rivers

    void Update()
    {
        if (Input.GetKeyDown(AddRiverKey))
        {
            AddRiversToTiles();
        }
        if (Input.GetKeyDown(ConnectRiverKey))
        {
            CheckRiverConnections();
        }
    }

    public void SetRiverTiles(List<Vector2> positions)
    {
        riverTilePositions = positions;
        Debug.Log("TEST: River tile positions set.");
    }

    private void AddRiversToTiles()
    {
        if (GridManager == null || riverTilePositions.Count == 0)
        {
            Debug.LogError("TEST: GridManager is missing or no river tile positions specified.");
            return;
        }

        foreach (var position in riverTilePositions)
        {
            if (GridManager.GetHexCells().TryGetValue(position, out var tile))
            {
                tile.Attributes.Gameplay.HasRiver = true;
                Debug.Log($"TEST: River added to tile at {position}.");
            }
            else
            {
                Debug.LogWarning($"TEST: No tile found at position {position}.");
            }
        }
    }

    private void CheckRiverConnections()
    {
        if (GridManager == null)
        {
            Debug.LogError("TEST: GridManager is missing.");
            return;
        }

        foreach (var position in riverTilePositions)
        {
            if (GridManager.GetHexCells().TryGetValue(position, out var tile))
            {
                if (tile.Attributes.Gameplay.HasRiver)
                {
                    // Check neighbors
                    var neighborsWithRivers = tile.Neighbors
                        .Where(neighbor => neighbor.Attributes.Gameplay.HasRiver)
                        .ToList();

                    if (neighborsWithRivers.Count > 0)
                    {
                        foreach (var neighbor in neighborsWithRivers)
                        {
                            // Calculate the edge index
                            int edgeIndex = HexUtility.GetEdgeBetweenTiles(tile, neighbor);
                            if (edgeIndex == -1)
                            {
                                Debug.LogError($"TEST: Invalid edge calculation between {position} and {neighbor.Attributes.GridPosition}.");
                                continue;
                            }

                            // Mark the river connection
                            tile.Attributes.Gameplay.RiverConnections[edgeIndex] = true;

                            // Mark the reverse connection on the neighbor
                            int reverseEdgeIndex = (edgeIndex + 3) % 6;
                            neighbor.Attributes.Gameplay.RiverConnections[reverseEdgeIndex] = true;

                            Debug.Log($"Tile at {position} connected to {neighbor.Attributes.GridPosition} on edge {edgeIndex}, reverse edge {reverseEdgeIndex}.");
                        }
                    }
                    else
                    {
                        Debug.Log($"Tile at {position} has no river-connected neighbors.");
                    }
                }
            }
        }
    }
    
}
