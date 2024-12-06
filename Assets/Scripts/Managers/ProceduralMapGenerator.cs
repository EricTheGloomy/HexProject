// File: Scripts/Managers/ProceduralMapGenerator.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour
{
    public int MapWidth;
    public int MapHeight;
    public float NoiseScale;
    public TileType[] TileTypes;

    public static event Action<Dictionary<Vector2Int, TileType>> OnMapGenerated;

    private void OnEnable()
    {
        GameManager.OnGameReady += GenerateMap;
    }

    private void OnDisable()
    {
        GameManager.OnGameReady -= GenerateMap;
    }

    private void GenerateMap()
    {
        Dictionary<Vector2Int, TileType> mapData = new Dictionary<Vector2Int, TileType>();

        for (int row = 0; row < MapHeight; row++)
        {
            for (int col = 0; col < MapWidth; col++)
            {
                // Generate Perlin noise value
                float perlinValue = Mathf.PerlinNoise(col / NoiseScale, row / NoiseScale);

                // Assign TileType based on the noise value
                TileType tileType = GetTileTypeFromNoise(perlinValue);
                mapData[new Vector2Int(col, row)] = tileType;
            }
        }

        Debug.Log("Map generation complete!");
        OnMapGenerated?.Invoke(mapData); // Notify subscribers
    }

    private TileType GetTileTypeFromNoise(float noiseValue)
    {
        if (TileTypes == null || TileTypes.Length == 0)
            throw new InvalidOperationException("TileTypes array is empty!");

        // Simple logic to map noise to TileType
        if (noiseValue < 0.3f)
            return TileTypes[0]; // E.g., Water
        if (noiseValue < 0.6f)
            return TileTypes[1]; // E.g., Grassland
        return TileTypes[2];     // E.g., Desert
    }
}
