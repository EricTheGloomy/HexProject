using System.Collections.Generic;
using UnityEngine;

public interface IMapGenerationStep
{
    void Generate(Dictionary<Vector2, Tile> tiles);
}
