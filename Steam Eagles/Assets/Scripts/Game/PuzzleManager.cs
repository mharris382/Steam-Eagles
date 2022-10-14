using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
 
}


[CreateAssetMenu(menuName = "Game/Puzzle Data", fileName = "New Puzzle")]
public class Puzzle : ScriptableObject
{
    [SerializeField, Multiline(5)]
    public string description;
    
    
}