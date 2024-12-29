using System;
using System.Collections.Generic;
using UnityEngine;

public class HexMapRenderer : MonoBehaviour, IRenderer
{
    [Header("Dependencies")]
    [SerializeField] private RiverModelConfig riverModelConfig;

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

            // Render rivers on tiles with river connections
            if (tile.Attributes.Gameplay.HasRiver)
            {
                RenderRiver(tile);
            }

            // Instantiate model from TileTypeData
            if (tileTypeData.TileModel != null && !tile.Attributes.Gameplay.HasRiver)
            {
                GameObject modelInstance = Instantiate(tileTypeData.TileModel, tile.TileModel.transform);
                modelInstance.transform.localPosition = Vector3.zero;

                // Apply materials
                ApplyTileMaterials(modelInstance, tileTypeData.BaseMaterial, tileTypeData.OverlayMaterial);
            }

            if (tile.Attributes.Gameplay.IsStartingLocation)
            {
                if (tileTypeData.StartingLocationDecoration != null)
                {
                    GameObject startingLocationInstance = Instantiate(tileTypeData.StartingLocationDecoration, tile.TileDecorations.transform);
                    startingLocationInstance.transform.localPosition = Vector3.zero;
                    startingLocationInstance.transform.localRotation = Quaternion.identity;
                }
            }

            // Instantiate housing decorations if applicable
            if (tile.Attributes.Gameplay.HasHousing && !tile.Attributes.Gameplay.IsStartingLocation)
            {
                GameObject decorationPrefab = GetHousingDecoration(tile.Attributes.Gameplay.SettlementType, tileTypeData);

                if (decorationPrefab != null)
                {
                    GameObject decorationInstance = Instantiate(decorationPrefab, tile.TileDecorations.transform);
                    decorationInstance.transform.localPosition = Vector3.zero;
                    decorationInstance.transform.localRotation = Quaternion.identity;
                }
            }

            if (tile.Attributes.Gameplay.HasVegetation && tileTypeData.VegetationDecoration != null)
            {
                GameObject vegetationInstance = Instantiate(tileTypeData.VegetationDecoration, tile.TileDecorations.transform);
                vegetationInstance.transform.localPosition = Vector3.zero;
                vegetationInstance.transform.localRotation = Quaternion.identity;
            }

            if (tile.Attributes.Gameplay.MountainType == MountainType.LowMountain && tileTypeData.LowMountainDecoration != null)
            {
                GameObject decorationInstance = Instantiate(tileTypeData.LowMountainDecoration, tile.TileDecorations.transform);
                decorationInstance.transform.localPosition = Vector3.zero;
                decorationInstance.transform.localRotation = Quaternion.identity;
            }
            else if (tile.Attributes.Gameplay.MountainType == MountainType.HighMountain && tileTypeData.HighMountainDecoration != null)
            {
                GameObject decorationInstance = Instantiate(tileTypeData.HighMountainDecoration, tile.TileDecorations.transform);
                decorationInstance.transform.localPosition = Vector3.zero;
                decorationInstance.transform.localRotation = Quaternion.identity;
            }

            // Instantiate fog overlay if applicable
            if (tileTypeData.FogOverlay != null)
            {
                GameObject fogInstance = Instantiate(tileTypeData.FogOverlay, tile.FogOverlay.transform);
                fogInstance.transform.localPosition = Vector3.zero;
            }
        }

        Debug.Log("HexMapRenderer: Map rendering complete!");
        OnRenderingComplete?.Invoke();
    }

    private GameObject GetHousingDecoration(SettlementType settlementType, TileTypeData tileTypeData)
    {
        return settlementType switch
        {
            SettlementType.City => tileTypeData.CityHousingDecoration,
            SettlementType.Town => tileTypeData.TownHousingDecoration,
            SettlementType.Village => tileTypeData.VillageHousingDecoration,
            SettlementType.Hamlet => tileTypeData.HamletHousingDecoration,
            _ => null,
        };
    }

    private void RenderRiver(Tile tile)
    {
        // Get river connections for this tile
        bool[] riverConnections = tile.Attributes.Gameplay.RiverConnections;

        // Fetch the correct river prefab and rotation steps
        int rotationSteps;
        GameObject riverPrefab = riverModelConfig.GetRiverModel(riverConnections, out rotationSteps);

        if (riverPrefab != null)
        {
            // Instantiate the river prefab
            GameObject riverInstance = Instantiate(riverPrefab, tile.TileModel.transform);
            riverInstance.transform.localPosition = Vector3.zero;

            // Apply rotation (each step is 60 degrees clockwise)
            float rotationAngle = rotationSteps * 60f;
            riverInstance.transform.localRotation = Quaternion.Euler(0, rotationAngle, 0);

            // Fetch the tile's terrain materials from TileTypeData
            TileTypeData tileTypeData = tile.Attributes.TileTypeData;
            if (tileTypeData != null)
            {
//                Debug.Log($"RenderRiver: Applying base material from TileTypeData for tile at {tile.Attributes.GridPosition}");
                ApplyRiverMaterials(riverInstance, tileTypeData.BaseMaterial);
            }
            else
            {
                Debug.LogWarning($"RenderRiver: TileTypeData is null for tile at {tile.Attributes.GridPosition}");
            }

//            Debug.Log($"Rendered river at {tile.Attributes.GridPosition} with {rotationSteps} rotation steps.");
        }
        else
        {
            Debug.LogWarning($"No river model found for tile at {tile.Attributes.GridPosition} with connections: {string.Join(",", riverConnections)}.");
        }
    }

    private void ApplyRiverMaterials(GameObject riverInstance, Material baseMaterial)
    {
        if (riverInstance == null || baseMaterial == null)
        {
            Debug.LogWarning("ApplyRiverMaterials: River instance or base material is null!");
            return;
        }

        Renderer renderer = riverInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material[] materials = renderer.materials;

            if (materials.Length > 0)
            {
                // Replace the base material while keeping the river overlay material intact
                materials[0] = baseMaterial;
                renderer.materials = materials;

//                Debug.Log("ApplyRiverMaterials: Successfully applied base material to river model.");
            }
            else
            {
                Debug.LogWarning("ApplyRiverMaterials: River model has no materials.");
            }
        }
        else
        {
            Debug.LogWarning("ApplyRiverMaterials: No Renderer found on the river instance.");
        }
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
                materials[0] = baseMaterial;
            }
            else if (materials.Length >= 2 && overlayMaterial != null)
            {
                materials[0] = baseMaterial;
                materials[1] = overlayMaterial;
            }

            renderer.materials = materials;
        }
        else
        {
            Debug.LogWarning("HexMapRenderer: No Renderer found on the TileModel.");
        }
    }
}

