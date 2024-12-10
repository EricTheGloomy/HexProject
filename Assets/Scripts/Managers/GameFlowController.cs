using System;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowController : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private ProceduralMapGenerator mapGenerator; // Concrete class for IMapGenerator
    [SerializeField] private HexGridDataManager gridManager;      // Concrete class for IGridManager
    [SerializeField] private HexMapRenderer mapRenderer;         // Concrete class for IRenderer

    private IMapGenerator MapGenerator;
    private IGridManager GridManager;
    private IRenderer MapRenderer;

    private Dictionary<Vector2, Tile> cachedHexCells;

    // Execution flags for state handlers
    private bool isGameStarted = false;
    private bool isMapGenerated = false;
    private bool isGridInitialized = false;
    private bool isMapRendered = false;

    private void Awake()
    {
        // Safeguard against missing dependencies
        if (!mapGenerator || !gridManager || !mapRenderer)
        {
            Debug.LogError("GameFlowController: Missing dependencies in the Inspector!");
            enabled = false; // Disable script if dependencies are missing
            return;
        }

        // Cast components to interfaces
        MapGenerator = mapGenerator;
        GridManager = gridManager;
        MapRenderer = mapRenderer;
    }

    private void Start()
    {
        TransitionToState(GameState.GameStart);
    }

    private void TransitionToState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning($"GameFlowController: Already in state {newState}, executing handler again.");
        }
        else
        {
            Debug.Log($"Transitioning to new state: {newState}");
            CurrentState = newState;
        }

        // Execute the state handler
        switch (newState)
        {
            case GameState.GameStart:
                StartGame();
                break;
            case GameState.MapGeneration:
                GenerateMap();
                break;
            case GameState.GridInitialization:
                InitializeGrid();
                break;
            case GameState.MapRendering:
                RenderMap();
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
        MapGenerator.OnMapGenerated -= OnMapGenerated; // Prevent duplicate subscriptions
        MapGenerator.OnMapGenerated += OnMapGenerated;
        MapGenerator.GenerateMap();
    }

    private void OnMapGenerated(Dictionary<Vector2Int, TileData> mapData)
    {
        try
        {
            if (mapData == null || mapData.Count == 0)
            {
                Debug.LogError("GameFlowController: Map generation failed or returned empty data.");
                return;
            }

            Debug.Log("GameFlowController: Map generation complete!");
            isMapGenerated = true;
            TransitionToState(GameState.GridInitialization);
        }
        finally
        {
            MapGenerator.OnMapGenerated -= OnMapGenerated;
        }
    }

    private void InitializeGrid()
    {
        if (isGridInitialized)
        {
            Debug.LogWarning("GameFlowController: GridInitialization already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Initializing grid...");

        if (MapGenerator.GeneratedMapData == null || MapGenerator.GeneratedMapData.Count == 0)
        {
            Debug.LogError("GameFlowController: Cannot initialize grid. Generated map data is null or empty.");
            return;
        }

        GridManager.OnGridReady -= OnGridReady; // Prevent duplicate subscriptions
        GridManager.OnGridReady += OnGridReady;
        GridManager.InitializeGrid(MapGenerator.GeneratedMapData);
    }

    private void OnGridReady(Dictionary<Vector2, Tile> hexCells)
    {
        try
        {
            if (hexCells == null || hexCells.Count == 0)
            {
                Debug.LogError("GameFlowController: Grid initialization failed or returned no tiles.");
                return;
            }

            Debug.Log($"GameFlowController: Grid initialization complete with {hexCells.Count} tiles.");
            cachedHexCells = hexCells;
            isGridInitialized = true;
            TransitionToState(GameState.MapRendering);
        }
        finally
        {
            GridManager.OnGridReady -= OnGridReady;
        }
    }

    private void RenderMap()
    {
        if (isMapRendered)
        {
            Debug.LogWarning("GameFlowController: MapRendering already executed, skipping.");
            return;
        }

        Debug.Log("GameFlowController: Rendering map...");

        if (cachedHexCells == null || cachedHexCells.Count == 0)
        {
            Debug.LogError("GameFlowController: Cannot render map. Grid data is null or empty.");
            return;
        }

        HexMapRenderer.OnRenderingComplete -= OnRenderingComplete; // Prevent duplicate subscriptions
        HexMapRenderer.OnRenderingComplete += OnRenderingComplete;
        MapRenderer.RenderMap(cachedHexCells);
    }

    private void OnRenderingComplete()
    {
        try
        {
            Debug.Log("GameFlowController: Map rendering complete!");
            isMapRendered = true;
            TransitionToState(GameState.Gameplay);
        }
        finally
        {
            HexMapRenderer.OnRenderingComplete -= OnRenderingComplete;
        }
    }

    private void StartGameplay()
    {
        Debug.Log("GameFlowController: Gameplay has started!");

        if (MapRenderer == null || GridManager == null)
        {
            Debug.LogError("GameFlowController: Cannot start gameplay. Dependencies are missing!");
            return;
        }

        Debug.Log("GameFlowController: Entering gameplay...");
    }

    public void ForceStateReEntry(GameState stateToReEnter)
    {
        Debug.Log($"GameFlowController: Forcing re-entry into state {stateToReEnter}.");
        ResetExecutionFlags(); // Reset execution flags for all states

        switch (stateToReEnter)
        {
            case GameState.GameStart:
                StartGame();
                break;
            case GameState.MapGeneration:
                GenerateMap();
                break;
            case GameState.GridInitialization:
                InitializeGrid();
                break;
            case GameState.MapRendering:
                RenderMap();
                break;
            case GameState.Gameplay:
                StartGameplay();
                break;
        }
    }

    private void ResetExecutionFlags()
    {
        isGameStarted = false;
        isMapGenerated = false;
        isGridInitialized = false;
        isMapRendered = false;
    }
}
