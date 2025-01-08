using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorationSkill : ITileSkill
{
    public readonly SkillData skillData;
    private readonly FogOfWarManager fogOfWarManager;

    public ExplorationSkill(SkillData skillData, FogOfWarManager fogOfWarManager)
    {
        this.skillData = skillData;
        this.fogOfWarManager = fogOfWarManager;
    }

    public bool CanUse(Tile targetTile)
    {
        if (targetTile == null) return false;
/*
        var neighbors = targetTile.Neighbors;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.Attributes.Visibility == VisibilityState.Hidden)
            {
                return true;
            }
        }

        return false;*/
        return true;
    }

    public void Use(Tile targetTile)
    {
        if (targetTile == null) return;

        int radius = skillData.radius; // Set radius for fog reveal
        fogOfWarManager.RevealAreaAroundTile(targetTile, radius);

        Debug.Log($"Used {skillData.skillName} skill on tile at {targetTile.Attributes.GridPosition}.");
    }
}
