using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMapRenderer : MonoBehaviour
{
    public MapConfig MapConfiguration;

    private float hexWidth;
    private float hexHeight;

    private void OnEnable()
    {
        HexGridDataManager.OnGridReady += RenderMap;
    }

    private void OnDisable()
    {
        HexGridDataManager.OnGridReady -= RenderMap;
    }

    private void RenderMap(Dictionary<Vector2Int, Vector3> positions, Dictionary<Vector2Int, TileType> tileTypes, float hexWidth, float hexHeight)
    {
        if (MapConfiguration == null)
        {
            Debug.LogError("MapConfiguration is missing!");
            return;
        }

    foreach (var entry in positions)
    {
        Vector2Int gridPosition = entry.Key;
        Vector3 worldPosition = entry.Value;

        Debug.Log($"Rendering tile at grid position {gridPosition}, world position {worldPosition}");
        
        if (!tileTypes.ContainsKey(gridPosition))
        {
            Debug.LogError($"No tile type found for position {gridPosition}!");
            continue;
        }

        TileType tileType = tileTypes[gridPosition];
        GameObject tilePrefab = tileType.Prefab;

        if (tilePrefab == null)
        {
            Debug.LogError($"TileType {tileType} is missing its prefab!");
            continue;
        }

        GameObject tileInstance = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
        Tile tile = tileInstance.GetComponent<Tile>();

        if (tile != null)
        {
            tile.Initialize(gridPosition, hexWidth, hexHeight, tileType);
        }
        else
        {
            Debug.LogWarning($"Tile prefab at {gridPosition} is missing a Tile component.");
        }
    }

        Debug.Log("HexMapRenderer: Map rendering complete!");
    }
}
