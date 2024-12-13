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
        TransitionToState(GameState.GridInitialization);
    }

    private void TransitionToState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.LogWarning($"Already in state {newState}, skipping transition.");
            return;
        }

        Debug.Log($"Transitioning from {CurrentState} to {newState}");
        CurrentState = newState;

        switch (newState)
        {
            case GameState.GridInitialization:
                InitializeGrid();
                break;

            case GameState.MapGeneration:
                GenerateMap();
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

    private void InitializeGrid()
    {
        Debug.Log("Initializing grid...");
        gridManager.InitializeGrid(); // No map data needed
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
        TransitionToState(GameState.MapGeneration);
    }

    private void GenerateMap()
    {
        Debug.Log("Generating map...");
        mapGenerator.ApplyTileTypeData(cachedHexCells); // Pass the grid to ProceduralMapGenerator
    }

    private void OnMapGenerated(Dictionary<Vector2Int, TileTypeData> mapData)
    {
        if (mapData == null || mapData.Count == 0)
        {
            Debug.LogError("Map generation failed.");
            return;
        }

        Debug.Log("Map generation complete!");
        TransitionToState(GameState.LocationsAssigning);
    }

    private void AssignLocations()
    {
        Debug.Log("Assigning map locations...");
        locationManager.AssignLocations(cachedHexCells);
        TransitionToState(GameState.MapRendering);
    }

    private void RenderMap()
    {
        Debug.Log("Rendering map...");
        mapRenderer.RenderMap(cachedHexCells);
        TransitionToState(GameState.FogOfWarInitialization);
    }

    private void InitializeFogOfWar()
    {
        Debug.Log("Initializing fog of war...");
        fogOfWarManager.Initialize(cachedHexCells);
        TransitionToState(GameState.CameraInitialization);
    }

    private void InitializeCamera()
    {
        Debug.Log("Initializing camera...");

        float tileSizeX = gridManager.GetTileWidth();
        float tileSizeZ = gridManager.GetTileHeight();

        cameraManager.Initialize(cachedHexCells, tileSizeX, tileSizeZ);
        TransitionToState(GameState.Gameplay);
    }

    private void StartGameplay()
    {
        Debug.Log("Gameplay has started!");
    }

    private void SubscribeToEvents()
    {
        gridManager.OnGridReady += OnGridReady;
        mapGenerator.OnMapGenerated += OnMapGenerated;
    }

    private void UnsubscribeFromEvents()
    {
        if (gridManager != null) gridManager.OnGridReady -= OnGridReady;
        if (mapGenerator != null) mapGenerator.OnMapGenerated -= OnMapGenerated;
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
