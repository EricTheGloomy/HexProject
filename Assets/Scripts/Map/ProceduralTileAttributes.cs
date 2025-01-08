[System.Serializable]
public class ProceduralTileAttributes
{
    public float Elevation;
    public float Moisture;
    public float Temperature;

    public TileTypeData TileTypeData; //remove??? unused now

    public TileTypeDataMappingConfig.ElevationCategory FixedElevationCategory;
}
