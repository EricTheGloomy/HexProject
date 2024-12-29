using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayStateHandler
{
    void EnterState();
    void ExitState();
}
