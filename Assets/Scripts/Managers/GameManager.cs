// File: Scripts/Managers/GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void GameReadyEventHandler();
    public static event GameReadyEventHandler OnGameReady;

    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        OnGameReady?.Invoke();
        Debug.Log("Game initialized!");
    }
}
