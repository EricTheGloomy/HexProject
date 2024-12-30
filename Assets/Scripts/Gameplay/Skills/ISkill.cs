using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileSkill
{
    bool CanUse(Tile targetTile); // Check if the skill can be used on the specified target
    void Use(Tile targetTile);    // Execute the skill's effect
}