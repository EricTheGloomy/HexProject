using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public SkillsManager skillsManager;
    private Dictionary<Vector2, Tile> mapTiles;
    [SerializeField]
    private GameplayState currentState;
    private Dictionary<GameplayState, IGameplayStateHandler> stateHandlers = new();

    private void Start()
    {
        if (stateHandlers.Count == 0)
        {
            InitializeStateHandlers();
        }
    }

    private void InitializeStateHandlers()
    {
        stateHandlers[GameplayState.PlayerTurn] = new PlayerTurnState(this);
        stateHandlers[GameplayState.EndTurn] = new EndTurnState(this);
        stateHandlers[GameplayState.MapUpdate] = new MapUpdateState(this);
        stateHandlers[GameplayState.TransitionToNextTurn] = new TransitionToNextTurnState(this);
    }

    public void SetState(GameplayState newState)
    {
        if (!stateHandlers.ContainsKey(newState))
        {
            Debug.LogError($"GameplayManager: No handler found for state {newState}. Transition aborted.");
            return;
        }

        if (stateHandlers.ContainsKey(currentState))
            stateHandlers[currentState].ExitState();

        var previousState = currentState;
        currentState = newState;

        Debug.Log($"Transitioning from {previousState} to {newState}");
        stateHandlers[newState].EnterState();
    }

    private void OnEnable()
    {
        if (stateHandlers.Count == 0)
        {
            InitializeStateHandlers();
        }

        GameInitializationFlowController.OnGameplayStart += InitializeGameplay;

        TileInteractionManager.OnTileSelected += HandleTileSelected;
        TileInteractionManager.OnTileDeselected += HandleTileDeselected;
    }

    private void OnDisable()
    {
        GameInitializationFlowController.OnGameplayStart -= InitializeGameplay;

        TileInteractionManager.OnTileSelected -= HandleTileSelected;
        TileInteractionManager.OnTileDeselected -= HandleTileDeselected;
    }

    private void InitializeGameplay(Dictionary<Vector2, Tile> tiles)
    {
        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogError("GameplayManager: Received invalid or empty map data.");
            return;
        }

        Debug.Log("GameplayManager: Map data received. Gameplay is starting...");
        mapTiles = tiles;

        Debug.Log($"Map contains {mapTiles.Count} tiles.");

        // Transition to the first gameplay state
        SetState(GameplayState.PlayerTurn);
    }

    private void HandleTileSelected(Tile tile)
    {
        //Debug.Log($"GameplayManager: Tile selected at {tile.transform.position}");
        Debug.Log($"GameplayManager: Tile selected at {tile.Attributes.GridPosition}");

        // Delegate to the current state handler
        if (stateHandlers.TryGetValue(currentState, out var stateHandler) && stateHandler is PlayerTurnState playerTurnState)
        {
            playerTurnState.OnTileSelected(tile);
        }
        else
        {
            Debug.LogWarning("GameplayManager: No handler found for the current state to handle tile selection.");
        }
    }

    private void HandleTileDeselected()
    {
        Debug.Log("GameplayManager: Tile deselected.");
    }
}
