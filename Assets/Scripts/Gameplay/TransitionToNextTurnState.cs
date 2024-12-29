using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToNextTurnState : IGameplayStateHandler
{
    private GameplayManager manager;

    public TransitionToNextTurnState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public void EnterState()
    {
        Debug.Log("TransitionToNextTurnState: Preparing for the next turn...");
        // TODO: Reset any temporary data, prepare UI, etc.

        // Transition back to the player's turn
        manager.SetState(GameplayState.PlayerTurn);
    }

    public void ExitState()
    {
        Debug.Log("TransitionToNextTurnState: Ready for next turn.");
    }
}

