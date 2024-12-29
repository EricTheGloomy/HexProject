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

        foreach (var tile in tiles.Values)
        {
            tile.SetVisibility(VisibilityState.Hidden);
            tile.TileDecorations.SetActive(false);
            tile.TileModel.SetActive(false);
        }

        Tile startingTile = FindStartingTile();
        if (startingTile != null)
        {
            Debug.Log($"FogOfWarManager: Revealing area around starting tile at {startingTile.Attributes.GridPosition}.");
            RevealAreaAroundTile(startingTile);
        }
        else
        {
            Debug.LogError("FogOfWarManager: No starting location found.");
        }
    }

    private Tile FindStartingTile()
    {
        if (allTiles == null || allTiles.Count == 0)
        {
            Debug.LogError("FogOfWarManager: Tile grid is null or empty. Cannot find starting tile.");
            return null;
        }

        foreach (var tile in allTiles.Values)
        {
            if (tile.Attributes.Gameplay.IsStartingLocation)
            {
                Debug.Log($"FogOfWarManager: Found starting tile at {tile.Attributes.GridPosition}.");
                return tile;
            }
        }

        Debug.LogError("FogOfWarManager: No starting tile found.");
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
                tile.TileDecorations.SetActive(true);
                tile.TileModel.SetActive(true);

                revealedTiles.Add(tile);
            }
        }

        Debug.Log($"FogOfWarManager: Revealed {tilesToReveal.Count} tiles around {centerTile.Attributes.GridPosition}.");
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
