using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUpdateState : IGameplayStateHandler
{
    private GameplayManager manager;

    public MapUpdateState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public void EnterState()
    {
        Debug.Log("MapUpdateState: Updating the map...");
        // TODO: Spawn events, update tiles, etc.

        // Transition to preparing for the next turn
        manager.SetState(GameplayState.TransitionToNextTurn);
    }

    public void ExitState()
    {
        Debug.Log("MapUpdateState: Finished map updates.");
    }
}
