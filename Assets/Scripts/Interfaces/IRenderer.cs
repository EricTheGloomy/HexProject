using System.Collections.Generic;
using UnityEngine;

public interface IRenderer
{
    void RenderMap(Dictionary<Vector2, Tile> hexTiles);
}
