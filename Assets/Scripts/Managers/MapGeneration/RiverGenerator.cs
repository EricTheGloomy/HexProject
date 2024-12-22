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

        while (riversGenerated < config.NumberOfRivers && attempts < config.NumberOfRivers * 2)
        {
            // Step 1: Find a random eligible land tile
            Tile randomLandTile = FindRandomEligibleLandTile(tiles, restrictedTiles);
            if (randomLandTile == null)
            {
                Debug.LogWarning("No eligible land tiles found.");
                break;
            }

            // Step 2: Find the closest water neighbor's land neighbor within MinRiverLength
            Tile closestLandNeighborOfWaterTile = FindClosestLandNeighborOfWater(randomLandTile, tiles, config.MinRiverLength, restrictedTiles);
            if (closestLandNeighborOfWaterTile == null)
            {
                Debug.LogWarning($"No valid land neighbors of water tiles found at or beyond the minimum range of {config.MinRiverLength} from the selected river start.");
                attempts++;
                continue;
            }

            // Step 3: Attempt to generate river path
            List<Tile> riverPath = FindRiverPath(randomLandTile, closestLandNeighborOfWaterTile, tiles, restrictedTiles);
            if (riverPath != null)
            {
                foreach (var tile in riverPath)
                {
                    tile.Attributes.Gameplay.HasRiver = true;
                    restrictedTiles.Add(tile);

                    // Also restrict neighbors of the river tiles
                    foreach (var neighbor in HexUtility.GetNeighbors(tile, tiles))
                    {
                        restrictedTiles.Add(neighbor);
                    }
                }
                riversGenerated++;
                Debug.Log($"River {riversGenerated} generated with {riverPath.Count} tiles.");
            }
            else
            {
                // Rollback: Mark the start and end tiles as not having a river
                randomLandTile.Attributes.Gameplay.HasRiver = false;
                closestLandNeighborOfWaterTile.Attributes.Gameplay.HasRiver = false;
                Debug.LogWarning("Failed to generate a river path. Rolling back.");
                attempts++;
            }
        }

        Debug.Log($"River generation completed. {riversGenerated} rivers successfully created.");
    }

    private Tile FindRandomEligibleLandTile(Dictionary<Vector2, Tile> tiles, HashSet<Tile> restrictedTiles)
    {
        List<Tile> landTiles = new List<Tile>();

        foreach (var tile in tiles.Values)
        {
            if (!restrictedTiles.Contains(tile) &&
                tile.Attributes.Procedural.FixedElevationCategory == TileTypeDataMappingConfig.ElevationCategory.Land &&
                tile.Attributes.Procedural.Elevation > 0.5f)
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

    private Tile FindClosestLandNeighborOfWater(Tile startTile, Dictionary<Vector2, Tile> tiles, int minRiverLength, HashSet<Tile> restrictedTiles)
    {
        Tile closestTile = null;
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
                            closestTile = neighbor;
                            closestDistance = distance;
                        }
                    }
                }
            }
        }

        if (closestTile != null)
        {
            Debug.Log($"Closest valid land neighbor of water tile found at distance {closestDistance}.");
        }
        else
        {
            Debug.LogWarning("No valid land neighbors of water tiles satisfy the minimum river length requirement.");
        }

        return closestTile;
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
        return elevationChange > 0 ? 1 + elevationChange * 5 : 1 + Mathf.Abs(elevationChange);
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