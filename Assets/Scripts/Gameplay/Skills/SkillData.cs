using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;          // Name of the skill
    [TextArea] public string description; // Description of the skill
    public int skillCost;             // Cost of using the skill (if applicable)
    public Sprite skillIcon;          // Icon for UI representation

    public int radius;
}

