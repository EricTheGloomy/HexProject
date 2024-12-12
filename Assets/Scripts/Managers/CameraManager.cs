using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour, ICameraManager
{
    [Header("References")]
    public Transform CameraAnchor; // The empty GameObject the camera follows
    [SerializeField] private CameraConfig cameraConfig; // Reference to the ScriptableObject

    public Vector3 MinBounds; // Minimum map bounds
    public Vector3 MaxBounds; // Maximum map bounds

    private Dictionary<Vector2, Tile> allTiles; // Dictionary of all tiles

    public void Initialize(Dictionary<Vector2, Tile> tiles, float tileWidth, float tileHeight)
    {
        allTiles = tiles;

        // Use config values for tile dimensions and grid orientation
        float hexWidth = tileWidth;
        float hexHeight = tileHeight;
        bool useFlatTop = cameraConfig.useFlatTop;

        // Calculate world bounds based on all tiles
        CalculateWorldBounds(hexWidth, hexHeight, useFlatTop);

        // Find the starting location
        Tile startingTile = FindStartingTile();
        if (startingTile != null)
        {
            // Move CameraAnchor to the starting tile position in world space
            Vector3 startingPosition = HexCoordinateHelper.GetWorldPosition(
                startingTile.OffsetCoordinates,
                useFlatTop,
                hexWidth,
                hexHeight
            );
            CameraAnchor.position = startingPosition;

            Debug.Log($"CameraManager: Camera positioned at starting tile {startingTile.GridPosition}.");
        }
        else
        {
            Debug.LogError("CameraManager: No starting tile found!");
        }
    }

    private void CalculateWorldBounds(float tileWidth, float tileHeight, bool useFlatTop)
    {
        Vector2 minCoords = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxCoords = new Vector2(float.MinValue, float.MinValue);

        // Find min and max offset coordinates
        foreach (var tile in allTiles.Values)
        {
            Vector2 offsetCoords = tile.OffsetCoordinates;
            if (offsetCoords.x < minCoords.x) minCoords.x = offsetCoords.x;
            if (offsetCoords.y < minCoords.y) minCoords.y = offsetCoords.y;
            if (offsetCoords.x > maxCoords.x) maxCoords.x = offsetCoords.x;
            if (offsetCoords.y > maxCoords.y) maxCoords.y = offsetCoords.y;
        }

        // Convert offset coordinates to world coordinates
        MinBounds = HexCoordinateHelper.GetWorldPosition(minCoords, useFlatTop, tileWidth, tileHeight);
        MaxBounds = HexCoordinateHelper.GetWorldPosition(maxCoords, useFlatTop, tileWidth, tileHeight);

        Debug.Log($"CameraManager: World bounds calculated - Min: {MinBounds}, Max: {MaxBounds}");
    }

    private Tile FindStartingTile()
    {
        foreach (var tile in allTiles.Values)
        {
            if (tile.IsStartingLocation)
            {
                return tile;
            }
        }

        Debug.LogError("CameraManager: No tile marked as the starting location.");
        return null;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        ConstrainToBounds();
    }

    private void HandleMovement()
    {
        // WASD or Arrow keys for movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down

        // Calculate movement direction and apply speed
        Vector3 movement = new Vector3(horizontal, 0, vertical) * cameraConfig.moveSpeed * Time.deltaTime;

        // Apply movement in local space (respects rotation of the CameraAnchor)
        CameraAnchor.Translate(movement, Space.Self);
    }

    private void HandleRotation()
    {
        // Q/E for rotation
        if (Input.GetKey(KeyCode.Q))
        {
            CameraAnchor.Rotate(Vector3.up, -cameraConfig.rotationSpeed * Time.deltaTime); // Counter-clockwise
        }
        else if (Input.GetKey(KeyCode.E))
        {
            CameraAnchor.Rotate(Vector3.up, cameraConfig.rotationSpeed * Time.deltaTime); // Clockwise
        }
    }

    private void ConstrainToBounds()
    {
        // Clamp the position of the CameraAnchor to the map bounds
        Vector3 position = CameraAnchor.position;

        position.x = Mathf.Clamp(position.x, MinBounds.x, MaxBounds.x); // Horizontal (X axis)
        position.z = Mathf.Clamp(position.z, MinBounds.z, MaxBounds.z); // Vertical (Z axis)

        CameraAnchor.position = position;
    }
}
