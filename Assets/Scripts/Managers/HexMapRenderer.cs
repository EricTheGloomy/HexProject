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

    private void RenderMap()
    {
        HexGridDataManager gridDataManager = FindObjectOfType<HexGridDataManager>();
        if (gridDataManager == null)
        {
            Debug.LogError("HexMapRenderer: HexGridDataManager is missing in the scene!");
            return;
        }

        // Fetch all tiles from HexGridDataManager
        var allTiles = gridDataManager.GetHexCells();

        foreach (var entry in allTiles)
        {
            Tile tile = entry.Value;

            // Fetch the TileType
            TileData tileType = tile.TileType;
            if (tileType == null)
            {
                Debug.LogError($"HexMapRenderer: Tile at {tile.GridPosition} is missing its TileType!");
                continue;
            }

            // Instantiate the TileModel and attach it
            if (tileType.TileModel != null)
            {
                GameObject modelInstance = Instantiate(tileType.TileModel, tile.TileModel.transform);
                modelInstance.transform.localPosition = Vector3.zero;
            }

            // Instantiate the FogOverlay and attach it
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
