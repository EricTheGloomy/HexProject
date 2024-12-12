using UnityEngine;

[CreateAssetMenu(fileName = "CameraConfig", menuName = "Game/CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Default movement speed
    public float rotationSpeed = 100f; // Default rotation speed

    [Header("Grid Settings")]
    public bool useFlatTop = false; // False = pointy-top hexes
}
