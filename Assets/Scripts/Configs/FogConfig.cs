using UnityEngine;

[CreateAssetMenu(fileName = "FogConfig", menuName = "Game/FogConfig")]
public class FogConfig : ScriptableObject
{
    [Header("Fog of War Settings")]
    [Tooltip("Radius of tiles to reveal around the starting location.")]
    public int RevealRadius = 4;
}
