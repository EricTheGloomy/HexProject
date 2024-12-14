// TileInteractionManager.cs - Updated to use IInteractable
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
    }

    private void HandleTileInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != null)
                {
                    // Deselect the previous tile
                    currentInteractable.Interact(); 
                }

                // Interact with the new tile
                interactable.Interact();
                currentInteractable = interactable;

                // Update selection indicator
                UpdateSelectionIndicator(hit.collider.transform.position);
            }
        }
    }

    private void UpdateSelectionIndicator(Vector3 position)
    {
        if (activeSelectionIndicator != null)
        {
            activeSelectionIndicator.transform.position = position;
            activeSelectionIndicator.SetActive(true);
        }
    }
}