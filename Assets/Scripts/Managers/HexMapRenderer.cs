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

            TileTypeData tileTypeData = tile.Attributes.TileTypeData;
            if (tileTypeData == null)
            {
                Debug.LogError($"HexMapRenderer: Tile at {tile.Attributes.GridPosition} is missing its TileTypeData!");
                continue;
            }

            // Instantiate model from TileTypeData
            if (tileTypeData.TileModel != null)
            {
                GameObject modelInstance = Instantiate(tileTypeData.TileModel, tile.TileModel.transform);
                modelInstance.transform.localPosition = Vector3.zero;

                // Apply both base and overlay materials
                ApplyTileMaterials(modelInstance, tileTypeData.BaseMaterial, tileTypeData.OverlayMaterial);
            }

            // Fog overlay remains unchanged
            if (tileTypeData.FogOverlay != null)
            {
                GameObject fogInstance = Instantiate(tileTypeData.FogOverlay, tile.FogOverlay.transform);
                fogInstance.transform.localPosition = Vector3.zero;
            }
        }

        Debug.Log("HexMapRenderer: Map rendering complete!");
        OnRenderingComplete?.Invoke();
    }

    private void ApplyTileMaterials(GameObject tileModel, Material baseMaterial, Material overlayMaterial)
    {
        if (tileModel == null) return;

        Renderer renderer = tileModel.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material[] materials = renderer.materials;

            if (materials.Length == 1)
            {
                // Apply only the base material if the model has one slot
                materials[0] = baseMaterial;
            }
            else if (materials.Length >= 2 && overlayMaterial != null)
            {
                // Apply both base and overlay materials if there are at least two slots
                materials[0] = baseMaterial;   // Base material
                materials[1] = overlayMaterial; // Overlay material
            }

            renderer.materials = materials;
        }
        else
        {
            Debug.LogWarning("HexMapRenderer: No Renderer found on the TileModel.");
        }
    }

}
