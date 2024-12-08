using UnityEngine;
using System.Collections.Generic;

public class HexGridDataManager : MonoBehaviour
{
    private Tile[,] mapGrid;
    private Dictionary<Vector2, Tile> hexCells = new Dictionary<Vector2, Tile>();

    public void InitializeGrid(int width, int height)
    {
        mapGrid = new Tile[width, height];
    }

    public void AddTile(Tile tile, Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            mapGrid[position.x, position.y] = tile;
            hexCells[tile.OffsetCoordinates] = tile;
        }
    }

    public void AssignNeighbors()
    {
        foreach (var cell in hexCells)
        {
            Tile tile = cell.Value;
            List<Tile> neighbors = HexUtility.GetNeighbors(tile, hexCells);

            foreach (Tile neighbor in neighbors)
            {
                tile.AddNeighbor(neighbor);
            }
        }
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
