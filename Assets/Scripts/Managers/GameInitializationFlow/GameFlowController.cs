using System;
using System.Collections.Generic;
using UnityEngine;

public class GameInitializationFlowController : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.Initial;

    [Header("Dependencies")]
    [SerializeField] private ProceduralMapGenerator mapGenerator;
    [SerializeField] private HexGridDataManager gridManager;
    [SerializeField] private HexMapRenderer mapRenderer;
    [SerializeField] private MapLocationManager locationManager;
    [SerializeField] private FogOfWarManager fogOfWarManager;
    [SerializeField] private CameraManager cameraManager;

    public static event Action<Dictionary<Vector2, Tile>> OnGameplayStart;

    private Dictionary<Vector2, Tile> cachedHexCells;
    private readonly Dictionary<GameState, IGameStateHandler> stateHandlers = new();

    // Define valid state transitions
    private readonly Dictionary<GameState, List<GameState>> validTransitions = new()
    {
        { GameState.GameStart, new List<GameState> { GameState.GridInitialization } },
        { GameState.GridInitialization, new List<GameState> { GameState.MapGeneration } },
        { GameState.MapGeneration, new List<GameState> { GameState.LocationsAssigning } },
        { GameState.LocationsAssigning, new List<GameState> { GameState.MapRendering } },
        { GameState.MapRendering, new List<GameState> { GameState.FogOfWarInitialization } },
        { GameState.FogOfWarInitialization, new List<GameState> { GameState.CameraInitialization } },
        { GameState.CameraInitialization, new List<GameState> { GameState.Gameplay } },
        { GameState.Gameplay, new List<GameState>() } // No transitions allowed from Gameplay
    };

    private void Awake()
    {
        ValidateDependencies();
        InitializeStateHandlers();
        SubscribeToEvents();
        CurrentState = GameState.GameStart;
    }

    private void Start()
    {
        TransitionToState(GameState.GridInitialization);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void TransitionToState(GameState newState)
    {
        if (CurrentState == GameState.Gameplay)
        {
            Debug.LogWarning($"Cannot transition from terminal state Gameplay to {newState}. Transition ignored.");
            return;
        }

        if (!validTransitions.TryGetValue(CurrentState, out var allowedStates) || !allowedStates.Contains(newState))
        {
            Debug.LogError($"Invalid transition from {CurrentState} to {newState}. Called by: {new System.Diagnostics.StackTrace()}");
            return;
        }

        Debug.Log($"Transitioning from {CurrentState} to {newState}");
        CurrentState = newState;

        if (newState == GameState.Gameplay)
        {
            OnGameplayStart?.Invoke(cachedHexCells); // Notify subscribers
            enabled = false; // Disable the GameFlowController
            return;
        }

        if (stateHandlers.TryGetValue(newState, out var handler))
        {
            handler.EnterState(cachedHexCells);
        }
        else
        {
            Debug.LogError($"No handler found for state {newState}");
        }
    }

    private void InitializeStateHandlers()
    {
        stateHandlers[GameState.GridInitialization] = new GridInitializationHandler(gridManager, TransitionToState);
        stateHandlers[GameState.MapGeneration] = new MapGenerationHandler(mapGenerator, TransitionToState);
        stateHandlers[GameState.LocationsAssigning] = new LocationsAssigningHandler(locationManager, TransitionToState);
        stateHandlers[GameState.MapRendering] = new MapRenderingHandler(mapRenderer, TransitionToState);
        stateHandlers[GameState.FogOfWarInitialization] = new FogOfWarInitializationHandler(fogOfWarManager, TransitionToState);
        stateHandlers[GameState.CameraInitialization] = new CameraInitializationHandler(cameraManager, gridManager, TransitionToState);
        stateHandlers[GameState.Gameplay] = new GameplayHandler();
    }

    private void ValidateDependencies()
    {
        var missingDependencies = new List<string>();

        if (!mapGenerator) missingDependencies.Add("MapGenerator");
        if (!gridManager) missingDependencies.Add("GridManager");
        if (!mapRenderer) missingDependencies.Add("MapRenderer");
        if (!locationManager) missingDependencies.Add("LocationManager");
        if (!fogOfWarManager) missingDependencies.Add("FogOfWarManager");
        if (!cameraManager) missingDependencies.Add("CameraManager");

        if (missingDependencies.Count > 0)
        {
            Debug.LogError($"Missing dependencies: {string.Join(", ", missingDependencies)}");
            enabled = false;
        }
    }

    private void SubscribeToEvents()
    {
        gridManager.OnGridReady += OnGridReady;
        mapGenerator.OnMapGenerated += OnMapGenerated;
    }

    private void UnsubscribeFromEvents()
    {
        gridManager.OnGridReady -= OnGridReady;
        mapGenerator.OnMapGenerated -= OnMapGenerated;
    }

    private void OnGridReady(Dictionary<Vector2, Tile> hexCells)
    {
        cachedHexCells = hexCells;

        if (CurrentState != GameState.Gameplay)
        {
            TransitionToState(GameState.MapGeneration);
        }
    }

    private void OnMapGenerated(Dictionary<Vector2Int, TileTypeData> mapData)
    {
        if (CurrentState != GameState.Gameplay)
        {
            TransitionToState(GameState.LocationsAssigning);
        }
    }
}
