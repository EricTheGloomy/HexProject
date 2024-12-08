using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMapRenderer : MonoBehaviour
{
    public MapConfig MapConfiguration;

    private float hexWidth;
    private float hexHeight;

    public static event Action OnRenderingComplete;

    private void OnEnable()
    {
        ProceduralMapGenerator.OnMapGenerated += RenderMap;
    }

    private void OnDisable()
    {
        ProceduralMapGenerator.OnMapGenerated -= RenderMap;
    }

    private void RenderMap(Dictionary<Vector2Int, TileType> mapData)
    {
        if (MapConfiguration == null || MapConfiguration.HexTilePrefabDefault == null)
        {
            Debug.LogError("MapConfiguration or HexTilePrefab is missing!");
            return;
        }

        CalculateHexSize();

        HexGridDataManager gridDataManager = FindObjectOfType<HexGridDataManager>();
        if (gridDataManager == null)
        {
            Debug.LogError("HexGridDataManager is missing in the scene!");
            return;
        }

        gridDataManager.InitializeGrid(MapConfiguration.MapWidth, MapConfiguration.MapHeight);

        foreach (var entry in mapData)
        {
            Vector2Int gridPosition = entry.Key;
            TileType tileType = entry.Value;

            Vector3 worldPosition = CalculateHexPosition(gridPosition.y, gridPosition.x);
            GameObject tilePrefab = Instantiate(tileType.Prefab, worldPosition, Quaternion.identity, transform);

            Tile tile = tilePrefab.GetComponent<Tile>();
            if (tile != null)
            {
                tile.Initialize(gridPosition, hexWidth, hexHeight, tileType);
                gridDataManager.AddTile(tile, gridPosition);
            }
            else
            {
                Debug.LogWarning($"Tile prefab at ({gridPosition.x}, {gridPosition.y}) is missing a Tile component.");
            }
        }

        Debug.Log("Map rendering complete!");
        OnRenderingComplete?.Invoke();
    }

    private void CalculateHexSize()
    {
        MeshRenderer renderer = MapConfiguration.HexTilePrefabDefault.GetComponentInChildren<MeshRenderer>();
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
