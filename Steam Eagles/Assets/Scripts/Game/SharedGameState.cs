using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

[CreateAssetMenu(menuName = "Shared Variables/Shared Game State", fileName = "Game State", order = -1)]
public class SharedGameState : SharedValue<GameState>
{
   
}

public enum GameState
{
    PUZZLE,
    STORY
}
