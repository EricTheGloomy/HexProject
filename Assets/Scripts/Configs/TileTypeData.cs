using UnityEngine;

[CreateAssetMenu(fileName = "TileTypeData", menuName = "Game/TileTypeData")]
public class TileTypeData : ScriptableObject
{
    public string Name;                    // Name of the tile type
    [TextArea] public string Description;  // Description of the tile
    public GameObject TileModel;           // Model associated with this tile type
    public GameObject FogOverlay;          // Fog of war model for this tile type
    public Material BaseMaterial;          // Base material for the terrain
    public Material OverlayMaterial;       // Material for rivers, roads, etc.
    public bool isEligibleForStart;        // Can the tile type be chosen as a starting location
    
    [Header("Housing Decorations")]
    public GameObject CityHousingDecoration;
    public GameObject TownHousingDecoration;
    public GameObject VillageHousingDecoration;
    public GameObject HamletHousingDecoration;

}
