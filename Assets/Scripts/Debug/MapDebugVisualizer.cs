using UnityEngine;
using System.Collections.Generic;

public class MapDebugVisualizer : MonoBehaviour
{
    public enum DebugMode
    {
        Default,
        Elevation,
        Moisture,
        Temperature
    }

    [Header("Debug Settings")]
    public DebugMode debugMode = DebugMode.Default;
    [Range(0f, 1f)] public float overlayOpacity = 0.5f;

    [Header("References")]
    public HexGridDataManager gridManager;

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            enabled = false;
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("MapDebugVisualizer: HexGridDataManager reference is missing!");
        }
    }

    private void Update()
    {
        HandleInput(); // Check for key presses and switch modes accordingly
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            debugMode = DebugMode.Default;
            UpdateVisualization();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            debugMode = DebugMode.Elevation;
            UpdateVisualization();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            debugMode = DebugMode.Moisture;
            UpdateVisualization();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            debugMode = DebugMode.Temperature;
            UpdateVisualization();
        }
    }

    private void UpdateVisualization()
    {
        if (gridManager == null || !gridManager.isGridReady)
        {
            Debug.LogError("MapDebugVisualizer: Grid manager not ready or missing.");
            return;
        }

        Dictionary<Vector2, Tile> tiles = gridManager.GetHexCells();
        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("MapDebugVisualizer: No tiles found! Ensure HexGridDataManager has initialized the grid.");
            return;
        }

        switch (debugMode)
        {
            case DebugMode.Default:
                ClearTileDebug(tiles);
                break;

            case DebugMode.Elevation:
                ApplyVisualization(tiles, tile => tile.Attributes.Procedural.Elevation);
                break;

            case DebugMode.Moisture:
                ApplyVisualization(tiles, tile => tile.Attributes.Procedural.Moisture);
                break;

            case DebugMode.Temperature:
                ApplyVisualization(tiles, tile => tile.Attributes.Procedural.Temperature);
                break;
        }
    }

    private void ApplyVisualization(Dictionary<Vector2, Tile> tiles, System.Func<Tile, float> valueSelector)
    {
        foreach (var tileEntry in tiles)
        {
            Tile tile = tileEntry.Value;

            // Get the value for the selected mode and clamp it
            float value = Mathf.Clamp01(valueSelector(tile));

            // Apply color based on the value
            ApplyDebugColor(tile, value);
        }
    }

    private void ApplyDebugColor(Tile tile, float value)
    {
        // Create grayscale color: 0 -> white, 1 -> black
        Color debugColor = Color.Lerp(Color.white, Color.black, value);

        if (tile.TileModel != null)
        {
            Renderer renderer = tile.TileModel.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                // Create a unique material for the tile
                Material tileMaterial = new Material(Shader.Find("Unlit/Color"))
                {
                    color = new Color(debugColor.r, debugColor.g, debugColor.b, overlayOpacity)
                };

                renderer.material = tileMaterial;
            }
            else
            {
                Debug.LogWarning($"Tile at {tile.Attributes.GridPosition} has no Renderer component!");
            }
        }
        else
        {
            Debug.LogWarning($"Tile at {tile.Attributes.GridPosition} has no TileModel assigned!");
        }
    }

    private void ClearTileDebug(Dictionary<Vector2, Tile> tiles)
    {
        foreach (var tileEntry in tiles)
        {
            Tile tile = tileEntry.Value;

            if (tile.TileModel != null)
            {
                Renderer renderer = tile.TileModel.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // Reset the material to the default base material
                    renderer.material = tile.Attributes.TileTypeData.BaseMaterial;
                }
            }
        }
    }
}
