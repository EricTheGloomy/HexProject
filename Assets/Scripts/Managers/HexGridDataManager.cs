// File: Scripts/Managers/HexGridDataManager.cs
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

            // Use HexCoordinateHelper for neighbor offsets
            Vector3[] neighborOffsets = HexCoordinateHelper.GetCubeNeighborOffsets();

            foreach (var offset in neighborOffsets)
            {
                Vector3 neighborCubeCoords = tile.CubeCoordinates + offset;

                // Convert cube coordinates back to offset for lookup
                Vector2 neighborOffsetCoords = HexCoordinateHelper.CubeToAxial(neighborCubeCoords);
                neighborOffsetCoords = HexCoordinateHelper.AxialToOffset(neighborOffsetCoords);

                if (hexCells.TryGetValue(neighborOffsetCoords, out Tile neighbor))
                {
                    tile.AddNeighbor(neighbor);
                }
            }
        }
    }

    public List<Tile> GetHexesInRange(Tile center, int range)
    {
        List<Tile> hexesInRange = new List<Tile>();
        Queue<Tile> frontier = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // Initialize the search with the center tile
        frontier.Enqueue(center);
        visited.Add(center);

        int currentRange = 0;

        // Perform breadth-first search up to the given range
        while (frontier.Count > 0 && currentRange < range)
        {
            int levelSize = frontier.Count; // Process all tiles in the current range
            for (int i = 0; i < levelSize; i++)
            {
                Tile current = frontier.Dequeue();
                hexesInRange.Add(current);

                foreach (Tile neighbor in current.Neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        frontier.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            currentRange++;
        }

        return hexesInRange;
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

    private Vector2[] GetNeighborOffsets(int row)
    {
        return row % 2 == 0
            ? new Vector2[] { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, -1) }
            : new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, -1) };
    }
}