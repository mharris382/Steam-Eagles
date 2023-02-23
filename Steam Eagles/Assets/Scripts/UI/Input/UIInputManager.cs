using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Players;
using UniRx;
using UnityEngine;

public class UIInputManager : MonoBehaviour
{
    public List<Player> players;
    

    public void Awake()
    {
        MessageBroker.Default.Receive<PlayerJoinedInfo>().Subscribe(OnPlayerJoined).AddTo(this);
    }
    
 
    void OnPlayerJoined(PlayerJoinedInfo info)
    {
        
    }
}
