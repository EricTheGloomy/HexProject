using System;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.Initial;

    [Header("Dependencies")]
    [SerializeField] private ProceduralMapGenerator mapGenerator;
    [SerializeField] private HexGridDataManager gridManager;
    [SerializeField] private HexMapRenderer mapRenderer;
    [SerializeField] private MapLocationManager locationManager;
    [SerializeField] private FogOfWarManager fogOfWarManager;
    [SerializeField] private CameraManager cameraManager;

    private IMapGenerator MapGenerator;
    private IGridManager GridManager;
    private IRenderer MapRenderer;
    private IMapLocationManager LocationManager;
    private IFogOfWarManager FogOfWarManager;
    private ICameraManager CameraManager;

    private Dictionary<Vector2, Tile> cachedHexCells;

    // Execution flags for state handlers
    private bool isGameStarted = false;
    private bool isMapGenerated = false;
    private bool isGridInitialized = false;
    private bool isMapRendered = false;

    private void Awake()
    {
        if (!mapGenerator || !gridManager || !mapRenderer || !locationManager || !fogOfWarManager || !cameraManager)
        {
            Debug.LogError("GameFlowController: Missing dependencies in the Inspector!");
            enabled = false;
            return;
        }

        MapGenerator = mapGenerator;
        GridManager = gridManager;
        MapRenderer = mapRenderer;
        LocationManager = locationManager;
        FogOfWarManager = fogOfWarManager;
        CameraManager = cameraManager;
    }

    private void Start()
    {
        TransitionToState(GameState.GameStart); // Transition from Initial to GameStart
    }

    private void TransitionToState(GameState newState, bool forceReExecution = false)
    {
        if (CurrentState == newState && !forceReExecution)
        {
            Debug.LogWarning($"GameFlowController: Already in state {newState}, skipping execution.");
            return;
        }

        Debug.Log($"Transitioning from {CurrentState} to {newState}");
        CurrentState = newState;

        // Execute the state handler
        switch (newState)
        {
            case GameState.Initial:
                Debug.Log("GameFlowController: Initial state. Waiting to transition...");
                break;
            case GameState.GameStart:
                StartGame();
                break;
            case GameState.MapGeneration:
                GenerateMap();
                break;
            case GameState.GridInitialization:
                InitializeGrid();
                break;
            case GameState.LocationsAssigning:
                AssignLocations();
                break;
            case GameState.MapRendering:
                RenderMap();
                break;
            case GameState.FogOfWarInitialization:
                InitializeFogOfWar();
                break;
            case GameState.CameraInitialization:
                InitializeCamera();
                break;
            case GameState.Gameplay:
                StartGameplay();
                break;
            default:
                Debug.LogError($"GameFlowController: Unhandled state {newState}.");
                break;
        }
    }

    private void StartGame()
    {
        if (isGameStarted)
        {
            Debug.LogWarning("GameFlowController: GameStart already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Starting game...");
        isGameStarted = true;
        TransitionToState(GameState.MapGeneration);
    }

    private void GenerateMap()
    {
        if (isMapGenerated)
        {
            Debug.LogWarning("GameFlowController: MapGeneration already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Generating map...");
        MapGenerator.OnMapGenerated -= OnMapGenerated;
        MapGenerator.OnMapGenerated += OnMapGenerated;
        MapGenerator.GenerateMap();
    }

    private void OnMapGenerated(Dictionary<Vector2Int, TileTypeData> mapData)
    {
        if (mapData == null || mapData.Count == 0)
        {
            Debug.LogError("GameFlowController: Map generation failed.");
            return;
        }

        Debug.Log("GameFlowController: Map generation complete!");
        isMapGenerated = true;
        TransitionToState(GameState.GridInitialization);
    }

    private void InitializeGrid()
    {
        if (isGridInitialized)
        {
            Debug.LogWarning("GameFlowController: GridInitialization already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Initializing grid...");
        GridManager.OnGridReady -= OnGridReady;
        GridManager.OnGridReady += OnGridReady;
        GridManager.InitializeGrid(MapGenerator.GeneratedMapData);
    }

    private void OnGridReady(Dictionary<Vector2, Tile> hexCells)
    {
        if (hexCells == null || hexCells.Count == 0)
        {
            Debug.LogError("GameFlowController: Grid initialization failed.");
            return;
        }

        Debug.Log($"GameFlowController: Grid initialized with {hexCells.Count} tiles.");
        cachedHexCells = hexCells;
        isGridInitialized = true;
        TransitionToState(GameState.LocationsAssigning);
    }

    private void AssignLocations()
    {
        Debug.Log("GameFlowController: Assigning map locations...");

        // Provide grid data explicitly
        LocationManager.AssignLocations(cachedHexCells);

        TransitionToState(GameState.MapRendering);
    }

    private void RenderMap()
    {
        if (isMapRendered)
        {
            Debug.LogWarning("GameFlowController: MapRendering already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Rendering map...");
        MapRenderer.RenderMap(cachedHexCells);
        isMapRendered = true;
        TransitionToState(GameState.FogOfWarInitialization);
    }

    private void InitializeFogOfWar()
    {
        Debug.Log("GameFlowController: Initializing fog of war...");
        FogOfWarManager.Initialize(cachedHexCells);
        TransitionToState(GameState.CameraInitialization);
    }

    private void InitializeCamera()
    {
        Debug.Log("GameFlowController: Initializing camera...");

        // Retrieve tile size from HexGridDataManager
        float tileSizeX = gridManager.GetTileWidth();
        float tileSizeZ = gridManager.GetTileHeight();

        // Pass the necessary data to CameraManager
        CameraManager.Initialize(cachedHexCells, tileSizeX, tileSizeZ);

        TransitionToState(GameState.Gameplay); // Proceed to gameplay
    }

    private void StartGameplay()
    {
        Debug.Log("GameFlowController: Gameplay has started!");
    }

    public void ForceStateReEntry(GameState stateToReEnter)
    {
        Debug.Log($"GameFlowController: Forcing re-entry into state {stateToReEnter}.");
        ResetExecutionFlags(); // Reset execution flags for all states

        TransitionToState(stateToReEnter, forceReExecution: true); // Enforce re-execution of the handler
    }

    private void ResetExecutionFlags()
    {
        isGameStarted = false;
        isMapGenerated = false;
        isGridInitialized = false;
        isMapRendered = false;
    }
}
