using UnityEngine;

public class TileInteractionManager : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject selectionIndicatorPrefab;

    private GameObject activeSelectionIndicator;
    private IInteractable currentInteractable;

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

            // Ensure tile is valid
            if (tile == null || interactable == null)
            {
                Debug.LogWarning("TileInteractionManager: No valid tile selected.");
                ClearSelectionIndicator();
                return;
            }

            // Ensure the tile is not under fog of war
            if (tile.Attributes.Visibility == VisibilityState.Hidden)
            {
                Debug.Log("TileInteractionManager: Cannot select a tile under fog of war.");
                ClearSelectionIndicator();
                return;
            }

            // Handle interaction
            if (currentInteractable != null)
            {
                currentInteractable.Interact(); // Deselect previous tile
            }

            interactable.Interact(); // Select new tile
            currentInteractable = interactable;

            // Update selection indicator
            UpdateSelectionIndicator(hit.collider.transform.position);
        }
        else
        {
            // Clear the selection indicator if no valid object is clicked
            ClearSelectionIndicator();
        }
    }

    private void HandleTileDeselection()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(); // Deselect the current tile
            currentInteractable = null;
        }

        ClearSelectionIndicator();

        Debug.Log("TileInteractionManager: Tile deselected.");
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
