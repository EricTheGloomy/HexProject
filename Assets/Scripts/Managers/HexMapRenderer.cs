using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMapRenderer : MonoBehaviour, IRenderer
{
    public static event Action OnRenderingComplete;

    public void RenderMap(Dictionary<Vector2, Tile> allTiles)
    {
        Debug.Log("HexMapRenderer: RenderMap invoked.");

        foreach (var entry in allTiles)
        {
            Tile tile = entry.Value;

            TileTypeData tileTypeData = tile.TileTypeData;
            if (tileTypeData == null)
            {
                Debug.LogError($"HexMapRenderer: Tile at {tile.GridPosition} is missing its TileTypeData!");
                continue;
            }

            if (tileTypeData.TileModel != null)
            {
                GameObject modelInstance = Instantiate(tileTypeData.TileModel, tile.TileModel.transform);
                modelInstance.transform.localPosition = Vector3.zero;
            }

            if (tileTypeData.FogOverlay != null)
            {
                GameObject fogInstance = Instantiate(tileTypeData.FogOverlay, tile.FogOverlay.transform);
                fogInstance.transform.localPosition = Vector3.zero;
            }
        }

        Debug.Log("HexMapRenderer: Map rendering complete!");
        OnRenderingComplete?.Invoke();
    }
}
