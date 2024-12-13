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

    private IMapGenerator MapGenerator => mapGenerator;
    private IGridManager GridManager => gridManager;
    private IRenderer MapRenderer => mapRenderer;
    private IMapLocationManager LocationManager => locationManager;
    private IFogOfWarManager FogOfWarManager => fogOfWarManager;
    private ICameraManager CameraManager => cameraManager;

    private Dictionary<Vector2, Tile> cachedHexCells;

    private void Awake()
    {
        ValidateDependencies();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Start()
    {
        TransitionToState(GameState.GameStart);
    }

    private void TransitionToState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning($"GameFlowController: Already in state {newState}, skipping redundant transition.");
            return;
        }

        Debug.Log($"Transitioning from {CurrentState} to {newState}");
        CurrentState = newState;

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
                Debug.LogError($"Unhandled state {newState}");
                break;
        }
    }

    private void StartGame()
    {
        Debug.Log("Starting game...");
        TransitionToState(GameState.MapGeneration); // Transition to the next state
    }

    private void GenerateMap()
    {
        Debug.Log("Generating map...");
        MapGenerator.GenerateMap();
        // Transition happens in OnMapGenerated
    }

    private void OnMapGenerated(Dictionary<Vector2Int, TileTypeData> mapData)
    {
        if (mapData == null || mapData.Count == 0)
        {
            Debug.LogError("Map generation failed.");
            return;
        }

        Debug.Log("Map generation complete!");
        TransitionToState(GameState.GridInitialization); // Transition to the next state
    }

    private void InitializeGrid()
    {
        Debug.Log("Initializing grid...");
        GridManager.InitializeGrid(MapGenerator.GeneratedMapData);
        // Transition happens in OnGridReady
    }

    private void OnGridReady(Dictionary<Vector2, Tile> hexCells)
    {
        if (hexCells == null || hexCells.Count == 0)
        {
            Debug.LogError("Grid initialization failed.");
            return;
        }

        Debug.Log($"Grid initialized with {hexCells.Count} tiles.");
        cachedHexCells = hexCells;
        TransitionToState(GameState.LocationsAssigning); // Transition to the next state
    }

    private void AssignLocations()
    {
        Debug.Log("Assigning map locations...");
        LocationManager.AssignLocations(cachedHexCells);
        TransitionToState(GameState.MapRendering); // Transition to the next state
    }

    private void RenderMap()
    {
        Debug.Log("Rendering map...");
        MapRenderer.RenderMap(cachedHexCells);
        TransitionToState(GameState.FogOfWarInitialization); // Transition to the next state
    }

    private void InitializeFogOfWar()
    {
        Debug.Log("Initializing fog of war...");
        FogOfWarManager.Initialize(cachedHexCells);
        TransitionToState(GameState.CameraInitialization); // Transition to the next state
    }

    private void InitializeCamera()
    {
        Debug.Log("Initializing camera...");

        float tileSizeX = GridManager.GetTileWidth();
        float tileSizeZ = GridManager.GetTileHeight();

        CameraManager.Initialize(cachedHexCells, tileSizeX, tileSizeZ);
        TransitionToState(GameState.Gameplay); // Transition to the next state
    }

    private void StartGameplay()
    {
        Debug.Log("Gameplay has started!");
    }

    private void SubscribeToEvents()
    {
        MapGenerator.OnMapGenerated += OnMapGenerated;
        GridManager.OnGridReady += OnGridReady;
    }

    private void UnsubscribeFromEvents()
    {
        if (MapGenerator != null) MapGenerator.OnMapGenerated -= OnMapGenerated;
        if (GridManager != null) GridManager.OnGridReady -= OnGridReady;
    }

    private void ValidateDependencies()
    {
        bool hasError = false;

        if (!mapGenerator) { Debug.LogError("Missing 'MapGenerator'!"); hasError = true; }
        if (!gridManager) { Debug.LogError("Missing 'GridManager'!"); hasError = true; }
        if (!mapRenderer) { Debug.LogError("Missing 'MapRenderer'!"); hasError = true; }
        if (!locationManager) { Debug.LogError("Missing 'LocationManager'!"); hasError = true; }
        if (!fogOfWarManager) { Debug.LogError("Missing 'FogOfWarManager'!"); hasError = true; }
        if (!cameraManager) { Debug.LogError("Missing 'CameraManager'!"); hasError = true; }

        if (hasError) enabled = false;
    }
}
