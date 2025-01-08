using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    [Header("Available Skills")]
    public List<SkillData> availableSkills; // List of all unlocked skills
    private List<ITileSkill> skillInstances; // Runtime instances of skills

    [Header("References")]
    public FogOfWarManager fogOfWarManager; // Reference to the FogOfWarManager

    public Tile selectedTile; // Track the currently selected tile

    private void OnEnable()
    {
        TileInteractionManager.OnTileSelected += HandleTileSelected;
        TileInteractionManager.OnTileDeselected += HandleTileDeselected;
    }

    private void OnDisable()
    {
        TileInteractionManager.OnTileSelected -= HandleTileSelected;
        TileInteractionManager.OnTileDeselected -= HandleTileDeselected;
    }

    private void HandleTileSelected(Tile tile)
    {
        selectedTile = tile;
        Debug.Log($"SkillsManager: Tile selected at {tile.Attributes.GridPosition}");
    }

    private void HandleTileDeselected()
    {
        selectedTile = null;
        Debug.Log("SkillsManager: Tile deselected.");
    }

    private void Awake()
    {
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        skillInstances = new List<ITileSkill>();

        foreach (var skillData in availableSkills)
        {
            if (skillData.skillName == "Exploration")
            {
                skillInstances.Add(new ExplorationSkill(skillData, fogOfWarManager));
            }
            // Add logic for other skills here.
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Example for Exploration Skill
        {
            if (selectedTile != null)
            {
                // Use the first available skill for testing (change as needed)
                if (availableSkills.Count > 0)
                {
                    UseSkill(availableSkills[0], selectedTile);
                }
            }
            else
            {
                Debug.LogWarning("No tile selected for skill usage.");
            }
        }
    }

    public void UseSkill(SkillData skillData, Tile targetTile)
    {
        var skill = skillInstances.Find(s =>
        {
            if (s is ExplorationSkill explorationSkill)
            {
                return explorationSkill.skillData == skillData;
            }
            return false;
        });

        if (skill == null)
        {
            Debug.LogWarning($"Skill {skillData.skillName} is not available.");
            return;
        }

        if (!skill.CanUse(targetTile))
        {
            Debug.LogWarning($"Skill {skillData.skillName} cannot be used on the target tile.");
            return;
        }

        skill.Use(targetTile);
    }
}

