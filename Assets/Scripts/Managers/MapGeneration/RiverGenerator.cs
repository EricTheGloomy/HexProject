using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : IMapGenerationStep
{
    private readonly MapGenerationConfig config;

    public RiverGenerator(MapGenerationConfig config)
    {
        this.config = config;
    }

    public void Generate(Dictionary<Vector2, Tile> tiles)
    {
        Debug.Log($"RiverGenerator: Attempting to generate {config.NumberOfRivers} rivers...");

        int riversGenerated = 0;
        int attempts = 0;

        // Keep track of tiles already used by rivers and their neighbors
        HashSet<Tile> restrictedTiles = new HashSet<Tile>();

        while (riversGenerated < config.NumberOfRivers && attempts < config.MaxRiverGenerationRetries)
        {
            Tile randomLandTile = FindRandomEligibleLandTile(tiles, restrictedTiles);
            if (randomLandTile == null)
            {
                Debug.LogWarning("No eligible land tiles found.");
                break;
            }

            var closestTiles = FindClosestLandNeighborOfWater(randomLandTile, tiles, config.MinRiverLength, restrictedTiles);
            Tile closestLandNeighborOfWaterTile = closestTiles.LandTile;
            Tile correspondingWaterTile = closestTiles.WaterTile;

            if (closestLandNeighborOfWaterTile == null || correspondingWaterTile == null)
            {
                Debug.LogWarning($"No valid land neighbors of water tiles found at or beyond the minimum range of {config.MinRiverLength} from the selected river start.");
                attempts++;
                continue;
            }

            List<Tile> riverPath = FindRiverPath(randomLandTile, closestLandNeighborOfWaterTile, tiles, restrictedTiles);
            if (riverPath != null)
            {
                foreach (Tile tile in riverPath)
                {
                    tile.Attributes.Gameplay.HasRiver = true;
                }

                SetRiverConnectionsSequentially(riverPath, correspondingWaterTile);

                if (config.RestrictNeighbors)
                {
                    restrictedTiles.UnionWith(riverPath);
                    foreach (Tile tile in riverPath)
                    {
                        foreach (Tile neighbor in HexUtility.GetNeighbors(tile, tiles))
                        {
                            restrictedTiles.Add(neighbor);
                        }
                    }
                }

                riversGenerated++;
                Debug.Log($"River {riversGenerated} generated with {riverPath.Count} tiles.");
            }
            else
            {
                randomLandTile.Attributes.Gameplay.HasRiver = false;
                closestLandNeighborOfWaterTile.Attributes.Gameplay.HasRiver = false;
                Debug.LogWarning("Failed to generate a river path. Rolling back.");

                attempts++;
            }
        }

        Debug.Log($"River generation completed. {riversGenerated} rivers successfully created.");
    }

    private void SetRiverConnection(Tile currentTile, Tile nextTile, Dictionary<Vector2, Tile> tiles)
    {
        // Find the edge from the current tile to the next tile
        int currentToNextEdge = HexUtility.GetEdgeBetweenTiles(currentTile, nextTile);
        if (currentToNextEdge == -1)
        {
            Debug.LogWarning("Unable to determine edge between tiles.");
            return;
        }

        // Find the reverse edge from the next tile to the current tile
        int nextToCurrentEdge = (currentToNextEdge + 3) % 6; // Opposite edge on hex grid

        // Set the river connections
        currentTile.Attributes.Gameplay.RiverConnections[currentToNextEdge] = true;
        nextTile.Attributes.Gameplay.RiverConnections[nextToCurrentEdge] = true;
    }

    private Tile FindRandomEligibleLandTile(Dictionary<Vector2, Tile> tiles, HashSet<Tile> restrictedTiles)
    {
        List<Tile> landTiles = new List<Tile>();

        foreach (var tile in tiles.Values)
        {
            if (!restrictedTiles.Contains(tile) &&
                tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Land &&
                tile.Attributes.Procedural.Elevation > config.MinElevationForRiverStart)
            {
                landTiles.Add(tile);
            }
        }

        Debug.Log($"Found {landTiles.Count} eligible land tiles.");
        if (landTiles.Count > 0)
        {
            System.Random random = new System.Random();
            return landTiles[random.Next(landTiles.Count)];
        }

        return null;
    }

    private (Tile LandTile, Tile WaterTile) FindClosestLandNeighborOfWater(Tile startTile, Dictionary<Vector2, Tile> tiles, int minRiverLength, HashSet<Tile> restrictedTiles)
    {
        Tile closestLandTile = null;
        Tile correspondingWaterTile = null;
        float closestDistance = float.MaxValue;

        foreach (var tile in tiles.Values)
        {
            if (tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Water)
            {
                float distance = Vector2.Distance(startTile.Attributes.GridPosition, tile.Attributes.GridPosition);

                if (distance >= minRiverLength && distance < closestDistance)
                {
                    List<Tile> neighbors = HexUtility.GetNeighbors(tile, tiles);
                    foreach (var neighbor in neighbors)
                    {
                        if (!restrictedTiles.Contains(neighbor) &&
                            neighbor.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Land)
                        {
                            closestLandTile = neighbor;
                            correspondingWaterTile = tile;
                            closestDistance = distance;
                        }
                    }
                }
            }
        }

        if (closestLandTile != null)
        {
            Debug.Log($"Closest valid land neighbor of water tile found at distance {closestDistance}.");
        }
        else
        {
            Debug.LogWarning("No valid land neighbors of water tiles satisfy the minimum river length requirement.");
        }

        return (closestLandTile, correspondingWaterTile);
    }

    private List<Tile> FindRiverPath(Tile startTile, Tile endTile, Dictionary<Vector2, Tile> tiles, HashSet<Tile> restrictedTiles)
    {
        var openList = new PriorityQueue<RiverNode>();
        var closedList = new HashSet<Tile>();

        var startNode = new RiverNode
        {
            Tile = startTile,
            GCost = 0,
            HCost = Vector2.Distance(startTile.Attributes.GridPosition, endTile.Attributes.GridPosition),
        };

        openList.Enqueue(startNode, startNode.FCost);

        while (openList.Count > 0)
        {
            var currentNode = openList.Dequeue();

            if (currentNode.Tile == endTile)
            {
                return ReconstructPath(currentNode);
            }

            closedList.Add(currentNode.Tile);

            foreach (var neighbor in HexUtility.GetNeighbors(currentNode.Tile, tiles))
            {
                if (closedList.Contains(neighbor) || restrictedTiles.Contains(neighbor) ||
                    neighbor.Attributes.Procedural.FixedElevationCategory != TileTypeDataMappingConfig.ElevationCategory.Land)
                    continue;

                float gCost = currentNode.GCost + CalculateCost(currentNode.Tile, neighbor);
                float hCost = Vector2.Distance(neighbor.Attributes.GridPosition, endTile.Attributes.GridPosition);

                var neighborNode = new RiverNode
                {
                    Tile = neighbor,
                    Parent = currentNode,
                    GCost = gCost,
                    HCost = hCost,
                };

                openList.Enqueue(neighborNode, neighborNode.FCost);
            }
        }

        Debug.LogWarning("No valid path found between the start and end tiles.");
        return null;
    }

    private float CalculateCost(Tile from, Tile to)
    {
        float elevationChange = to.Attributes.Procedural.Elevation - from.Attributes.Procedural.Elevation;
        return config.BaseRiverPathCost + (elevationChange > 0 ? elevationChange * config.ElevationCostMultiplier : Mathf.Abs(elevationChange));
    }

    private List<Tile> ReconstructPath(RiverNode endNode)
    {
        var path = new List<Tile>();
        var current = endNode;

        while (current != null)
        {
            path.Add(current.Tile);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private void SetRiverConnectionsSequentially(List<Tile> riverPath, Tile waterTile)
    {
        for (int i = 0; i < riverPath.Count - 1; i++)
        {
            Tile currentTile = riverPath[i];
            Tile nextTile = riverPath[i + 1];

            // Calculate the edge between the current and next tiles
            int currentToNextEdge = HexUtility.GetEdgeBetweenTiles(currentTile, nextTile);
            if (currentToNextEdge == -1)
            {
                Debug.LogWarning($"Invalid edge between tiles: {currentTile.Attributes.GridPosition} and {nextTile.Attributes.GridPosition}");
                continue;
            }

            // Calculate the opposite edge for the next tile
            int nextToCurrentEdge = (currentToNextEdge + 3) % 6;

            // Set the river connections for both tiles
            currentTile.Attributes.Gameplay.RiverConnections[currentToNextEdge] = true;
            nextTile.Attributes.Gameplay.RiverConnections[nextToCurrentEdge] = true;

            Debug.Log($"River connection set: {currentTile.Attributes.GridPosition} edge {currentToNextEdge} <-> {nextTile.Attributes.GridPosition} edge {nextToCurrentEdge}");
        }

        // Connect the final river tile to the water tile
        Tile lastTile = riverPath[riverPath.Count - 1];
        int lastToWaterEdge = HexUtility.GetEdgeBetweenTiles(lastTile, waterTile);

        if (lastToWaterEdge != -1)
        {
            lastTile.Attributes.Gameplay.RiverConnections[lastToWaterEdge] = true;
            Debug.Log($"Final river tile connected to water neighbor: {lastTile.Attributes.GridPosition} edge {lastToWaterEdge}");
        }
        else
        {
            Debug.LogWarning($"Failed to connect last river tile to water neighbor: {lastTile.Attributes.GridPosition}");
        }
    }

}




// Supporting Classes
public class RiverNode
{
    public Tile Tile;
    public RiverNode Parent;
    public float GCost; // Cost to reach this node
    public float HCost; // Estimated cost to goal
    public float FCost => GCost + HCost; // Total cost
}

// A simple priority queue implementation can be added separately or replaced with any available library.
public class PriorityQueue<T>
{
    private readonly SortedList<float, Queue<T>> _elements = new SortedList<float, Queue<T>>();

    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!_elements.ContainsKey(priority))
        {
            _elements[priority] = new Queue<T>();
        }

        _elements[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            throw new System.InvalidOperationException("The queue is empty.");
        }

        var firstKey = _elements.Keys[0];
        var firstQueue = _elements[firstKey];
        var item = firstQueue.Dequeue();
        if (firstQueue.Count == 0)
        {
            _elements.Remove(firstKey);
        }

        Count--;
        return item;
    }
}
