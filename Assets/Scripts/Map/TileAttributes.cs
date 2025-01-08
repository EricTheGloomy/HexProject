using UnityEngine;

[System.Serializable]
public class TileAttributes
{
    [Header("Grid Data")]
    public Vector2 GridPosition;
    public Vector2 OffsetCoordinates;
    public Vector3 CubeCoordinates;

    [Header("Procedural Attributes")]
    [SerializeField]
    private ProceduralTileAttributes procedural = new ProceduralTileAttributes();
    public ProceduralTileAttributes Procedural => procedural;

    [Header("Gameplay Attributes")]
    [SerializeField]
    private GameplayTileAttributes gameplay = new GameplayTileAttributes();
    public GameplayTileAttributes Gameplay => gameplay;

    [Header("Terrain Data")]
    public string TerrainType;
    public TileTypeData TileTypeData;

    [Header("Visibility")]
    public VisibilityState Visibility;
}
