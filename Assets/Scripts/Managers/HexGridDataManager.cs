using System;
using System.Collections.Generic;
using System.Linq;
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

        // Instantiate and initialize each tile
        for (int row = 0; row < MapConfiguration.MapHeight; row++)
        {
            for (int col = 0; col < MapConfiguration.MapWidth; col++)
            {
                Vector2Int gridPosition = new Vector2Int(col, row);

                // Replace manual instantiation with CreateTile
                var defaultTileTypeData = ScriptableObject.CreateInstance<TileTypeData>();
                defaultTileTypeData.Name = "Default"; // Optional: set default fields

                CreateTile(gridPosition, defaultTileTypeData);
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

            Debug.Log($"Assigning neighbors for tile at {tile.Attributes.GridPosition}");
            List<Tile> neighbors = HexUtility.GetNeighbors(tile, hexCells);

            // Clear and reassign neighbors in the correct order
            tile.Neighbors.Clear();

            foreach (Tile neighbor in neighbors)
            {
                tile.AddNeighbor(neighbor);
            }

            // Log final neighbor order for debugging
            Debug.Log($"Tile {tile.Attributes.GridPosition} neighbors: {string.Join(", ", tile.Neighbors.Select(n => n.Attributes.GridPosition))}");
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
    private void CreateTile(Vector2Int gridPosition, TileTypeData tileTypeData)
    {
        // Create attributes for the tile
        var attributes = new TileAttributes
        {
            GridPosition = gridPosition,
            TileTypeData = tileTypeData
        };

        // Calculate CubeCoordinates and OffsetCoordinates
        attributes.CubeCoordinates = HexCoordinateHelper.AxialToCube(HexCoordinateHelper.OffsetToAxial(new Vector2(gridPosition.x, gridPosition.y)));
        attributes.OffsetCoordinates = HexCoordinateHelper.AxialToOffset(HexCoordinateHelper.CubeToAxial(attributes.CubeCoordinates));

        // Instantiate the tile object
        GameObject tileObject = Instantiate(TilePrefab, transform);
        tileObject.name = $"Tile_{gridPosition.x}_{gridPosition.y}";

        Tile tile = tileObject.GetComponent<Tile>();
        if (tile != null)
        {
            // Initialize tile with attributes
            tile.Initialize(attributes, useFlatTop: MapConfiguration.useFlatTop, hexPrefabWidth, hexPrefabHeight);

            // Store tile in dictionaries
            allTiles[gridPosition] = tile;
            mapGrid[gridPosition.x, gridPosition.y] = tile;
            hexCells[attributes.OffsetCoordinates] = tile; // Ensure OffsetCoordinates is populated
        }
        else
        {
            Debug.LogError($"HexGridDataManager: TilePrefab is missing the Tile script at {gridPosition}!");
        }
    }

}
