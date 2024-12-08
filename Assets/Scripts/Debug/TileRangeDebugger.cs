using System.Collections.Generic;
using UnityEngine;

public class TileRangeDebugger : MonoBehaviour
{
    [Header("Dependencies")]
    public HexGridDataManager GridManager; // Reference to the grid manager
    public GameObject DebugCubePrefab;   // Prefab for the debug cube

    [Header("Debugging Options")]
    public Vector2Int CenterTilePosition; // Center tile grid position
    [Min(0)] public int Range = 1;        // Range to debug (non-negative)

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
        // Validate dependencies
        if (GridManager == null)
        {
            Debug.LogError("GridManager is not assigned. Cannot debug range.");
            return;
        }

        if (DebugCubePrefab == null)
        {
            Debug.LogError("DebugCubePrefab is not assigned. Cannot instantiate debug cubes.");
            return;
        }

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
            Debug.LogError($"Center tile not found at the specified position: {CenterTilePosition}");
            return;
        }

        // Get the hexCells dictionary from GridManager
        Dictionary<Vector2, Tile> hexCells = GridManager.GetHexCells();
        if (hexCells == null)
        {
            Debug.LogError("HexGridDataManager did not return hex cells. Ensure grid is initialized.");
            return;
        }

        // Get the tiles in range
        List<Tile> tilesInRange = HexUtility.GetHexesInRange(centerTile, Range, hexCells);

        // Spawn debug cubes on the tiles
        foreach (Tile tile in tilesInRange)
        {
            GameObject debugCube = Instantiate(DebugCubePrefab, tile.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            spawnedCubes.Add(debugCube);
        }

        Debug.Log($"Debugged range: {tilesInRange.Count} tiles found.");
    }
}
