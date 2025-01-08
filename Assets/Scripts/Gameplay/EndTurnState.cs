using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnState : IGameplayStateHandler
{
    private GameplayManager manager;

    public EndTurnState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public void EnterState()
    {
        Debug.Log("EndTurnState: Performing end-turn calculations...");
        // TODO: Add calculations for map events (e.g., determining next turn's spawns)

        // Transition to map updates after calculations
        manager.SetState(GameplayState.MapUpdate);
    }

    public void ExitState()
    {
        Debug.Log("EndTurnState: Finished calculations.");
    }
}
