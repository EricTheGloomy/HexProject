// File: Scripts/Configs/RiverModelConfig.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "RiverModelConfig", menuName = "Game/RiverModelConfig")]
public class RiverModelConfig : ScriptableObject
{
    [System.Serializable]
    public class RiverModelPattern
    {
        public string Name;                 // Optional name for debugging
        public GameObject Model;            // The associated river model
        public bool[] Connections = new bool[6]; // Edge connection pattern
    }

    public RiverModelPattern RiverStart;    // River start (1 connection)
    public RiverModelPattern[] River2Way;    // Straight and curved rivers (gap=0, gap=1)
    public RiverModelPattern[] TJunctions;      // T-junctions
    public RiverModelPattern[] FourWayJunctions;// 4-way junctions
    public RiverModelPattern River5Way;         // 5-way junction
    public RiverModelPattern River6Way;         // Full river hex

    // Lookup the appropriate model based on river connections
    public GameObject GetRiverModel(bool[] riverConnections, out int rotationSteps)
    {
        rotationSteps = 0;
        GameObject model = null;

        int activeEdges = riverConnections.Count(c => c);

        if (activeEdges == 1) model = MatchPattern(RiverStart, riverConnections, out rotationSteps);
        if (activeEdges == 2){model = MatchPatterns(River2Way, riverConnections, out rotationSteps);}
        if (activeEdges == 3) model = MatchPatterns(TJunctions, riverConnections, out rotationSteps);
        if (activeEdges == 4) model = MatchPatterns(FourWayJunctions, riverConnections, out rotationSteps);
        if (activeEdges == 5) model = MatchPattern(River5Way, riverConnections, out rotationSteps);
        if (activeEdges == 6) model = MatchPattern(River6Way, riverConnections, out rotationSteps);

        if (model == null)
        {
            Debug.LogWarning($"No river model found for connections: {string.Join(",", riverConnections)} with {activeEdges} active edges.");
        }
        else
        {
//            Debug.Log($"Matched river model for connections: {string.Join(",", riverConnections)} with rotation steps: {rotationSteps}");
        }

        return model;
    }

    private GameObject MatchPattern(RiverModelPattern pattern, bool[] connections, out int rotationSteps)
    {
        rotationSteps = 0;
        if (pattern == null) return null;

        for (int i = 0; i < 6; i++)
        {
            if (connections.SequenceEqual(RotateConnections(pattern.Connections, i)))
            {
                rotationSteps = i;
                return pattern.Model;
            }
        }

        return null;
    }

    private GameObject MatchPatterns(RiverModelPattern[] patterns, bool[] connections, out int rotationSteps)
    {
        rotationSteps = 0;

        foreach (var pattern in patterns)
        {
            for (int i = 0; i < 6; i++)
            {
//                Debug.Log($"Trying pattern: {pattern.Name}, Rotation Steps: {i}, Connections: {string.Join(",", connections)}");

                if (connections.SequenceEqual(RotateConnections(pattern.Connections, i)))
                {
//                    Debug.Log($"Matched pattern {pattern.Name} with rotation {i * 60} degrees for connections: {string.Join(",", connections)}");
                    rotationSteps = i;
                    return pattern.Model;
                }
            }
        }

        Debug.LogWarning($"No pattern matched for connections: {string.Join(",", connections)}");
        return null;
    }

    private bool[] RotateConnections(bool[] connections, int steps)
    {
        bool[] rotated = new bool[6];
        for (int i = 0; i < 6; i++)
        {
            rotated[i] = connections[(i - steps + 6) % 6];
        }
        return rotated;
    }
}
