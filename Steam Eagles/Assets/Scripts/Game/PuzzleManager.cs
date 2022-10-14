using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

public class PuzzleManager : MonoBehaviour
{
 
}


public abstract class Puzzle : MonoBehaviour
{
    
    [SerializeField, Multiline(5)]
    public string description;


    private int _totalAttempts;
    
    /// <summary>number of times the puzzle has been restarted </summary>
    public int TotalAttempts => _totalAttempts;


    public abstract bool IsSolved
    {
        get;
    }

    /// <summary>
    /// called the first time the puzzle is started by players
    /// </summary>
    public abstract void PuzzleStartup();
    

    protected abstract void StartPuzzle();
    protected abstract void StopPuzzle(PuzzleResult result);
    
    public enum PuzzleResult
    {
        COMPLETED,
        CANCELED,
        FAILED
    }
    
}