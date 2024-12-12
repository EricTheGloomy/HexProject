using System.Collections.Generic;
using UnityEngine;

public interface ICameraManager
{
    void Initialize(Dictionary<Vector2, Tile> tiles, float tileWidth, float tileHeight); // Initialize with tiles and tile size
}
