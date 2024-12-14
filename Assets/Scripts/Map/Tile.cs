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

    public TileTypeData TileTypeData { get; private set; }

    public Vector2 OffsetCoordinates { get; private set; }
    public Vector3 CubeCoordinates { get; private set; }

    [SerializeField]
    private List<Tile> neighbors = new List<Tile>();
    public List<Tile> Neighbors => neighbors;

    public bool IsStartingLocation { get; private set; }
    
    [SerializeField]
    private bool isSelected = false; // Tracks whether this tile is selected
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            OnSelectionStateChanged(isSelected);
        }
    }

    private void OnSelectionStateChanged(bool isSelected)
    {
        // This method can be expanded for custom behaviors when selection changes
        // For example, you might want to trigger a visual effect or log changes
        if (isSelected)
        {
            Debug.Log($"Tile at {GridPosition} is selected.");
        }
        else
        {
            Debug.Log($"Tile at {GridPosition} is deselected.");
        }
    }

    public void Initialize(Vector2Int gridPosition, bool useFlatTop, float hexWidth, float hexHeight, TileTypeData tileTypeData)
    {
        GridPosition = gridPosition;
        TileTypeData = tileTypeData;

        OffsetCoordinates = new Vector2(gridPosition.x, gridPosition.y);

        Vector2 axialCoords = HexCoordinateHelper.OffsetToAxial(OffsetCoordinates);
        CubeCoordinates = HexCoordinateHelper.AxialToCube(axialCoords);

        transform.position = HexCoordinateHelper.GetWorldPosition(OffsetCoordinates, useFlatTop, hexWidth, hexHeight);

        ApplyTileVisuals();
    }

    private void ApplyTileVisuals()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // Apply visuals based on TileTypeData prefab (if needed later)
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

    public void SetAsStartingLocation()
    {
        IsStartingLocation = true;
        Debug.Log($"Tile at {GridPosition} marked as starting location.");
    }

    public void SetTileTypeData(TileTypeData tileTypeData)
    {
        TileTypeData = tileTypeData;
    }
}
