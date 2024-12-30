using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnState : IGameplayStateHandler
{
    private GameplayManager manager;

    public PlayerTurnState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public void EnterState()
    {
        Debug.Log("Player turn started. Waiting for player actions...");
        manager.skillsManager.enabled = true;
    }

    public void ExitState()
    {
        Debug.Log("Player turn ended.");
        manager.skillsManager.enabled = false;
    }

    public void EndTurn()
    {
        Debug.Log("Ending player turn...");
        manager.SetState(GameplayState.EndTurn);
    }

    public void OnTileSelected(Tile tile)
    {
        if (tile == null)
        {
            Debug.LogWarning("PlayerTurnState: No tile selected.");
            return;
        }

        Debug.Log($"PlayerTurnState: Player selected tile at {tile.transform.position}");

        // Interact with the tile using the IInteractable interface
        tile.Interact();

        // Add any additional logic for processing the selected tile
        Debug.Log("PlayerTurnState: Interaction with the tile completed.");
    }
}
