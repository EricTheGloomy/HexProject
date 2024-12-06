// File: Scripts/Managers/HexMapRenderer.cs
using UnityEngine;

public class HexMapRenderer : MonoBehaviour
{
    public MapConfig MapConfiguration;

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
        if (MapConfiguration == null || MapConfiguration.HexTilePrefab == null)
        {
            Debug.LogError("MapConfiguration or HexTilePrefab is missing!");
            return;
        }

        CalculateHexSize();
        gridDataManager.InitializeGrid(MapConfiguration.MapWidth, MapConfiguration.MapHeight);

        for (int row = 0; row < MapConfiguration.MapHeight; row++)
        {
            for (int col = 0; col < MapConfiguration.MapWidth; col++)
            {
                Vector3 position = CalculateHexPosition(row, col);
                GameObject hexTile = Instantiate(MapConfiguration.HexTilePrefab, position, Quaternion.identity, transform);

                Tile tile = hexTile.GetComponent<Tile>();
                if (tile != null)
                {
                    // Initialize the tile with its coordinates
                    tile.Initialize(new Vector2Int(col, row), hexWidth, hexHeight);

                    gridDataManager.AddTile(tile, new Vector2Int(col, row));
                }
                else
                {
                    Debug.LogWarning($"Hex tile at ({col}, {row}) is missing a Tile component.");
                }
            }
        }

        // Assign neighbors after all tiles are added
        gridDataManager.AssignNeighbors();

        Debug.Log("Map rendered and neighbors assigned!");
    }

    private void CalculateHexSize()
    {
        MeshRenderer renderer = MapConfiguration.HexTilePrefab.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            hexWidth = renderer.bounds.size.x;
            hexHeight = renderer.bounds.size.z;
        }
        else
        {
            Debug.LogError("HexTilePrefab is missing a MeshRenderer!");
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
