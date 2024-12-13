using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisibilityState
{
    Hidden,    // Completely covered by fog
    Explored,  // Revealed but not currently visible
    Visible    // Actively visible
}
