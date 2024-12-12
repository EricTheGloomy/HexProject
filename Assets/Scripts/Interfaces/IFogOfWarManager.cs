using System.Collections.Generic;
using UnityEngine;

public interface IFogOfWarManager
{
    void Initialize(Dictionary<Vector2, Tile> tiles); // Initialize the fog state for all tiles
    void RevealAreaAroundTile(Tile centerTile); // Reveal tiles within a radius
    VisibilityState GetFogState(Tile tile); // Get the fog state of a specific tile
}
