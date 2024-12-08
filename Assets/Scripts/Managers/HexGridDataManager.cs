using System.Collections.Generic;
using UnityEngine;

public class HexGridDataManager : MonoBehaviour
{
    private Tile[,] mapGrid;
    private Dictionary<Vector2, Tile> hexCells = new Dictionary<Vector2, Tile>();

    private void OnEnable()
    {
        HexMapRenderer.OnRenderingComplete += AssignNeighbors;
    }

    private void OnDisable()
    {
        HexMapRenderer.OnRenderingComplete -= AssignNeighbors;
    }

    public void InitializeGrid(int width, int height)
    {
        mapGrid = new Tile[width, height];
        Debug.Log($"HexGridDataManager: Initialized grid with size {width}x{height}.");
    }

    public void AddTile(Tile tile, Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            mapGrid[position.x, position.y] = tile;
            hexCells[tile.OffsetCoordinates] = tile;

            Debug.Log($"Tile added at ({position.x}, {position.y}) with OffsetCoordinates {tile.OffsetCoordinates}");
        }
    }

    public void AssignNeighbors()
    {
        Debug.Log("Assigning neighbors...");
        foreach (var cell in hexCells)
        {
            Tile tile = cell.Value;
            List<Tile> neighbors = HexUtility.GetNeighbors(tile, hexCells);

            foreach (Tile neighbor in neighbors)
            {
                tile.AddNeighbor(neighbor);
            }
        }
        Debug.Log("Neighbors assigned!");
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        return IsValidPosition(position) ? mapGrid[position.x, position.y] : null;
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < mapGrid.GetLength(0) &&
               position.y >= 0 && position.y < mapGrid.GetLength(1);
    }
}
