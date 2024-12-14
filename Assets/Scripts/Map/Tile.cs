// Tile.cs - Updated to implement IInteractable
using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour, IInteractable
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

    public void Interact()
    {
        SetSelected(!isSelected);
    }

    public bool CanInteract()
    {
        return true; // Tiles are always interactable for now
    }

    public string GetInteractionDescription()
    {
        return isSelected ? "Tile is already selected" : "Click to select this tile";
    }

    private void OnSelectionStateChanged(bool isSelected)
    {
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
            // Apply visuals based on TileTypeData prefab
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        OnSelectionStateChanged(selected);
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