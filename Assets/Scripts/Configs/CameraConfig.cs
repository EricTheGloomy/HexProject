using UnityEngine;

[CreateAssetMenu(fileName = "CameraConfig", menuName = "Game/CameraConfig")]
public class CameraConfig : ScriptableObject
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
}
