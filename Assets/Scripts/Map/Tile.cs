using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Tile : MonoBehaviour, IInteractable
{
    // Reference to TileAttributes, serialized for Inspector visibility
    [SerializeField]
    private TileAttributes attributes;
    public TileAttributes Attributes => attributes; // Read-only accessor

    [Header("Tile Components")]
    public GameObject TileModel;
    public GameObject TileDecorations;
    public GameObject FogOverlay;

    [SerializeField]
    private List<Tile> neighbors = new List<Tile>();
    public List<Tile> Neighbors => neighbors;

    public bool IsSelected { get; private set; }

    public void Initialize(TileAttributes newAttributes, bool useFlatTop, float tileSizeX, float tileSizeZ)
    {
        attributes = newAttributes;

        // Calculate coordinates
        attributes.OffsetCoordinates = new Vector2(attributes.GridPosition.x, attributes.GridPosition.y);
        Vector2 axialCoords = HexCoordinateHelper.OffsetToAxial(attributes.OffsetCoordinates);
        attributes.CubeCoordinates = HexCoordinateHelper.AxialToCube(axialCoords);

        // Set world position
        transform.position = HexCoordinateHelper.GetWorldPosition(attributes.OffsetCoordinates, useFlatTop, tileSizeX, tileSizeZ);

        // Apply visuals
        ApplyTileVisuals();
    }

    public void AddNeighbor(Tile neighbor)
    {
        if (!neighbors.Contains(neighbor))
        {
            neighbors.Add(neighbor);
            Debug.Log($"Added neighbor {neighbor.Attributes.GridPosition} to {Attributes.GridPosition}");
        }
    }

    private void ApplyTileVisuals()
    {
        if (attributes.TileTypeData != null && TileModel != null)
        {
            Renderer renderer = TileModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Apply visual updates here if necessary
            }
        }
    }

    public void SetVisibility(VisibilityState state)
    {
        attributes.Visibility = state;
        if (FogOverlay != null)
        {
            FogOverlay.SetActive(state == VisibilityState.Hidden);
        }
    }

    public void SetTileTypeData(TileTypeData tileTypeData)
    {
        attributes.TileTypeData = tileTypeData;
        ApplyTileVisuals();
    }

    public void SetAsStartingLocation()
    {
        attributes.Gameplay.IsStartingLocation = true;
        attributes.Gameplay.HasHousing = true;
        attributes.Gameplay.IsOccupied = true;
        Debug.Log($"Tile at {attributes.GridPosition} marked as starting location.");
    }

    public void Interact()
    {
        IsSelected = !IsSelected;
        Debug.Log($"Tile at {attributes.GridPosition} {(IsSelected ? "selected" : "deselected")}.");
    }

    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractionDescription()
    {
        return IsSelected ? "Tile is already selected" : "Click to select this tile";
    }
}
