using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMapRenderer : MonoBehaviour
{
    public MapConfig MapConfiguration;

    public static event Action OnRenderingComplete;

    private void OnEnable()
    {
        HexGridDataManager.OnGridInitialized += RenderMap;
    }

    private void OnDisable()
    {
        HexGridDataManager.OnGridInitialized -= RenderMap;
    }

    private void RenderMap(Dictionary<Vector2, Tile> allTiles)
    {
        foreach (var entry in allTiles)
        {
            Tile tile = entry.Value;

            TileData tileType = tile.TileType;
            if (tileType == null)
            {
                Debug.LogError($"HexMapRenderer: Tile at {tile.GridPosition} is missing its TileType!");
                continue;
            }

            if (tileType.TileModel != null)
            {
                GameObject modelInstance = Instantiate(tileType.TileModel, tile.TileModel.transform);
                modelInstance.transform.localPosition = Vector3.zero;
            }

            if (tileType.FogOverlay != null)
            {
                GameObject fogInstance = Instantiate(tileType.FogOverlay, tile.FogOverlay.transform);
                fogInstance.transform.localPosition = Vector3.zero;
            }
        }

        Debug.Log("HexMapRenderer: Map rendering complete!");
        OnRenderingComplete?.Invoke();
    }
}
