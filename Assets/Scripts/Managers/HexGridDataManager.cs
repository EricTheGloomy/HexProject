using System;
using System.Collections.Generic;
using UnityEngine;

public class HexGridDataManager : MonoBehaviour
{
    public static event Action<Dictionary<Vector2Int, Vector3>, Dictionary<Vector2Int, TileType>, float, float> OnGridReady;

    public MapConfig MapConfiguration;

    private Dictionary<Vector2Int, Vector3> tilePositions = new Dictionary<Vector2Int, Vector3>();
    private Dictionary<Vector2Int, TileType> tileTypes;
    private Tile[,] mapGrid;
    private Dictionary<Vector2, Tile> hexCells = new Dictionary<Vector2, Tile>();


    private float hexWidth;
    private float hexHeight;

    private void OnEnable()
    {
        ProceduralMapGenerator.OnMapGenerated += InitializeGrid;
    }

    private void OnDisable()
    {
        ProceduralMapGenerator.OnMapGenerated -= InitializeGrid;
    }

    private void InitializeGrid(Dictionary<Vector2Int, TileType> mapData)
    {
        if (MapConfiguration == null || MapConfiguration.HexTilePrefabDefault == null)
        {
            Debug.LogError("MapConfiguration or HexTilePrefabDefault is missing!");
            return;
        }

        // Store tile types for later use
        tileTypes = mapData;

        CalculateHexSize(MapConfiguration.HexTilePrefabDefault);

        mapGrid = new Tile[MapConfiguration.MapWidth, MapConfiguration.MapHeight];
        tilePositions.Clear();

        foreach (var entry in mapData)
        {
            Vector2Int gridPosition = entry.Key;
            Vector3 worldPosition = CalculateHexPosition(gridPosition.y, gridPosition.x);
            tilePositions[gridPosition] = worldPosition;
        }

        Debug.Log("HexGridDataManager: Grid initialized.");
        OnGridReady?.Invoke(tilePositions, tileTypes, hexWidth, hexHeight);
    }

    private void CalculateHexSize(GameObject hexTilePrefab)
    {
        MeshRenderer renderer = hexTilePrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("HexTilePrefab is missing a MeshRenderer!");
            return;
        }

        hexWidth = renderer.bounds.size.x;
        hexHeight = renderer.bounds.size.z;

        if (hexWidth == 0 || hexHeight == 0)
        {
            Debug.LogError("Invalid hex size! Check HexTilePrefabDefault.");
        }

        Debug.Log($"Hex size calculated: width={hexWidth}, height={hexHeight}");
    }

    private Vector3 CalculateHexPosition(int row, int col)
    {
        if (hexWidth == 0 || hexHeight == 0)
        {
            Debug.LogError("Hex dimensions are zero! Check CalculateHexSize.");
            return Vector3.zero;
        }

        float xOffset = (row % 2 == 0) ? 0 : hexWidth * 0.5f;
        float x = col * hexWidth + xOffset;
        float z = row * (hexHeight * 0.75f);
        Vector3 position = new Vector3(x, 0, z);

        Debug.Log($"Calculated position for row {row}, col {col}: {position}");
        return position;
    }

    public void AddTile(Tile tile, Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            mapGrid[position.x, position.y] = tile;
            Debug.Log($"Tile added at ({position.x}, {position.y})");
        }
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        return IsValidPosition(position) ? mapGrid[position.x, position.y] : null;
    }

    public Dictionary<Vector2, Tile> GetHexCells() => hexCells;

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < mapGrid.GetLength(0) &&
               position.y >= 0 && position.y < mapGrid.GetLength(1);
    }
}
