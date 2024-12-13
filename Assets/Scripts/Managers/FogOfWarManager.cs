using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour, IFogOfWarManager
{
    [Header("Configuration")]
    [SerializeField] private FogConfig fogConfig;

    private Dictionary<Vector2, Tile> allTiles;
    private HashSet<Tile> revealedTiles = new HashSet<Tile>();

    public void Initialize(Dictionary<Vector2, Tile> tiles)
    {
        allTiles = tiles;

        foreach (var tile in allTiles.Values)
        {
            tile.SetVisibility(VisibilityState.Hidden);
        }

        Debug.Log("FogOfWarManager: All tiles initialized under fog.");

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
            if (tile.IsStartingLocation)
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

        int radius = fogConfig != null ? fogConfig.RevealRadius : 4; // Fallback to 4 if config is missing
        var tilesToReveal = HexUtility.GetHexesInRange(centerTile, radius, allTiles);

        foreach (var tile in tilesToReveal)
        {
            if (!revealedTiles.Contains(tile))
            {
                tile.SetVisibility(VisibilityState.Visible);
                revealedTiles.Add(tile);
            }
        }

        Debug.Log($"FogOfWarManager: Revealed {tilesToReveal.Count} tiles around {centerTile.GridPosition}.");
    }

    public VisibilityState GetFogState(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("FogOfWarManager: Tile is null. Cannot determine fog state.");
            return VisibilityState.Hidden;
        }

        return revealedTiles.Contains(tile) ? VisibilityState.Visible : VisibilityState.Hidden;
    }
}
