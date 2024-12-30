using System.Collections.Generic;
using UnityEngine;

public interface IFogOfWarManager
{
    void Initialize(Dictionary<Vector2, Tile> tiles);
    void RevealAreaAroundTile(Tile centerTile, int radius);
    VisibilityState GetFogState(Tile tile);
}
