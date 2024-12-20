[System.Serializable]
public class GameplayTileAttributes
{
    public int Population;
    public float Happiness;

    public bool HasHousing;
    public bool IsExtremeSettlement;
    public bool HasVegetation;
    public bool HasResources;
    public bool HasRoads;
    public bool HasRiver;
    public bool[] RiverConnections = new bool[6];

    // Add SettlementType
    public SettlementType SettlementType; // Type of settlement for housing
}
