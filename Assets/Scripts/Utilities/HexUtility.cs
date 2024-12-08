using System.Collections.Generic;
using UnityEngine;

public static class HexUtility
{
    public static List<Tile> GetNeighbors(Tile center, Dictionary<Vector2, Tile> hexCells)
    {
        List<Tile> neighbors = new List<Tile>();
        foreach (var offset in HexCoordinateHelper.GetCubeNeighborOffsets())
        {
            Vector3 neighborCubeCoords = center.CubeCoordinates + offset;
            Vector2 neighborOffsetCoords = HexCoordinateHelper.CubeToAxial(neighborCubeCoords);
            neighborOffsetCoords = HexCoordinateHelper.AxialToOffset(neighborOffsetCoords);

            if (hexCells.TryGetValue(neighborOffsetCoords, out Tile neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
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
}
