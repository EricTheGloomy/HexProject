using System.Collections.Generic;
using UnityEngine;

public class TileRangeDebugger : MonoBehaviour
{
    public HexGridDataManager GridManager; // Reference to the grid manager
    public Vector2Int CenterTilePosition; // Center tile grid position
    public int Range = 1;                 // Range to debug
    public GameObject DebugCubePrefab;   // Prefab for the debug cube

    private List<GameObject> spawnedCubes = new List<GameObject>();

    void Update()
    {
        // Check for spacebar input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugRange();
        }
    }

    private void DebugRange()
    {
        // Clear previously spawned cubes
        foreach (GameObject cube in spawnedCubes)
        {
            Destroy(cube);
        }
        spawnedCubes.Clear();

        // Get the center tile
        Tile centerTile = GridManager.GetTileAtPosition(CenterTilePosition);
        if (centerTile == null)
        {
            Debug.LogError("Center tile not found at the specified position.");
            return;
        }

        // Get the tiles in range
        List<Tile> tilesInRange = GridManager.GetHexesInRange(centerTile, Range);

        // Spawn debug cubes on the tiles
        foreach (Tile tile in tilesInRange)
        {
            GameObject debugCube = Instantiate(DebugCubePrefab, tile.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            spawnedCubes.Add(debugCube);
        }

        Debug.Log($"Debugged range: {tilesInRange.Count} tiles found.");
    }
}
