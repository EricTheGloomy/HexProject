// File: Scripts/Managers/TileInteractionManager.cs
using UnityEngine;

public class TileInteractionManager : MonoBehaviour
{
    public Camera mainCamera; // Assign the main camera in the editor
    private Tile selectedTile;

    [SerializeField] private GameObject selectionIndicatorPrefab; // Prefab for the selection indicator
    private GameObject activeSelectionIndicator;

    private void Start()
    {
        // Instantiate the selection indicator once and disable it initially
        if (selectionIndicatorPrefab != null)
        {
            activeSelectionIndicator = Instantiate(selectionIndicatorPrefab);
            activeSelectionIndicator.SetActive(false); // Ensure it's not visible until a tile is selected
        }
        else
        {
            Debug.LogError("TileInteractionManager: Selection Indicator Prefab is not assigned!");
        }
    }

    private void Update()
    {
        HandleTileSelection();
        HandleTileDeselection(); // Check for right-click to deselect
    }

    private void HandleTileSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile != null && clickedTile.Visibility == VisibilityState.Visible)
                {
                    SelectTile(clickedTile);
                }
            }
        }
    }

    private void HandleTileDeselection()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button click
        {
            if (selectedTile != null) // Check if a tile is currently selected
            {
                DeselectCurrentTile();
                Debug.Log("Tile deselected via right mouse button.");
            }
        }
    }

    private void SelectTile(Tile tile)
    {
        if (selectedTile == tile)
        {
            Debug.Log("Tile is already selected.");
            return;
        }

        DeselectCurrentTile();

        selectedTile = tile;
        selectedTile.IsSelected = true;

        // Update the position of the selection indicator and activate it
        if (activeSelectionIndicator != null)
        {
            activeSelectionIndicator.transform.position = tile.transform.position;
            activeSelectionIndicator.SetActive(true);
        }

        Debug.Log($"Tile at {tile.GridPosition} selected.");
    }

    private void DeselectCurrentTile()
    {
        if (selectedTile != null)
        {
            selectedTile.IsSelected = false;
            selectedTile = null; // Clear the reference to the selected tile
        }

        // Hide the selection indicator when no tile is selected
        if (activeSelectionIndicator != null)
        {
            activeSelectionIndicator.SetActive(false);
        }
    }
}
