// File: Scripts/Managers/HexMapRenderer.cs
using UnityEngine;

public class HexMapRenderer : MonoBehaviour
{
    public MapConfig MapConfiguration;
    public TileType[] TileTypes; // Array of possible TileTypes

    private float hexWidth;
    private float hexHeight;

    private HexGridDataManager gridDataManager;

    private void Awake()
    {
        gridDataManager = GetComponent<HexGridDataManager>();
        if (gridDataManager == null)
        {
            Debug.LogError("HexGridDataManager is required but not found!");
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameReady += GenerateMap;
    }

    private void OnDisable()
    {
        GameManager.OnGameReady -= GenerateMap;
    }

    public void GenerateMap()
    {
        if (MapConfiguration == null || TileTypes == null || TileTypes.Length == 0)
        {
            Debug.LogError("MapConfiguration or TileTypes are missing!");
            return;
        }

        CalculateHexSize();
        gridDataManager.InitializeGrid(MapConfiguration.MapWidth, MapConfiguration.MapHeight);

        for (int row = 0; row < MapConfiguration.MapHeight; row++)
        {
            for (int col = 0; col < MapConfiguration.MapWidth; col++)
            {
                Vector3 position = CalculateHexPosition(row, col);
                TileType assignedTileType = AssignTileType(row, col); // Assign a type based on position

                GameObject tilePrefab = Instantiate(assignedTileType.Prefab, position, Quaternion.identity, transform);

                Tile tile = tilePrefab.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.Initialize(new Vector2Int(col, row), hexWidth, hexHeight, assignedTileType);
                    gridDataManager.AddTile(tile, new Vector2Int(col, row));
                }
                else
                {
                    Debug.LogWarning($"Tile prefab at ({col}, {row}) is missing a Tile component.");
                }
            }
        }

        // Assign neighbors after all tiles are added
        gridDataManager.AssignNeighbors();

        Debug.Log("Map rendered and neighbors assigned!");
    }

    private TileType AssignTileType(int row, int col)
    {
        // Example logic for type assignment
        return TileTypes[(row + col) % TileTypes.Length];
    }

    private void CalculateHexSize()
    {
        if (TileTypes.Length > 0 && TileTypes[0].Prefab != null)
        {
            MeshRenderer renderer = TileTypes[0].Prefab.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                hexWidth = renderer.bounds.size.x;
                hexHeight = renderer.bounds.size.z;
            }
            else
            {
                Debug.LogError("Tile prefab is missing a MeshRenderer!");
            }
        }
        else
        {
            Debug.LogError("TileTypes array is empty or invalid!");
        }
    }

    private Vector3 CalculateHexPosition(int row, int col)
    {
        float xOffset = (row % 2 == 0) ? 0 : hexWidth * 0.5f;
        float x = col * hexWidth + xOffset;
        float z = row * (hexHeight * 0.75f);
        return new Vector3(x, 0, z);
    }
}