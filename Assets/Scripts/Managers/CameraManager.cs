using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour, ICameraManager
{
    [Header("References")]
    public Transform CameraAnchor;
    [SerializeField] private CameraConfig cameraConfig;
    [SerializeField] private MapConfig mapConfig;

    public Vector3 MinBounds;
    public Vector3 MaxBounds;

    private Dictionary<Vector2, Tile> allTiles;

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        ConstrainToBounds();
    }

    public void Initialize(Dictionary<Vector2, Tile> tiles, float tileWidth, float tileHeight)
    {
        allTiles = tiles;

        // Use config values for tile dimensions and grid orientation
        float hexWidth = tileWidth;
        float hexHeight = tileHeight;
        bool useFlatTop = mapConfig.useFlatTop;

        CalculateWorldBounds(hexWidth, hexHeight, useFlatTop);

        Tile startingTile = FindStartingTile();
        if (startingTile != null)
        {
            Vector3 startingPosition = HexCoordinateHelper.GetWorldPosition(
                startingTile.Attributes.OffsetCoordinates,
                useFlatTop,
                hexWidth,
                hexHeight
            );
            CameraAnchor.position = startingPosition;

            Debug.Log($"CameraManager: Camera positioned at starting tile {startingTile.Attributes.GridPosition}.");
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
            Vector2 offsetCoords = tile.Attributes.OffsetCoordinates;
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
            if (tile.Attributes.IsStartingLocation)
            {
                return tile;
            }
        }

        Debug.LogError("CameraManager: No tile marked as the starting location.");
        return null;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * cameraConfig.moveSpeed * Time.deltaTime;

        // Apply movement in local space (respects rotation of the CameraAnchor)
        CameraAnchor.Translate(movement, Space.Self);
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            CameraAnchor.Rotate(Vector3.up, -cameraConfig.rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            CameraAnchor.Rotate(Vector3.up, cameraConfig.rotationSpeed * Time.deltaTime);
        }
    }

    private void ConstrainToBounds()
    {
        // Clamp the position of the CameraAnchor to the map bounds
        Vector3 position = CameraAnchor.position;

        position.x = Mathf.Clamp(position.x, MinBounds.x, MaxBounds.x);
        position.z = Mathf.Clamp(position.z, MinBounds.z, MaxBounds.z);

        CameraAnchor.position = position;
    }
}
