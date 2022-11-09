using System.Collections.Generic;
using Puzzles;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Pipe : MonoBehaviour
{
    
    private EndPoint _connectedEndPoint;
    private StartPoint _connectedStartPoint;
    
    
    private EndPoint connectedEndPoint
    {
        get => _connectedEndPoint;
    }
    
    private StartPoint connectedStartPoint
    {
        get => _connectedStartPoint;
    }

    
    public void CheckIfPipeChanged()
    {
        if (WasPipeDisconnected())
        {
            GameObject.Destroy(gameObject);
            return;
        }
        
    }
    
    bool WasPipeDisconnected()
    {
        throw new System.NotImplementedException();
    }
}
