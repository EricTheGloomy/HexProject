using System;
using System.Collections.Generic;
using UnityEngine;

public class HexGridDataManager : MonoBehaviour, IGridManager
{
    public event Action<Dictionary<Vector2, Tile>> OnGridReady;

    public MapConfig MapConfiguration;

    public GameObject TilePrefab;

    private Dictionary<Vector2Int, Tile> allTiles = new Dictionary<Vector2Int, Tile>();
    private Dictionary<Vector2, Tile> hexCells = new Dictionary<Vector2, Tile>(); // For HexUtility
    public Dictionary<Vector2, Tile> GetHexCells() => hexCells;
    private Tile[,] mapGrid;
    public bool isGridReady = false;

    private float hexPrefabWidth;
    private float hexPrefabHeight;

    public void InitializeGrid()
    {
        isGridReady = false;
        Debug.Log("HexGridDataManager: Initializing grid...");

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

        CalculateHexSize(MapConfiguration.HexTilePrefabDefault);

        mapGrid = new Tile[MapConfiguration.MapWidth, MapConfiguration.MapHeight];
        allTiles.Clear();
        hexCells.Clear();

        bool useFlatTop = MapConfiguration.useFlatTop;

        // Instantiate and initialize each tile with default data
        for (int row = 0; row < MapConfiguration.MapHeight; row++)
        {
            for (int col = 0; col < MapConfiguration.MapWidth; col++)
            {
                Vector2Int gridPosition = new Vector2Int(col, row);

                GameObject tileObject = Instantiate(TilePrefab, transform);
                tileObject.name = $"Tile_{gridPosition.x}_{gridPosition.y}";

                Tile tile = tileObject.GetComponent<Tile>();
                if (tile == null)
                {
                    Debug.LogError($"HexGridDataManager: TilePrefab is missing the Tile script at {gridPosition}!");
                    continue;
                }

                var defaultTileTypeData = ScriptableObject.CreateInstance<TileTypeData>();
                defaultTileTypeData.Name = "Default"; // Optional: set default fields
                
                tile.Initialize(gridPosition, useFlatTop, hexPrefabWidth, hexPrefabHeight, defaultTileTypeData);

                allTiles[gridPosition] = tile;
                mapGrid[gridPosition.x, gridPosition.y] = tile;
                hexCells[tile.OffsetCoordinates] = tile;
            }
        }

        AssignNeighbors();

        isGridReady = true;
        Debug.Log("HexGridDataManager: Grid initialized and neighbors assigned.");
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

        hexPrefabWidth = renderer.bounds.size.x;
        hexPrefabHeight = renderer.bounds.size.z;

        if (hexPrefabWidth == 0 || hexPrefabHeight == 0)
        {
            Debug.LogError("HexGridDataManager: Invalid hex size! Check HexTilePrefabDefault.");
        }

        Debug.Log($"HexGridDataManager: Hex size calculated: width={hexPrefabWidth}, height={hexPrefabHeight}");
    }

    private void AssignNeighbors()
    {
        Debug.Log("HexGridDataManager: Assigning neighbors...");

        foreach (var entry in allTiles)
        {
            Tile tile = entry.Value;

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

    public Vector2Int GetMapDimensions()
    {
        return new Vector2Int(MapConfiguration.MapWidth, MapConfiguration.MapHeight);
    }

    public float GetTileWidth()
    {
        return hexPrefabWidth;
    }

    public float GetTileHeight()
    {
        return hexPrefabHeight;
    }

}
