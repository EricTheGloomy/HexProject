using System;
using System.Collections.Generic;
using UnityEngine;

public class HexGridDataManager : MonoBehaviour, IGridManager
{
    public event Action<Dictionary<Vector2, Tile>> OnGridReady;

    public MapConfig MapConfiguration;

    public GameObject TilePrefab; // Assign the prefab in the Unity Inspector

    private Dictionary<Vector2Int, Tile> allTiles = new Dictionary<Vector2Int, Tile>(); // Renamed for clarity
    private Dictionary<Vector2, Tile> hexCells = new Dictionary<Vector2, Tile>(); // For HexUtility
    public Dictionary<Vector2, Tile> GetHexCells() => hexCells;
    private Tile[,] mapGrid;
    public bool isGridReady = false;

    private float hexWidth;
    private float hexHeight;

    public void InitializeGrid(Dictionary<Vector2Int, TileTypeData> mapData)
    {
        // Reset state
        isGridReady = false;
        Debug.Log("HexGridDataManager: Initializing grid...");

        // Validate dependencies
        if (MapConfiguration == null || MapConfiguration.HexTilePrefabDefault == null)
        {
            Debug.LogError("HexGridDataManager: MapConfiguration or HexTilePrefabDefault is missing!");
            return;
        }

        if (TilePrefab == null)
        {
            Debug.LogError("HexGridDataManager: TilePrefab is not assigned!");
            return;
        }

        // Calculate hex dimensions from the default prefab
        CalculateHexSize(MapConfiguration.HexTilePrefabDefault);

        // Initialize grid containers
        mapGrid = new Tile[MapConfiguration.MapWidth, MapConfiguration.MapHeight];
        allTiles.Clear();
        hexCells.Clear();

        // Instantiate and initialize each tile
        foreach (var entry in mapData)
        {
            Vector2Int gridPosition = entry.Key;
            TileTypeData tileData = entry.Value;

            // Instantiate the tile prefab
            GameObject tileObject = Instantiate(TilePrefab, transform);
            tileObject.name = $"Tile_{gridPosition.x}_{gridPosition.y}";

            // Get the Tile script from the prefab
            Tile tile = tileObject.GetComponent<Tile>();
            if (tile == null)
            {
                Debug.LogError($"HexGridDataManager: TilePrefab is missing the Tile script at {gridPosition}!");
                continue;
            }

            // Initialize the tile
            tile.Initialize(gridPosition, hexWidth, hexHeight, tileData);

            // Store the tile in dictionaries
            allTiles[gridPosition] = tile;
            mapGrid[gridPosition.x, gridPosition.y] = tile;

            // Calculate and store offset coordinates for HexUtility
            hexCells[tile.OffsetCoordinates] = tile;
        }

        // Assign neighbors to each tile using HexUtility
        AssignNeighbors();

        // Mark the grid as ready
        isGridReady = true;
        Debug.Log("HexGridDataManager: Grid initialized and neighbors assigned.");
        Debug.Log($"HexGridDataManager: Invoking OnGridReady with {hexCells.Count} tiles.");

        // Notify subscribers that the grid is ready
        OnGridReady?.Invoke(hexCells);
    }

    private void CalculateHexSize(GameObject hexTilePrefab)
    {
        MeshRenderer renderer = hexTilePrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("HexGridDataManager: HexTilePrefab is missing a MeshRenderer!");
            return;
        }

        hexWidth = renderer.bounds.size.x;
        hexHeight = renderer.bounds.size.z;

        if (hexWidth == 0 || hexHeight == 0)
        {
            Debug.LogError("HexGridDataManager: Invalid hex size! Check HexTilePrefabDefault.");
        }

        Debug.Log($"HexGridDataManager: Hex size calculated: width={hexWidth}, height={hexHeight}");
    }

    private void AssignNeighbors()
    {
        Debug.Log("HexGridDataManager: Assigning neighbors...");

        foreach (var entry in allTiles)
        {
            Tile tile = entry.Value;

            // Use HexUtility to find neighbors
            List<Tile> neighbors = HexUtility.GetNeighbors(tile, hexCells);

            foreach (Tile neighbor in neighbors)
            {
                tile.AddNeighbor(neighbor); // Ensure bi-directional relationship
            }
        }

        Debug.Log("HexGridDataManager: Neighbors assigned.");
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        return allTiles.TryGetValue(position, out Tile tile) ? tile : null;
    }
}
