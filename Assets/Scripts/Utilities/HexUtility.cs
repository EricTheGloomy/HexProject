using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HexUtility
{    
    public static List<Tile> GetNeighbors(Tile tile, Dictionary<Vector2, Tile> hexCells)
    {
        List<Tile> neighbors = new List<Tile>();

        // Get the cube neighbor offsets in the correct NE -> E -> SE -> SW -> W -> NW order
        Vector3[] neighborOffsets = HexCoordinateHelper.GetCubeNeighborOffsets();

        foreach (var offset in neighborOffsets)
        {
            // Calculate the cube coordinates of the neighbor
            Vector3 neighborCubeCoords = tile.Attributes.CubeCoordinates + offset;

            // Convert cube coordinates to offset coordinates for lookup
            Vector2 neighborOffsetCoords = HexCoordinateHelper.CubeToAxial(neighborCubeCoords);
            neighborOffsetCoords = HexCoordinateHelper.AxialToOffset(neighborOffsetCoords);

            // Add neighbors in order
            if (hexCells.TryGetValue(neighborOffsetCoords, out Tile neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        // Neighbors are now guaranteed to be in the correct NE -> E -> SE -> SW -> W -> NW order
        return neighbors;
    }

    public static List<Tile> GetHexesInRange(Tile center, int range, Dictionary<Vector2, Tile> hexCells)
    {
        List<Tile> hexesInRange = new List<Tile>();
        Queue<Tile> frontier = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        frontier.Enqueue(center);
        visited.Add(center);

        int currentRange = 0;
        while (frontier.Count > 0 && currentRange < range)
        {
            int levelSize = frontier.Count;
            for (int i = 0; i < levelSize; i++)
            {
                Tile current = frontier.Dequeue();
                hexesInRange.Add(current);

                foreach (Tile neighbor in GetNeighbors(current, hexCells))
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

    public static int GetEdgeBetweenTiles(Tile currentTile, Tile neighborTile)
    {
        Vector2 currentPos = currentTile.Attributes.GridPosition;
        Vector2 neighborPos = neighborTile.Attributes.GridPosition;

        // Calculate direction with rounding to avoid precision issues
        Vector2 direction = new Vector2(
            Mathf.Round(neighborPos.x - currentPos.x),
            Mathf.Round(neighborPos.y - currentPos.y)
        );

//        Debug.Log($"Calculating edge from {currentTile.Attributes.GridPosition} to {neighborTile.Attributes.GridPosition}, Direction: {direction}");

        // Determine if the current row is even or odd
        bool isEvenRow = ((int)currentPos.y % 2) == 0;

        // Unified edge determination for all cases
        if (isEvenRow)
        {
            if (direction == new Vector2(0, 1)) return 0;  // Top Right (NE)
            if (direction == new Vector2(1, 0)) return 1;  // Right (E)
            if (direction == new Vector2(0, -1)) return 2; // Bottom Right (SE)
            if (direction == new Vector2(-1, -1)) return 3; // Bottom Left (SW)
            if (direction == new Vector2(-1, 0)) return 4; // Left (W)
            if (direction == new Vector2(-1, 1)) return 5;  // Top Left (NW)
        }
        else
        {
            if (direction == new Vector2(1, 1)) return 0;  // Top Right (NE)
            if (direction == new Vector2(1, 0)) return 1;  // Right (E)
            if (direction == new Vector2(1, -1)) return 2; // Bottom Right (SE)
            if (direction == new Vector2(0, -1)) return 3; // Bottom Left (SW)
            if (direction == new Vector2(-1, 0)) return 4; // Left (W)
            if (direction == new Vector2(0, 1)) return 5;  // Top Left (NW)
        }

        Debug.LogWarning($"Invalid direction between tiles at {currentPos} and {neighborPos}: {direction}");
        return -1; // Invalid direction
    }

    public static int GetHexDistance(Tile origin, Tile target)
    {
        Vector3 originCube = origin.Attributes.CubeCoordinates;
        Vector3 targetCube = target.Attributes.CubeCoordinates;

        return (int)((Mathf.Abs(originCube.x - targetCube.x) 
                    + Mathf.Abs(originCube.y - targetCube.y) 
                    + Mathf.Abs(originCube.z - targetCube.z)) / 2);
    }

    public static int GetHexDistance(Vector3 originCube, Vector3 targetCube)
    {
        return (int)((Mathf.Abs(originCube.x - targetCube.x) 
                    + Mathf.Abs(originCube.y - targetCube.y) 
                    + Mathf.Abs(originCube.z - targetCube.z)) / 2);
    }

}
