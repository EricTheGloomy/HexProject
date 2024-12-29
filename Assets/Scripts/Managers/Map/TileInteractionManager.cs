using System;
using UnityEngine;

public class TileInteractionManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject selectionIndicatorPrefab;

    private GameObject activeSelectionIndicator;
    private IInteractable currentInteractable;

    public static event Action<Tile> OnTileSelected;
    public static event Action OnTileDeselected;

    private void Start()
    {
        if (selectionIndicatorPrefab != null)
        {
            activeSelectionIndicator = Instantiate(selectionIndicatorPrefab);
            activeSelectionIndicator.SetActive(false);
        }
        else
        {
            Debug.LogError("TileInteractionManager: Selection Indicator Prefab is not assigned!");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            HandleTileInteraction();
        }

        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            HandleTileDeselection();
        }
    }

    private void HandleTileInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            IInteractable interactable = tile as IInteractable;

            if (tile == null || interactable == null)
            {
                Debug.LogWarning("TileInteractionManager: No valid tile selected.");
                ClearSelectionIndicator();
                return;
            }

            if (tile.Attributes.Visibility == VisibilityState.Hidden)
            {
                Debug.Log("TileInteractionManager: Cannot select a tile under fog of war.");
                ClearSelectionIndicator();
                return;
            }

            interactable.Interact();
            currentInteractable = interactable;

            UpdateSelectionIndicator(hit.collider.transform.position);

            // Notify listeners about the tile selection
            OnTileSelected?.Invoke(tile);
        }
    }

    private void HandleTileDeselection()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
            currentInteractable = null;
        }

        ClearSelectionIndicator();

        Debug.Log("TileInteractionManager: Tile deselected.");

        // Notify listeners about the tile deselection
        OnTileDeselected?.Invoke();
    }

    private void UpdateSelectionIndicator(Vector3 position)
    {
        if (activeSelectionIndicator != null)
        {
            activeSelectionIndicator.transform.position = position;
            activeSelectionIndicator.SetActive(true);
        }
    }

    private void ClearSelectionIndicator()
    {
        if (activeSelectionIndicator != null)
        {
            activeSelectionIndicator.SetActive(false);
        }
    }
}
