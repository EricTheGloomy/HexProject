using System.Collections.Generic;
using UnityEngine;

public class NeighborDebugVisualizer : MonoBehaviour
{
    public GameObject debugCubePrefab; // Assign a small cube prefab in the Unity Editor
    private Tile selectedTile;

    private void Update()
    {
        HandleSelection();
        HandleDebugVisualization();
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click for selecting a tile
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    selectedTile = tile;
                    Debug.Log($"Selected Tile at {tile.Attributes.GridPosition}");
                }
            }
        }
    }

    private void HandleDebugVisualization()
    {
        if (Input.GetKeyDown(KeyCode.U) && selectedTile != null) // Press 'U' to visualize neighbors
        {
            Debug.Log($"Visualizing neighbors for tile at {selectedTile.Attributes.GridPosition}");
            VisualizeNeighbors(selectedTile);
        }

        if (Input.GetKeyDown(KeyCode.E)) // Press 'E' to validate all edge tiles
        {
            ValidateEdgeTiles();
        }
    }

    private void VisualizeNeighbors(Tile tile)
    {
        List<Tile> neighbors = HexUtility.GetNeighbors(tile, GetHexCellsFromManager());
        if (neighbors == null || neighbors.Count == 0)
        {
            Debug.LogWarning($"No neighbors found for the selected tile at {tile.Attributes.GridPosition}!");
            return;
        }

        foreach (var neighbor in neighbors)
        {
            if (neighbor == null) continue;

            // Debug: Validate bi-directional neighbor relationship
            if (!neighbor.Neighbors.Contains(tile))
            {
                Debug.LogWarning($"Bi-directional mismatch: Tile {tile.Attributes.GridPosition} lists {neighbor.Attributes.GridPosition} as a neighbor, but the reverse is not true.");
            }

            // Debug: Log directions
            Vector2Int direction = Vector2Int.RoundToInt(neighbor.Attributes.GridPosition - tile.Attributes.GridPosition);
            Debug.Log($"Neighbor {neighbor.Attributes.GridPosition} is in direction {direction} relative to {tile.Attributes.GridPosition}");

            // Visualize neighbors
            Vector3 spawnPosition = neighbor.transform.position + new Vector3(0, 1.5f, 0);
            Instantiate(debugCubePrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void ValidateEdgeTiles()
    {
        Dictionary<Vector2, Tile> tiles = GetHexCellsFromManager();
        if (tiles == null) return;

        List<Tile> edgeTiles = FindEdgeTiles(tiles);

        foreach (var edgeTile in edgeTiles)
        {
            Debug.Log($"Validating edge tile at {edgeTile.Attributes.GridPosition} with {edgeTile.Neighbors.Count} neighbors.");
            ValidateTileNeighbors(edgeTile);
        }
    }

    private void ValidateTileNeighbors(Tile tile)
    {
        foreach (var neighbor in tile.Neighbors)
        {
            if (neighbor == null) continue;

            // Check bi-directional relationship
            if (!neighbor.Neighbors.Contains(tile))
            {
                Debug.LogWarning($"Bi-directional mismatch at edge: Tile {tile.Attributes.GridPosition} and {neighbor.Attributes.GridPosition}");
            }

            // Log direction
            Vector2Int direction = Vector2Int.RoundToInt(neighbor.Attributes.GridPosition - tile.Attributes.GridPosition);
            Debug.Log($"Direction to neighbor {neighbor.Attributes.GridPosition} from {tile.Attributes.GridPosition}: {direction}");
        }
    }

    private List<Tile> FindEdgeTiles(Dictionary<Vector2, Tile> tiles)
    {
        List<Tile> edgeTiles = new List<Tile>();

        foreach (var tile in tiles.Values)
        {
            if (tile.Neighbors.Count < 6) // Edge or corner tile
            {
                edgeTiles.Add(tile);
            }
        }

        Debug.Log($"Found {edgeTiles.Count} edge tiles.");
        return edgeTiles;
    }

    private Dictionary<Vector2, Tile> GetHexCellsFromManager()
    {
        // Assuming you have a reference to HexGridDataManager
        HexGridDataManager gridManager = FindObjectOfType<HexGridDataManager>();
        if (gridManager != null && gridManager.isGridReady)
        {
            return gridManager.GetHexCells();
        }

        Debug.LogError("HexGridDataManager is not initialized or grid is not ready!");
        return null;
    }
}
