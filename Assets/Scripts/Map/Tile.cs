// File: Scripts/Map/Tile.cs
using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition;
    public VisibilityState Visibility;
    public string TerrainType;
    public GameObject TileModel;
    public GameObject TileDecorations;
    public GameObject FogOverlay;

    public TileData TileType { get; private set; } // Reference to the scriptable object

    public Vector2 OffsetCoordinates { get; private set; }
    public Vector3 CubeCoordinates { get; private set; }

    [SerializeField]
    private List<Tile> neighbors = new List<Tile>();
    public List<Tile> Neighbors => neighbors;

    public void Initialize(Vector2Int gridPosition, float hexWidth, float hexHeight, TileData tileType)
    {
        GridPosition = gridPosition;
        TileType = tileType;

        // Calculate Offset Coordinates
        OffsetCoordinates = new Vector2(gridPosition.x, gridPosition.y);

        // Calculate Cube Coordinates
        Vector2 axialCoords = HexCoordinateHelper.OffsetToAxial(OffsetCoordinates);
        CubeCoordinates = HexCoordinateHelper.AxialToCube(axialCoords);

        // Set World Position
        transform.position = HexCoordinateHelper.GetWorldPosition(OffsetCoordinates, false, hexWidth, hexHeight);

        // Apply tile-specific visuals (if necessary)
        ApplyTileVisuals();
    }

    private void ApplyTileVisuals()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // Apply visuals based on TileType prefab (if needed later)
        }
    }

    public void AddNeighbor(Tile neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            neighbor.neighbors.Add(this); // Ensure bi-directional relationship
        }
    }

    public void SetVisibility(VisibilityState newState)
    {
        Visibility = newState;
        FogOverlay?.SetActive(Visibility == VisibilityState.Hidden);
    }
}