public class RiverNode
{
    public Tile Tile;
    public RiverNode Parent;
    public float GCost; // Cost to reach this node
    public float HCost; // Estimated cost to goal
    public float FCost => GCost + HCost; // Total cost
}