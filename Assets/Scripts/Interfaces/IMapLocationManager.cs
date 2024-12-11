using System.Collections.Generic;
using UnityEngine;

public interface IMapLocationManager
{
    void AssignLocations(Dictionary<Vector2, Tile> grid);
    Tile GetStartingTile();
}
