using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour, IFogOfWarManager
{
    [Header("Configuration")]
    [SerializeField] private FogConfig fogConfig; // Reference to the ScriptableObject

    private Dictionary<Vector2, Tile> allTiles; // Dictionary of all tiles
    private HashSet<Tile> revealedTiles = new HashSet<Tile>(); // Tracks tiles that are revealed

    public void Initialize(Dictionary<Vector2, Tile> tiles)
    {
        allTiles = tiles;

        // Set all tiles to Hidden initially
        foreach (var tile in allTiles.Values)
        {
            tile.SetVisibility(VisibilityState.Hidden);
        }

        Debug.Log("FogOfWarManager: All tiles initialized under fog.");

        // Find and reveal the starting location
        Tile startingTile = FindStartingTile();
        if (startingTile != null)
        {
            RevealAreaAroundTile(startingTile);
        }
        else
        {
            Debug.LogError("FogOfWarManager: No starting location found.");
        }
    }

    private Tile FindStartingTile()
    {
        foreach (var tile in allTiles.Values)
        {
            if (tile.IsStartingLocation) // Check runtime-specific flag
            {
                return tile;
            }
        }

        Debug.LogError("FogOfWarManager: No starting tile marked as starting location.");
        return null;
    }

    public void RevealAreaAroundTile(Tile centerTile)
    {
        if (centerTile == null)
        {
            Debug.LogError("FogOfWarManager: Center tile is null. Cannot reveal fog.");
            return;
        }

        // Get tiles in the specified radius
        int radius = fogConfig != null ? fogConfig.RevealRadius : 2; // Fallback to 2 if config is missing
        var tilesToReveal = HexUtility.GetHexesInRange(centerTile, radius, allTiles);

        // Reveal each tile in the range
        foreach (var tile in tilesToReveal)
        {
            if (!revealedTiles.Contains(tile))
            {
                tile.SetVisibility(VisibilityState.Visible); // Update tile visibility
                revealedTiles.Add(tile); // Mark the tile as revealed
            }
        }

        Debug.Log($"FogOfWarManager: Revealed {tilesToReveal.Count} tiles around {centerTile.GridPosition}.");
    }

    public VisibilityState GetFogState(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("FogOfWarManager: Tile is null. Cannot determine fog state.");
            return VisibilityState.Hidden; // Default to hidden for invalid input
        }

        return revealedTiles.Contains(tile) ? VisibilityState.Visible : VisibilityState.Hidden;
    }
}
